using PlataformaECommerce.Domain.Common;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.Rules;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Domain.Entities.Cart;

/// <summary>
/// Representa el carrito de compras de un cliente dentro del dominio del e-commerce.
/// </summary>
/// <remarks>
/// Esta entidad modela el contenedor temporal de productos seleccionados por un cliente
/// antes de la confirmación de un pedido. El carrito administra líneas de compra
/// mediante instancias de <see cref="ItemCarrito"/>, controla su estado operativo,
/// consolida cantidades y subtotales, y centraliza reglas de negocio asociadas
/// a la operación comercial previa al checkout.
/// 
/// La entidad se apoya en reglas de negocio reutilizables para validar el agregado
/// de productos de manera consistente con el resto del dominio.
/// </remarks>
public sealed class CarritoCompra : AggregateRoot
{
    #region Campos privados

    /// <summary>
    /// Colección interna de ítems registrados en el carrito.
    /// </summary>
    private readonly List<ItemCarrito> _items = new();

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor privado sin parámetros requerido por herramientas de persistencia como EF Core.
    /// </summary>
    private CarritoCompra()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="CarritoCompra"/> para un cliente específico.
    /// </summary>
    /// <param name="clienteId">Identificador del cliente propietario del carrito.</param>
    public CarritoCompra(Guid clienteId)
    {
        if (clienteId == Guid.Empty)
        {
            throw new CartException("El identificador del cliente asociado al carrito no puede ser vacío.");
        }

        InicializarAggregateRoot();
        ClienteId = clienteId;
        Activo = true;
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Identificador del cliente propietario del carrito.
    /// </summary>
    public Guid ClienteId { get; private set; }

    /// <summary>
    /// Colección de ítems del carrito en modo de solo lectura.
    /// </summary>
    public IReadOnlyCollection<ItemCarrito> Items => _items.AsReadOnly();

    /// <summary>
    /// Indica si el carrito se encuentra activo para recibir operaciones.
    /// </summary>
    public bool Activo { get; private set; }

    /// <summary>
    /// Cantidad total de líneas o ítems distintos registrados en el carrito.
    /// </summary>
    public int CantidadItems => _items.Count;

    /// <summary>
    /// Cantidad total de unidades acumuladas entre todos los ítems del carrito.
    /// </summary>
    public int CantidadTotalUnidades => _items.Sum(i => i.Cantidad);

    /// <summary>
    /// Total monetario actual del carrito.
    /// </summary>
    /// <remarks>
    /// Cuando el carrito no contiene ítems, se devuelve un valor monetario cero
    /// utilizando la moneda por defecto del dominio.
    /// </remarks>
    public Money Total
    {
        get
        {
            if (_items.Count == 0)
            {
                return Money.Zero(DomainDefaults.DefaultCurrency);
            }

            string moneda = _items[0].PrecioUnitario.Currency;
            Money total = Money.Zero(moneda);

            foreach (ItemCarrito item in _items)
            {
                ValidarConsistenciaMonetaria(item.PrecioUnitario);
                total += item.Subtotal;
            }

            return total;
        }
    }

    #endregion

    #region Métodos de negocio

    /// <summary>
    /// Agrega un producto al carrito. Si el producto ya existe como línea, incrementa su cantidad.
    /// </summary>
    /// <param name="producto">Producto que será agregado.</param>
    /// <param name="cantidad">Cantidad a agregar.</param>
    public void AgregarProducto(Producto producto, int cantidad)
    {
        ValidarCarritoActivo();
        ArgumentNullException.ThrowIfNull(producto);

        if (!CarritoPuedeAgregarProductoRule.IsSatisfiedBy(this, producto, cantidad))
        {
            throw new CartException("No es posible agregar el producto al carrito porque no cumple las reglas del negocio para esta operación.");
        }

        ItemCarrito? itemExistente = BuscarItemPorProductoId(producto.Id);

        if (itemExistente is null)
        {
            ItemCarrito nuevoItem = new(producto, cantidad);
            ValidarConsistenciaMonetaria(nuevoItem.PrecioUnitario);
            _items.Add(nuevoItem);
        }
        else
        {
            ValidarConsistenciaMonetaria(producto.Precio);
            itemExistente.IncrementarCantidad(cantidad, producto.Stock);
            itemExistente.SincronizarDesdeProducto(producto);
        }

        MarcarActualizacion();
    }

    /// <summary>
    /// Actualiza la cantidad de un producto existente dentro del carrito.
    /// </summary>
    /// <param name="producto">Producto al cual se le ajustará la cantidad.</param>
    /// <param name="nuevaCantidad">Nueva cantidad deseada.</param>
    public void ActualizarCantidadProducto(Producto producto, int nuevaCantidad)
    {
        ValidarCarritoActivo();
        ArgumentNullException.ThrowIfNull(producto);

        ItemCarrito item = BuscarItemPorProductoId(producto.Id)
            ?? throw new CartException($"El producto con identificador '{producto.Id}' no existe dentro del carrito.");

        ValidarConsistenciaMonetaria(producto.Precio);
        item.ActualizarCantidad(nuevaCantidad, producto.Stock);
        item.SincronizarDesdeProducto(producto);

        MarcarActualizacion();
    }

    /// <summary>
    /// Incrementa la cantidad de un producto existente dentro del carrito.
    /// </summary>
    /// <param name="producto">Producto al cual se incrementará la cantidad.</param>
    /// <param name="cantidad">Cantidad a adicionar.</param>
    public void IncrementarCantidadProducto(Producto producto, int cantidad)
    {
        ValidarCarritoActivo();
        ArgumentNullException.ThrowIfNull(producto);

        ItemCarrito item = BuscarItemPorProductoId(producto.Id)
            ?? throw new CartException($"El producto con identificador '{producto.Id}' no existe dentro del carrito.");

        if (!CarritoPuedeAgregarProductoRule.IsSatisfiedBy(this, producto, cantidad))
        {
            throw new CartException("No es posible incrementar la cantidad del producto en el carrito porque no cumple las reglas del negocio para esta operación.");
        }

        ValidarConsistenciaMonetaria(producto.Precio);
        item.IncrementarCantidad(cantidad, producto.Stock);
        item.SincronizarDesdeProducto(producto);

        MarcarActualizacion();
    }

    /// <summary>
    /// Reduce la cantidad de un producto existente dentro del carrito.
    /// Si la cantidad resultante llega a cero o menos, el producto se elimina del carrito.
    /// </summary>
    /// <param name="productoId">Identificador del producto a ajustar.</param>
    /// <param name="cantidad">Cantidad a reducir.</param>
    public void ReducirCantidadProducto(Guid productoId, int cantidad)
    {
        ValidarCarritoActivo();

        if (productoId == Guid.Empty)
        {
            throw new CartException("El identificador del producto no puede ser vacío.");
        }

        if (cantidad <= 0)
        {
            throw new CartException("La cantidad a reducir del carrito debe ser mayor que cero.");
        }

        ItemCarrito item = BuscarItemPorProductoId(productoId)
            ?? throw new CartException($"El producto con identificador '{productoId}' no existe dentro del carrito.");

        if (item.Cantidad - cantidad < 1)
        {
            _items.Remove(item);
        }
        else
        {
            item.ReducirCantidad(cantidad);
        }

        MarcarActualizacion();
    }

    /// <summary>
    /// Elimina completamente un producto del carrito según su identificador.
    /// </summary>
    /// <param name="productoId">Identificador del producto a eliminar.</param>
    /// <returns>
    /// <see langword="true"/> si el producto fue removido;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool RemoverProducto(Guid productoId)
    {
        ValidarCarritoActivo();

        if (productoId == Guid.Empty)
        {
            throw new CartException("El identificador del producto no puede ser vacío.");
        }

        ItemCarrito? item = BuscarItemPorProductoId(productoId);

        if (item is null)
        {
            return false;
        }

        _items.Remove(item);
        MarcarActualizacion();
        return true;
    }

    /// <summary>
    /// Elimina todos los ítems registrados en el carrito.
    /// </summary>
    public void VaciarCarrito()
    {
        ValidarCarritoActivo();

        if (_items.Count == 0)
        {
            throw new CarritoVacioException();
        }

        _items.Clear();
        MarcarActualizacion();
    }

    /// <summary>
    /// Determina si el carrito contiene al menos una línea asociada a un producto específico.
    /// </summary>
    /// <param name="productoId">Identificador del producto a consultar.</param>
    /// <returns>
    /// <see langword="true"/> si el producto existe en el carrito;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool ContieneProducto(Guid productoId)
    {
        if (productoId == Guid.Empty)
        {
            return false;
        }

        return _items.Any(i => i.CorrespondeAProducto(productoId));
    }

    /// <summary>
    /// Obtiene la cantidad actualmente registrada para un producto específico dentro del carrito.
    /// </summary>
    /// <param name="productoId">Identificador del producto consultado.</param>
    /// <returns>Cantidad registrada para el producto; cero si no existe dentro del carrito.</returns>
    public int ObtenerCantidadDeProducto(Guid productoId)
    {
        if (productoId == Guid.Empty)
        {
            throw new CartException("El identificador del producto no puede ser vacío.");
        }

        ItemCarrito? item = BuscarItemPorProductoId(productoId);
        return item?.Cantidad ?? 0;
    }

    /// <summary>
    /// Obtiene el ítem asociado a un producto específico dentro del carrito.
    /// </summary>
    /// <param name="productoId">Identificador del producto.</param>
    /// <returns>Ítem encontrado o <see langword="null"/> si no existe dentro del carrito.</returns>
    public ItemCarrito? ObtenerItemPorProductoId(Guid productoId)
    {
        if (productoId == Guid.Empty)
        {
            return null;
        }

        return BuscarItemPorProductoId(productoId);
    }

    /// <summary>
    /// Verifica si el carrito contiene al menos un ítem válido.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si el carrito tiene contenido;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool TieneItems()
    {
        return _items.Count > 0;
    }

    /// <summary>
    /// Valida que el carrito tenga contenido antes de continuar con un flujo comercial.
    /// </summary>
    public void ValidarQueTengaItems()
    {
        if (_items.Count == 0)
        {
            throw new CarritoVacioException();
        }
    }

    /// <summary>
    /// Activa el carrito para permitir operaciones nuevamente.
    /// </summary>
    public void Activar()
    {
        if (Activo)
        {
            return;
        }

        Activo = true;
        MarcarActualizacion();
    }

    /// <summary>
    /// Desactiva lógicamente el carrito para impedir nuevas operaciones.
    /// </summary>
    public void Desactivar()
    {
        if (!Activo)
        {
            return;
        }

        Activo = false;
        MarcarActualizacion();
    }

    #endregion

    #region Métodos privados auxiliares

    /// <summary>
    /// Busca internamente un ítem del carrito a partir del identificador del producto.
    /// </summary>
    /// <param name="productoId">Identificador del producto.</param>
    /// <returns>Ítem encontrado o <see langword="null"/> si no existe coincidencia.</returns>
    private ItemCarrito? BuscarItemPorProductoId(Guid productoId)
    {
        return _items.FirstOrDefault(i => i.CorrespondeAProducto(productoId));
    }

    /// <summary>
    /// Valida que el carrito se encuentre activo antes de ejecutar operaciones de modificación.
    /// </summary>
    private void ValidarCarritoActivo()
    {
        if (!Activo)
        {
            throw new CartException("No es posible realizar la operación porque el carrito se encuentra inactivo.");
        }
    }

    /// <summary>
    /// Valida que la moneda de una línea comercial sea consistente con el resto del carrito.
    /// </summary>
    /// <param name="valorMonetario">Valor monetario que se desea incorporar o sincronizar.</param>
    private void ValidarConsistenciaMonetaria(Money valorMonetario)
    {
        ArgumentNullException.ThrowIfNull(valorMonetario);

        string? monedaReferencia = _items.Count == 0
            ? null
            : _items[0].PrecioUnitario.Currency;

        if (!MonedaConsistenteRule.IsSatisfiedBy(monedaReferencia, valorMonetario))
        {
            throw new CartException(
                $"No es posible operar el carrito con productos en monedas distintas. Moneda esperada: '{monedaReferencia}', moneda recibida: '{valorMonetario.Currency}'.");
        }
    }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del carrito para trazabilidad y depuración.
    /// </summary>
    /// <returns>Cadena representativa del carrito.</returns>
    public override string ToString()
    {
        return $"CarritoCompra: {Id} | Cliente: {ClienteId} | Líneas: {CantidadItems} | Unidades: {CantidadTotalUnidades} | Total: {Total} | Activo: {Activo}";
    }

    #endregion
}