using PlataformaECommerce.Domain.Common;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.Rules;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Domain.Entities.Cart;

/// <summary>
/// Representa una línea o ítem individual dentro del carrito de compras.
/// </summary>
/// <remarks>
/// Esta entidad encapsula la relación entre un producto y la cantidad seleccionada
/// por el cliente dentro del carrito. Además, conserva una instantánea comercial
/// mínima del producto al momento de ser agregado, lo cual permite trabajar de forma
/// consistente con nombre, SKU, precio unitario, imagen principal y tipo de producto
/// sin depender permanentemente de una referencia viva al catálogo.
/// </remarks>
public sealed class ItemCarrito
{
    #region Constantes de negocio

    /// <summary>
    /// Cantidad mínima permitida para un ítem del carrito.
    /// </summary>
    private const int CantidadMinima = 1;

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor privado sin parámetros requerido por herramientas de persistencia como EF Core.
    /// </summary>
    private ItemCarrito()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="ItemCarrito"/>
    /// a partir de un producto disponible y una cantidad solicitada.
    /// </summary>
    /// <param name="producto">Producto que será agregado al carrito.</param>
    /// <param name="cantidad">Cantidad solicitada del producto.</param>
    public ItemCarrito(Producto producto, int cantidad)
    {
        ArgumentNullException.ThrowIfNull(producto);

        producto.ValidarDisponibilidad();

        int cantidadValidada = ValidarCantidad(cantidad);

        if (!StockDisponibleRule.IsSatisfiedBy(producto.Stock, cantidadValidada))
        {
            throw new InventarioInsuficienteException(producto.Sku.Value, producto.Stock, cantidadValidada);
        }

        Id = Guid.NewGuid();
        ProductoId = producto.Id;
        NombreProducto = producto.Nombre;
        SkuProducto = producto.Sku;
        TipoProducto = producto.TipoProducto;
        ImagenPrincipalUrl = producto.ImagenPrincipalUrl;
        PrecioUnitario = producto.Precio;
        Cantidad = cantidadValidada;
        FechaCreacionUtc = DateTime.UtcNow;
        FechaActualizacionUtc = null;
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Identificador único del ítem dentro del carrito.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Identificador del producto asociado al ítem del carrito.
    /// </summary>
    public Guid ProductoId { get; private set; }

    /// <summary>
    /// Nombre comercial del producto al momento de ser agregado al carrito.
    /// </summary>
    public string NombreProducto { get; private set; } = string.Empty;

    /// <summary>
    /// SKU del producto al momento de ser agregado al carrito.
    /// </summary>
    public Sku SkuProducto { get; private set; } = null!;

    /// <summary>
    /// Tipo de producto asociado al ítem del carrito.
    /// </summary>
    public TipoProducto TipoProducto { get; private set; }

    /// <summary>
    /// URL o ruta de la imagen principal del producto al momento de ser agregado al carrito.
    /// </summary>
    public string? ImagenPrincipalUrl { get; private set; }

    /// <summary>
    /// Precio unitario del producto al momento de ser agregado o sincronizado en el carrito.
    /// </summary>
    public Money PrecioUnitario { get; private set; } = null!;

    /// <summary>
    /// Cantidad seleccionada del producto dentro del carrito.
    /// </summary>
    public int Cantidad { get; private set; }

    /// <summary>
    /// Fecha y hora UTC en que fue creado el ítem del carrito.
    /// </summary>
    public DateTime FechaCreacionUtc { get; private set; }

    /// <summary>
    /// Fecha y hora UTC de la última modificación del ítem del carrito.
    /// </summary>
    public DateTime? FechaActualizacionUtc { get; private set; }

    /// <summary>
    /// Subtotal calculado del ítem del carrito.
    /// </summary>
    public Money Subtotal => PrecioUnitario * Cantidad;

    #endregion

    #region Métodos de negocio

    /// <summary>
    /// Actualiza la cantidad del ítem del carrito validando stock disponible.
    /// </summary>
    /// <param name="nuevaCantidad">Nueva cantidad solicitada.</param>
    /// <param name="stockDisponible">Stock actualmente disponible del producto asociado.</param>
    public void ActualizarCantidad(int nuevaCantidad, int stockDisponible)
    {
        int cantidadValidada = ValidarCantidad(nuevaCantidad);
        ValidarStockDisponible(stockDisponible, cantidadValidada);

        Cantidad = cantidadValidada;
        MarcarActualizacion();
    }

    /// <summary>
    /// Incrementa la cantidad del ítem del carrito validando el stock disponible.
    /// </summary>
    /// <param name="cantidad">Cantidad a adicionar.</param>
    /// <param name="stockDisponible">Stock actualmente disponible del producto asociado.</param>
    public void IncrementarCantidad(int cantidad, int stockDisponible)
    {
        if (cantidad <= 0)
        {
            throw new CartException("La cantidad a incrementar en el carrito debe ser mayor que cero.");
        }

        int nuevaCantidad = Cantidad + cantidad;

        if (nuevaCantidad > DomainLimits.MaximoCantidadPorLinea)
        {
            throw new CartException($"La cantidad total del ítem no puede superar {DomainLimits.MaximoCantidadPorLinea} unidades.");
        }

        ValidarStockDisponible(stockDisponible, nuevaCantidad);

        Cantidad = nuevaCantidad;
        MarcarActualizacion();
    }

    /// <summary>
    /// Reduce la cantidad del ítem del carrito.
    /// </summary>
    /// <param name="cantidad">Cantidad a descontar.</param>
    public void ReducirCantidad(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new CartException("La cantidad a reducir en el carrito debe ser mayor que cero.");
        }

        int nuevaCantidad = Cantidad - cantidad;

        if (nuevaCantidad < CantidadMinima)
        {
            throw new CartException("La cantidad resultante del ítem del carrito no puede ser menor que uno.");
        }

        Cantidad = nuevaCantidad;
        MarcarActualizacion();
    }

