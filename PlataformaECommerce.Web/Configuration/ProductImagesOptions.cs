namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Representa las opciones de configuración para la gestión de imágenes de productos en el backoffice.
/// </summary>
public sealed class ProductImagesOptions
{
    /// <summary>
    /// Ruta de configuración utilizada para enlazar las opciones desde la configuración externa.
    /// </summary>
    public const string SectionName = "Backoffice:ProductImages";

    /// <summary>
    /// Directorio relativo dentro de <c>wwwroot</c> donde se almacenan las imágenes cargadas.
    /// </summary>
    public string UploadsDirectory { get; set; } = "uploads/products";

    /// <summary>
    /// Ruta pública utilizada para exponer las imágenes cargadas desde la aplicación web.
    /// </summary>
    public string RequestPath { get; set; } = "/uploads/products";

    /// <summary>
    /// Tamaño máximo permitido para cada archivo de imagen expresado en bytes.
    /// </summary>
    public long MaxFileSizeInBytes { get; set; } = 5 * 1024 * 1024;

    /// <summary>
    /// Extensiones admitidas para la carga de imágenes de producto.
    /// </summary>
    public List<string> AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
}
