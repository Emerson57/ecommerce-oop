using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Common;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.Events;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.Rules;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Domain.Entities.Orders;

/// <summary>
/// Representa un pedido dentro del dominio del e-commerce.
/// </summary>
/// <remarks>
/// Un pedido materializa la intención de compra de un cliente y consolida
/// las líneas comerciales confirmadas a partir de un carrito de compras.
/// Esta entidad centraliza el ciclo de vida del pedido, el control de sus
/// estados operativos, la trazabilidad temporal y el cálculo del total
/// económico de la transacción.
/// 
/// La entidad se apoya en reglas de negocio reutilizables para expresar
/// decisiones del dominio de manera más limpia, consistente y mantenible.
/// 
/// Adicionalmente, registra eventos de dominio cuando ocurren hechos relevantes
/// del negocio, como la creación del pedido, el registro de pago y la cancelación.
/// </remarks>
public sealed class Pedido : AggregateRoot
{
    #region Campos privados

    /// <summary>
    /// Colección interna de detalles del pedido.
    /// </summary>
    private readonly List<DetallePedido> _detalles = new();

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor privado sin parámetros requerido por herramientas de persistencia como EF Core.
    /// </summary>
    private Pedido()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="Pedido"/> para un cliente específico.
    /// </summary>
    /// <param name="clienteId">Identificador del cliente propietario del pedido.</param>
    public Pedido(Guid clienteId)
    {
        if (clienteId == Guid.Empty)
        {
            throw new DomainException("El identificador del cliente del pedido no puede ser vacío.");
        }

        InicializarPedido(clienteId);

        AddDomainEvent(new PedidoCreadoEvent(this));
    }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="Pedido"/> a partir de un carrito de compras.
    /// </summary>
    /// <param name="carritoCompra">Carrito que servirá como origen del pedido.</param>
    public Pedido(CarritoCompra carritoCompra)
    {
        ArgumentNullException.ThrowIfNull(carritoCompra);

        carritoCompra.ValidarQueTengaItems();

        InicializarPedido(carritoCompra.ClienteId);

        if (carritoCompra.CantidadItems > DomainLimits.MaximoDetallesPorPedido)
        {
            throw new DomainException($"El pedido no puede superar {DomainLimits.MaximoDetallesPorPedido} líneas.");
        }

        foreach (ItemCarrito item in carritoCompra.Items)
        {
            AgregarDetalleInterno(new DetallePedido(Id, item));
        }

        if (_detalles.Count == 0)
        {
            throw new CarritoVacioException();
        }

        AddDomainEvent(new PedidoCreadoEvent(this));
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Identificador del cliente propietario del pedido.
    /// </summary>
    public Guid ClienteId { get; private set; }

    /// <summary>
    /// Estado actual del ciclo de vida del pedido.
    /// </summary>
    public EstadoPedido Estado { get; private set; }

    /// <summary>
    /// Colección de detalles del pedido en modo de solo lectura.
    /// </summary>
    public IReadOnlyCollection<DetallePedido> Detalles => _detalles.AsReadOnly();

    /// <summary>
    /// Cantidad total de líneas registradas en el pedido.
    /// </summary>
    public int CantidadDetalles => _detalles.Count;

    /// <summary>
    /// Cantidad total de unidades compradas entre todas las líneas del pedido.
    /// </summary>
    public int CantidadTotalUnidades => _detalles.Sum(d => d.Cantidad);

    /// <summary>
    /// Total monetario del pedido calculado a partir de sus detalles.
    /// </summary>
    /// <remarks>
    /// Cuando el pedido no contiene detalles, se devuelve un valor monetario cero
    /// utilizando la moneda por defecto del dominio.
    /// </remarks>
    public Money Total
    {
        get
        {
            if (_detalles.Count == 0)
            {
                return Money.Zero(DomainDefaults.DefaultCurrency);
            }

            string moneda = _detalles[0].PrecioUnitario.Currency;
            Money total = Money.Zero(moneda);

            foreach (DetallePedido detalle in _detalles)
            {
                ValidarConsistenciaMonetaria(detalle.PrecioUnitario);
                total += detalle.Subtotal;
            }

            return total;
        }
    }

    /// <summary>
    /// Fecha y hora UTC en que el pedido fue confirmado.
    /// </summary>
    public DateTime? FechaConfirmacionUtc { get; private set; }

    /// <summary>
    /// Fecha y hora UTC en que el pago del pedido fue registrado como exitoso.
    /// </summary>
    public DateTime? FechaPagoUtc { get; private set; }

    /// <summary>
    /// Fecha y hora UTC en que el pedido fue enviado o despachado.
    /// </summary>
    public DateTime? FechaEnvioUtc { get; private set; }

    /// <summary>
    /// Fecha y hora UTC en que el pedido fue entregado o completado.
    /// </summary>
    public DateTime? FechaEntregaUtc { get; private set; }

    /// <summary>
    /// Fecha y hora UTC en que el pedido fue cancelado.
    /// </summary>
    public DateTime? FechaCancelacionUtc { get; private set; }

    /// <summary>
    /// Observación o motivo de cancelación registrado para el pedido.
    /// </summary>
    public string? ObservacionCancelacion { get; private set; }

    /// <summary>
    /// Dirección de envío asociada al pedido cuando aplica.
    /// </summary>
    public DireccionEnvio? DireccionEnvio { get; private set; }

    #endregion

    #region Métodos de negocio

    /// <summary>
    /// Agrega un detalle al pedido antes de su confirmación formal.
    /// </summary>
    /// <param name="detallePedido">Detalle que será incorporado al pedido.</param>
    public void AgregarDetalle(DetallePedido detallePedido)
    {
        ArgumentNullException.ThrowIfNull(detallePedido);

        ValidarQuePermitaEdicion();

        if (detallePedido.PedidoId != Id)
        {
            throw new DomainException("No es posible agregar un detalle cuyo pedido asociado no corresponde al pedido actual.");
        }

        AgregarDetalleInterno(detallePedido);
        MarcarActualizacion();
    }

    /// <summary>
    /// Confirma el pedido y lo deja listo para avanzar en el flujo comercial.
    /// </summary>
    public void Confirmar()
    {
        ValidarEstadoActual("confirmar el pedido", EstadoPedido.Pendiente);
        ValidarQueTengaDetalles();

        CambiarEstado(EstadoPedido.Confirmado);
        FechaConfirmacionUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca el pedido como pagado.
    /// </summary>
    public void RegistrarPago()
    {
        if (Estado != EstadoPedido.Confirmado)
        {
            throw new PagoFallidoException(Id, $"No es posible registrar el pago porque el pedido se encuentra en estado '{Estado}'.");
        }

        CambiarEstado(EstadoPedido.Pagado);
        FechaPagoUtc = DateTime.UtcNow;

        AddDomainEvent(new PedidoPagadoEvent(this));
    }

    /// <summary>
    /// Marca el pedido como en proceso operativo.
    /// </summary>
    public void MarcarEnProceso()
    {
        ValidarEstadoActual($"pasar el pedido a estado '{EstadoPedido.EnProceso}'", EstadoPedido.Pagado);
        ValidarQueTengaDetalles();

        CambiarEstado(EstadoPedido.EnProceso);
    }

    /// <summary>
    /// Marca el pedido como enviado o despachado.
    /// </summary>
    public void MarcarEnviado()
    {
        ValidarEstadoActual("enviar el pedido", EstadoPedido.EnProceso);
        ValidarQueTengaDetalles();
        ValidarDireccionEnvioParaEnvio();

        CambiarEstado(EstadoPedido.Enviado);
        FechaEnvioUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca el pedido como entregado o completado satisfactoriamente.
    /// </summary>
    public void MarcarEntregado()
    {
        ValidarEstadoActual("marcar como entregado el pedido", EstadoPedido.Enviado);
        ValidarQueTengaDetalles();

        CambiarEstado(EstadoPedido.Entregado);
        FechaEntregaUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancela el pedido registrando el motivo correspondiente.
    /// </summary>
    /// <param name="motivo">Motivo funcional de la cancelación.</param>
    public void Cancelar(string motivo)
    {
        if (Estado == EstadoPedido.Cancelado)
        {
            return;
        }

        if (!PedidoCancelableRule.IsSatisfiedBy(this))
        {
            throw new DomainException($"No es posible cancelar el pedido porque su estado actual es '{Estado}'.");
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new DomainException("El motivo de cancelación del pedido es obligatorio.");
        }

        Estado = EstadoPedido.Cancelado;
        FechaCancelacionUtc = DateTime.UtcNow;
        ObservacionCancelacion = motivo.Trim();
        MarcarActualizacion();

        AddDomainEvent(new PedidoCanceladoEvent(this));
    }

    /// <summary>
    /// Determina si el pedido se encuentra en un estado final.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si el pedido ya fue entregado o cancelado;
    /// de lo contrario, <see langword="false"/>.
    /// </returns>
    public bool EstaFinalizado()
    {
        return Estado is EstadoPedido.Entregado or EstadoPedido.Cancelado;
    }

    /// <summary>
    /// Determina si el pedido tiene al menos una línea registrada.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si el pedido contiene detalles;
    /// de lo contrario, <see langword="false"/>.
    /// </returns>
    public bool TieneDetalles()
    {
        return _detalles.Count > 0;
    }

    /// <summary>
    /// Obtiene un detalle del pedido a partir del identificador del producto.
    /// </summary>
    /// <param name="productoId">Identificador del producto buscado.</param>
    /// <returns>Detalle encontrado o <see langword="null"/> si no existe coincidencia.</returns>
    public DetallePedido? ObtenerDetallePorProductoId(Guid productoId)
    {
        if (productoId == Guid.Empty)
        {
            return null;
        }

        return _detalles.FirstOrDefault(d => d.CorrespondeAProducto(productoId));
    }

    /// <summary>
    /// Asigna o reemplaza la dirección de envío del pedido.
    /// </summary>
    /// <param name="direccionEnvio">Dirección de envío a asociar.</param>
    public void AsignarDireccionEnvio(DireccionEnvio direccionEnvio)
    {
        ArgumentNullException.ThrowIfNull(direccionEnvio);

        DireccionEnvio = direccionEnvio;
        MarcarActualizacion();
    }

    /// <summary>
    /// Elimina la dirección de envío asociada al pedido.
    /// </summary>
    public void QuitarDireccionEnvio()
    {
        if (DireccionEnvio is null)
        {
            return;
        }

        DireccionEnvio = null;
        MarcarActualizacion();
    }

    /// <summary>
    /// Indica si el pedido tiene una dirección de envío asociada.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si el pedido tiene dirección de envío;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool TieneDireccionEnvio()
    {
        return DireccionEnvio is not null;
    }

    /// <summary>
    /// Indica si el pedido contiene al menos una línea de producto físico.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si el pedido contiene productos físicos;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool ContieneProductosFisicos()
    {
        return _detalles.Any(detalle => detalle.TipoProducto == TipoProducto.Fisico);
    }

    /// <summary>
    /// Indica si el pedido contiene al menos una línea de producto digital.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si el pedido contiene productos digitales;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool ContieneProductosDigitales()
    {
        return _detalles.Any(detalle => detalle.TipoProducto == TipoProducto.Digital);
    }

    #endregion

    #region Métodos privados auxiliares

    /// <summary>
    /// Valida que el pedido tenga al menos un detalle antes de continuar con el flujo comercial.
    /// </summary>
    private void ValidarQueTengaDetalles()
    {
        if (_detalles.Count == 0)
        {
            throw new DomainException("No es posible operar el pedido porque no contiene detalles.");
        }
    }

    /// <summary>
    /// Valida que exista dirección de envío cuando el pedido contiene productos físicos.
    /// </summary>
    private void ValidarDireccionEnvioParaEnvio()
    {
        if (ContieneProductosFisicos() && DireccionEnvio is null)
        {
            throw new DomainException("No es posible despachar un pedido con productos físicos sin una dirección de envío asociada.");
        }
    }

    /// <summary>
    /// Inicializa el estado base del pedido al momento de su creación.
    /// </summary>
    /// <param name="clienteId">Identificador del cliente propietario.</param>
    private void InicializarPedido(Guid clienteId)
    {
        InicializarAggregateRoot();
        ClienteId = clienteId;
        Estado = EstadoPedido.Pendiente;
        FechaConfirmacionUtc = null;
        FechaPagoUtc = null;
        FechaEnvioUtc = null;
        FechaEntregaUtc = null;
        FechaCancelacionUtc = null;
        ObservacionCancelacion = null;
        DireccionEnvio = null;
    }

    /// <summary>
    /// Incorpora un detalle al pedido preservando límites, unicidad y consistencia monetaria.
    /// </summary>
    /// <param name="detallePedido">Detalle a incorporar al agregado.</param>
    private void AgregarDetalleInterno(DetallePedido detallePedido)
    {
        if (_detalles.Count >= DomainLimits.MaximoDetallesPorPedido)
        {
            throw new DomainException($"El pedido no puede superar {DomainLimits.MaximoDetallesPorPedido} líneas.");
        }

        if (_detalles.Any(d => d.ProductoId == detallePedido.ProductoId))
        {
            throw new DomainException("No es posible registrar dos líneas separadas para el mismo producto dentro del mismo pedido.");
        }

        ValidarConsistenciaMonetaria(detallePedido.PrecioUnitario);
        _detalles.Add(detallePedido);
    }

    /// <summary>
    /// Valida que la moneda del detalle sea consistente con el resto del pedido.
    /// </summary>
    /// <param name="valorMonetario">Valor monetario a validar.</param>
    private void ValidarConsistenciaMonetaria(Money valorMonetario)
    {
        ArgumentNullException.ThrowIfNull(valorMonetario);

        string? monedaReferencia = _detalles.Count == 0
            ? null
            : _detalles[0].PrecioUnitario.Currency;

        if (!MonedaConsistenteRule.IsSatisfiedBy(monedaReferencia, valorMonetario))
        {
            throw new DomainException(
                $"No es posible operar el pedido con detalles en monedas distintas. Moneda esperada: '{monedaReferencia}', moneda recibida: '{valorMonetario.Currency}'.");
        }
    }

    /// <summary>
    /// Actualiza el estado del pedido y registra la modificación correspondiente.
    /// </summary>
    /// <param name="nuevoEstado">Nuevo estado del pedido.</param>
    private void CambiarEstado(EstadoPedido nuevoEstado)
    {
        Estado = nuevoEstado;
        MarcarActualizacion();
    }

    /// <summary>
    /// Valida que el estado actual del pedido permita modificación de sus detalles.
    /// </summary>
    private void ValidarQuePermitaEdicion()
    {
        if (Estado != EstadoPedido.Pendiente)
        {
            throw new DomainException($"No es posible modificar los detalles del pedido cuando se encuentra en estado '{Estado}'.");
        }
    }

    /// <summary>
    /// Valida que el estado actual del pedido permita ejecutar una operación determinada.
    /// </summary>
    /// <param name="operacion">Descripción funcional de la operación.</param>
    /// <param name="estadosPermitidos">Estados desde los cuales la operación es válida.</param>
    private void ValidarEstadoActual(string operacion, params EstadoPedido[] estadosPermitidos)
    {
        if (estadosPermitidos.Contains(Estado))
        {
            return;
        }

        throw new DomainException($"No es posible {operacion} porque el pedido se encuentra en estado '{Estado}'.");
    }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del pedido para trazabilidad y depuración.
    /// </summary>
    /// <returns>Cadena representativa del pedido.</returns>
    public override string ToString()
    {
        return $"Pedido: {Id} | Cliente: {ClienteId} | Estado: {Estado} | Líneas: {CantidadDetalles} | Total: {Total}";
    }

    #endregion
}