    /// <summary>
    /// Determina si el ítem corresponde a un producto específico.
    /// </summary>
    /// <param name="productoId">Identificador del producto a comparar.</param>
    /// <returns>
    /// <see langword="true"/> si el ítem corresponde al producto indicado;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool CorrespondeAProducto(Guid productoId)
    {
        return productoId != Guid.Empty && ProductoId == productoId;
    }

    /// <summary>
    /// Sincroniza la información comercial del ítem a partir del producto actual del catálogo.
    /// </summary>
    /// <param name="producto">Producto actual del catálogo.</param>
    /// <remarks>
    /// Este método resulta útil cuando se requiere refrescar nombre, SKU, imagen,
    /// precio o tipo de producto del ítem sin alterar la cantidad seleccionada.
    /// </remarks>
    public void SincronizarDesdeProducto(Producto producto)
    {
        ArgumentNullException.ThrowIfNull(producto);

        if (producto.Id != ProductoId)
        {
            throw new CartException("No es posible sincronizar el ítem del carrito porque el producto indicado no corresponde al producto asociado.");
        }

        NombreProducto = producto.Nombre;
        SkuProducto = producto.Sku;
        TipoProducto = producto.TipoProducto;
        ImagenPrincipalUrl = producto.ImagenPrincipalUrl;
        PrecioUnitario = producto.Precio;

        MarcarActualizacion();
    }

    #endregion

    #region Métodos privados

    /// <summary>
    /// Registra la fecha de modificación del ítem en tiempo UTC.
    /// </summary>
    private void MarcarActualizacion()
    {
        FechaActualizacionUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Valida la cantidad del ítem conforme a las reglas del carrito.
    /// </summary>
    /// <param name="cantidad">Cantidad a validar.</param>
    /// <returns>Cantidad válida.</returns>
    private static int ValidarCantidad(int cantidad)
    {
        if (cantidad < CantidadMinima)
        {
            throw new CartException($"La cantidad del ítem del carrito debe ser al menos {CantidadMinima}.");
        }

        if (cantidad > DomainLimits.MaximoCantidadPorLinea)
        {
            throw new CartException($"La cantidad del ítem del carrito no puede superar {DomainLimits.MaximoCantidadPorLinea} unidades.");
        }

        return cantidad;
    }

    /// <summary>
    /// Valida que exista stock suficiente para la cantidad solicitada.
    /// </summary>
    /// <param name="stockDisponible">Stock disponible del producto.</param>
    /// <param name="cantidadSolicitada">Cantidad requerida.</param>
    private void ValidarStockDisponible(int stockDisponible, int cantidadSolicitada)
    {
        if (stockDisponible < 0)
        {
            throw new CartException("El stock disponible informado para el ítem del carrito no puede ser negativo.");
        }

        if (!StockDisponibleRule.IsSatisfiedBy(stockDisponible, cantidadSolicitada))
        {
            throw new InventarioInsuficienteException(SkuProducto.Value, stockDisponible, cantidadSolicitada);
        }
    }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del ítem del carrito para trazabilidad y depuración.
    /// </summary>
    /// <returns>Cadena representativa del ítem del carrito.</returns>
    public override string ToString()
    {
        return $"ItemCarrito: {NombreProducto} | SKU: {SkuProducto} | Cantidad: {Cantidad} | Precio unitario: {PrecioUnitario} | Subtotal: {Subtotal}";
    }

    #endregion
}