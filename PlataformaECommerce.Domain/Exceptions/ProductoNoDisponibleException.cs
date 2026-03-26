namespace PlataformaECommerce.Domain.Exceptions;

/// <summary>
/// Representa el error generado cuando un producto no se encuentra disponible
/// para ser operado comercialmente dentro del sistema.
/// </summary>
/// <remarks>
/// Esta excepción aplica cuando el producto está inactivo, no tiene stock suficiente,
/// fue retirado del catálogo o no cumple las condiciones requeridas para su compra
/// o procesamiento dentro del flujo del negocio.
/// </remarks>
public class ProductoNoDisponibleException : ProductException
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ProductoNoDisponibleException"/>.
    /// </summary>
    /// <param name="productId">Identificador del producto afectado.</param>
    /// <param name="nombreProducto">Nombre comercial del producto.</param>
    public ProductoNoDisponibleException(Guid productId, string nombreProducto)
        : base($"El producto '{nombreProducto}' con identificador '{productId}' no se encuentra disponible para la operación solicitada.")
    {
        ProductId = productId;
        NombreProducto = nombreProducto;
    }

    /// <summary>
    /// Obtiene el identificador del producto afectado.
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// Obtiene el nombre comercial del producto afectado.
    /// </summary>
    public string NombreProducto { get; }
}