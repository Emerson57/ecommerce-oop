using PlataformaECommerce.Domain.Common;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Domain.Entities.Orders;

/// <summary>
/// Representa una línea individual dentro de un pedido.
/// </summary>
/// <remarks>
/// Esta entidad conserva una instantánea comercial del producto al momento de la compra,
/// permitiendo mantener consistencia histórica aunque posteriormente el producto cambie
/// en el catálogo. Su propósito es preservar la información relevante de venta utilizada
/// en la transacción, como nombre, SKU, precio unitario, cantidad y subtotal.
/// </remarks>
public sealed class DetallePedido
{
    #region Constantes de negocio

    /// <summary>
    /// Cantidad mínima permitida para una línea de pedido.
    /// </summary>
    private const int CantidadMinima = 1;

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor privado sin parámetros requerido por herramientas de persistencia como EF Core.
    /// </summary>
    private DetallePedido()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="DetallePedido"/>
    /// a partir de los datos explícitos de una línea de compra.
    /// </summary>
    /// <param name="pedidoId">Identificador del pedido al que pertenece el detalle.</param>
    /// <param name="productoId">Identificador del producto asociado.</param>
    /// <param name="nombreProducto">Nombre comercial del producto.</param>
    /// <param name="skuProducto">SKU del producto representado como Value Object.</param>
    /// <param name="tipoProducto">Tipo de producto asociado a la línea.</param>
    /// <param name="imagenPrincipalUrl">Imagen principal del producto al momento de la compra.</param>
    /// <param name="precioUnitario">Precio unitario aplicado en la compra representado como Value Object.</param>
    /// <param name="cantidad">Cantidad adquirida.</param>
    public DetallePedido(
        Guid pedidoId,
        Guid productoId,
        string nombreProducto,
        Sku skuProducto,
        TipoProducto tipoProducto,
        string? imagenPrincipalUrl,
        Money precioUnitario,
        int cantidad)
    {
        Id = Guid.NewGuid();
        PedidoId = ValidarPedidoId(pedidoId);
        ProductoId = ValidarProductoId(productoId);
        NombreProducto = ValidarNombreProducto(nombreProducto);
        SkuProducto = ValidarSkuProducto(skuProducto);
        TipoProducto = tipoProducto;
        ImagenPrincipalUrl = ValidarImagenPrincipalUrl(imagenPrincipalUrl);
        PrecioUnitario = ValidarPrecioUnitario(precioUnitario);
        Cantidad = ValidarCantidad(cantidad);
        FechaCreacionUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="DetallePedido"/>
    /// a partir de un ítem previamente registrado en el carrito.
    /// </summary>
    /// <param name="pedidoId">Identificador del pedido al que pertenecerá el detalle.</param>
    /// <param name="itemCarrito">Ítem del carrito que servirá como origen de la línea del pedido.</param>
    public DetallePedido(Guid pedidoId, ItemCarrito itemCarrito)
    {
        ArgumentNullException.ThrowIfNull(itemCarrito);

        Id = Guid.NewGuid();
        PedidoId = ValidarPedidoId(pedidoId);
        ProductoId = ValidarProductoId(itemCarrito.ProductoId);
        NombreProducto = ValidarNombreProducto(itemCarrito.NombreProducto);
        SkuProducto = ValidarSkuProducto(itemCarrito.SkuProducto);
        TipoProducto = itemCarrito.TipoProducto;
        ImagenPrincipalUrl = ValidarImagenPrincipalUrl(itemCarrito.ImagenPrincipalUrl);
        PrecioUnitario = ValidarPrecioUnitario(itemCarrito.PrecioUnitario);
        Cantidad = ValidarCantidad(itemCarrito.Cantidad);
        FechaCreacionUtc = DateTime.UtcNow;
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Identificador único del detalle del pedido.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Identificador del pedido al cual pertenece la línea.
    /// </summary>
    public Guid PedidoId { get; private set; }

    /// <summary>
    /// Identificador del producto asociado a la línea del pedido.
    /// </summary>
    public Guid ProductoId { get; private set; }

    /// <summary>
    /// Nombre comercial del producto al momento de la compra.
    /// </summary>
    public string NombreProducto { get; private set; } = string.Empty;

    /// <summary>
    /// SKU del producto al momento de la compra.
    /// </summary>
    public Sku SkuProducto { get; private set; } = null!;

    /// <summary>
    /// Tipo de producto asociado al detalle del pedido.
    /// </summary>
    public TipoProducto TipoProducto { get; private set; }

    /// <summary>
    /// URL o ruta de la imagen principal del producto al momento de la compra.
    /// </summary>
    public string? ImagenPrincipalUrl { get; private set; }

    /// <summary>
    /// Precio unitario aplicado al producto dentro del pedido.
    /// </summary>
    public Money PrecioUnitario { get; private set; } = null!;

    /// <summary>
    /// Cantidad adquirida del producto dentro del pedido.
    /// </summary>
    public int Cantidad { get; private set; }

    /// <summary>
    /// Fecha y hora UTC en que fue creada la línea del pedido.
    /// </summary>
    public DateTime FechaCreacionUtc { get; private set; }

    /// <summary>
    /// Subtotal calculado de la línea del pedido.
    /// </summary>
    public Money Subtotal => PrecioUnitario * Cantidad;

    #endregion

    #region Métodos de negocio

    /// <summary>
    /// Determina si el detalle corresponde a un producto específico.
    /// </summary>
    /// <param name="productoId">Identificador del producto a comparar.</param>
    /// <returns>
    /// <see langword="true"/> si la línea corresponde al producto indicado;
    /// de lo contrario, <see langword="false"/>.
    /// </returns>
    public bool CorrespondeAProducto(Guid productoId)
    {
        return productoId != Guid.Empty && ProductoId == productoId;
    }

    /// <summary>
    /// Devuelve una descripción detallada y legible de la línea del pedido.
    /// </summary>
    /// <returns>Cadena con la información principal del detalle del pedido.</returns>
    public string ObtenerDescripcionDetallada()
    {
        return $"{NombreProducto} | SKU: {SkuProducto} | Tipo: {TipoProducto} | Cantidad: {Cantidad} | Precio unitario: {PrecioUnitario} | Subtotal: {Subtotal}";
    }

    #endregion

    #region Métodos privados de validación

    /// <summary>
    /// Valida el identificador del pedido.
    /// </summary>
    /// <param name="pedidoId">Identificador a validar.</param>
    /// <returns>Identificador válido.</returns>
    private static Guid ValidarPedidoId(Guid pedidoId)
    {
        if (pedidoId == Guid.Empty)
        {
            throw new DomainException("El identificador del pedido no puede ser vacío.");
        }

        return pedidoId;
    }

    /// <summary>
    /// Valida el identificador del producto.
    /// </summary>
    /// <param name="productoId">Identificador a validar.</param>
    /// <returns>Identificador válido.</returns>
    private static Guid ValidarProductoId(Guid productoId)
    {
        if (productoId == Guid.Empty)
        {
            throw new DomainException("El identificador del producto no puede ser vacío.");
        }

        return productoId;
    }

    /// <summary>
    /// Valida el nombre comercial del producto.
    /// </summary>
    /// <param name="nombreProducto">Nombre a validar.</param>
    /// <returns>Nombre normalizado y válido.</returns>
    private static string ValidarNombreProducto(string nombreProducto)
    {
        if (string.IsNullOrWhiteSpace(nombreProducto))
        {
            throw new DomainException("El nombre del producto en el detalle del pedido es obligatorio.");
        }

        return nombreProducto.Trim();
    }

    /// <summary>
    /// Valida el SKU del producto.
    /// </summary>
    /// <param name="skuProducto">SKU a validar.</param>
    /// <returns>SKU válido.</returns>
    private static Sku ValidarSkuProducto(Sku skuProducto)
    {
        if (skuProducto is null)
        {
            throw new DomainException("El SKU del producto en el detalle del pedido es obligatorio.");
        }

        return skuProducto;
    }

    /// <summary>
    /// Valida el precio unitario aplicado al detalle del pedido.
    /// </summary>
    /// <param name="precioUnitario">Precio a validar.</param>
    /// <returns>Precio válido.</returns>
    private static Money ValidarPrecioUnitario(Money precioUnitario)
    {
        if (precioUnitario is null)
        {
            throw new DomainException("El precio unitario del detalle del pedido es obligatorio.");
        }

        return precioUnitario;
    }

    /// <summary>
    /// Valida la cantidad aplicada al detalle del pedido.
    /// </summary>
    /// <param name="cantidad">Cantidad a validar.</param>
    /// <returns>Cantidad válida.</returns>
    private static int ValidarCantidad(int cantidad)
    {
        if (cantidad < CantidadMinima)
        {
            throw new DomainException($"La cantidad del detalle del pedido debe ser al menos {CantidadMinima}.");
        }

        if (cantidad > DomainLimits.MaximoCantidadPorLinea)
        {
            throw new DomainException($"La cantidad del detalle del pedido no puede superar {DomainLimits.MaximoCantidadPorLinea} unidades.");
        }

        return cantidad;
    }

    /// <summary>
    /// Valida y normaliza la URL o ruta de la imagen principal del producto.
    /// </summary>
    /// <param name="imagenPrincipalUrl">Valor de imagen a validar.</param>
    /// <returns>Ruta normalizada o <see langword="null"/> cuando no se suministra valor.</returns>
    private static string? ValidarImagenPrincipalUrl(string? imagenPrincipalUrl)
    {
        if (string.IsNullOrWhiteSpace(imagenPrincipalUrl))
        {
            return null;
        }

        return imagenPrincipalUrl.Trim();
    }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del detalle del pedido para trazabilidad y depuración.
    /// </summary>
    /// <returns>Cadena representativa del detalle del pedido.</returns>
    public override string ToString()
    {
        return $"DetallePedido: {NombreProducto} | SKU: {SkuProducto} | Cantidad: {Cantidad} | Precio unitario: {PrecioUnitario} | Subtotal: {Subtotal}";
    }

    #endregion
}