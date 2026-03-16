using PlataformaECommerce.Domain.Entities.Cart;
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
public sealed class Pedido
{
    #region Constantes de negocio

    /// <summary>
    /// Cantidad máxima de líneas permitidas dentro de un pedido.
    /// </summary>
    private const int MaximoDetallesPermitidos = 100;

    /// <summary>
    /// Moneda por defecto utilizada por el pedido cuando aún no existen detalles.
    /// </summary>
    private const string MonedaPorDefecto = "COP";

    #endregion

    #region Campos privados

    /// <summary>
    /// Colección interna de detalles del pedido.
    /// </summary>
    private readonly List<DetallePedido> _detalles = new();

    /// <summary>
    /// Colección interna de eventos de dominio generados por la entidad.
    /// </summary>
    private readonly List<DomainEvent> _domainEvents = new();

    #endregion

    #region Reglas de negocio

    /// <summary>
    /// Regla reutilizable para determinar si un pedido puede ser cancelado.
    /// </summary>
    private static readonly PedidoCancelableRule PedidoCancelableRule = new();

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

        Id = Guid.NewGuid();
        ClienteId = clienteId;
        Estado = EstadoPedido.Pendiente;
        FechaCreacionUtc = DateTime.UtcNow;
        FechaActualizacionUtc = null;
        FechaConfirmacionUtc = null;
        FechaPagoUtc = null;
        FechaEnvioUtc = null;
        FechaEntregaUtc = null;
        FechaCancelacionUtc = null;
        ObservacionCancelacion = null;

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

        Id = Guid.NewGuid();
        ClienteId = carritoCompra.ClienteId;
        Estado = EstadoPedido.Pendiente;
        FechaCreacionUtc = DateTime.UtcNow;
        FechaActualizacionUtc = null;
        FechaConfirmacionUtc = null;
        FechaPagoUtc = null;
        FechaEnvioUtc = null;
        FechaEntregaUtc = null;
        FechaCancelacionUtc = null;
        ObservacionCancelacion = null;

        if (carritoCompra.CantidadItems > MaximoDetallesPermitidos)
        {
            throw new DomainException($"El pedido no puede superar {MaximoDetallesPermitidos} líneas.");
        }

        foreach (ItemCarrito item in carritoCompra.Items)
        {
            _detalles.Add(new DetallePedido(Id, item));
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
    /// Identificador único del pedido dentro del dominio.
    /// </summary>
    public Guid Id { get; private set; }

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
    /// Colección de eventos de dominio generados por la entidad.
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

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
                return Money.Zero(MonedaPorDefecto);
            }

            string moneda = _detalles[0].PrecioUnitario.Currency;
            Money total = Money.Zero(moneda);

            foreach (DetallePedido detalle in _detalles)
            {
                total += detalle.Subtotal;
            }

            return total;
        }
    }

    /// <summary>
    /// Fecha y hora UTC en que fue creado el pedido.
    /// </summary>
    public DateTime FechaCreacionUtc { get; private set; }

    /// <summary>
    /// Fecha y hora UTC de la última modificación relevante del pedido.
    /// </summary>
    public DateTime? FechaActualizacionUtc { get; private set; }

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

        if (_detalles.Count >= MaximoDetallesPermitidos)
        {
            throw new DomainException($"El pedido no puede superar {MaximoDetallesPermitidos} líneas.");
        }

        if (_detalles.Any(d => d.ProductoId == detallePedido.ProductoId))
        {
            throw new DomainException("No es posible registrar dos líneas separadas para el mismo producto dentro del mismo pedido.");
        }

        _detalles.Add(detallePedido);
        MarcarActualizacion();
    }

    /// <summary>
    /// Confirma el pedido y lo deja listo para avanzar en el flujo comercial.
    /// </summary>
    public void Confirmar()
    {
        if (Estado != EstadoPedido.Pendiente)
        {
            throw new DomainException($"No es posible confirmar el pedido porque su estado actual es '{Estado}'.");
        }

        ValidarQueTengaDetalles();

        Estado = EstadoPedido.Confirmado;
        FechaConfirmacionUtc = DateTime.UtcNow;
        MarcarActualizacion();
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

        Estado = EstadoPedido.Pagado;
        FechaPagoUtc = DateTime.UtcNow;
        MarcarActualizacion();

        AddDomainEvent(new PedidoPagadoEvent(this));
    }

    /// <summary>
    /// Marca el pedido como en proceso operativo.
    /// </summary>
    public void MarcarEnProceso()
    {
        if (Estado != EstadoPedido.Pagado && Estado != EstadoPedido.Confirmado)
        {
            throw new DomainException($"No es posible pasar el pedido a estado '{EstadoPedido.EnProceso}' desde el estado '{Estado}'.");
        }

        Estado = EstadoPedido.EnProceso;
        MarcarActualizacion();
    }

    /// <summary>
    /// Marca el pedido como enviado o despachado.
    /// </summary>
    public void MarcarEnviado()
    {
        if (Estado != EstadoPedido.EnProceso && Estado != EstadoPedido.Pagado)
        {
            throw new DomainException($"No es posible enviar el pedido desde el estado '{Estado}'.");
        }

        Estado = EstadoPedido.Enviado;
        FechaEnvioUtc = DateTime.UtcNow;
        MarcarActualizacion();
    }

    /// <summary>
    /// Marca el pedido como entregado o completado satisfactoriamente.
    /// </summary>
    public void MarcarEntregado()
    {
        if (Estado != EstadoPedido.Enviado && Estado != EstadoPedido.EnProceso)
        {
            throw new DomainException($"No es posible marcar como entregado un pedido en estado '{Estado}'.");
        }

        Estado = EstadoPedido.Entregado;
        FechaEntregaUtc = DateTime.UtcNow;
        MarcarActualizacion();
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
    /// Elimina todos los eventos de dominio registrados por la entidad.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Devuelve una descripción detallada y legible del pedido.
    /// </summary>
    /// <returns>Cadena descriptiva del pedido y sus totales principales.</returns>
    public string ObtenerDescripcionDetallada()
    {
        return $"Pedido: {Id} | Cliente: {ClienteId} | Estado: {Estado} | Líneas: {CantidadDetalles} | Unidades: {CantidadTotalUnidades} | Total: {Total}";
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
    /// Registra la fecha de modificación del pedido en tiempo UTC.
    /// </summary>
    private void MarcarActualizacion()
    {
        FechaActualizacionUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Registra un nuevo evento de dominio dentro de la entidad.
    /// </summary>
    /// <param name="domainEvent">Evento de dominio a registrar.</param>
    private void AddDomainEvent(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _domainEvents.Add(domainEvent);
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