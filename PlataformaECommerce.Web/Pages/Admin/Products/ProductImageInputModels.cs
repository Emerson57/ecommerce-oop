using Microsoft.AspNetCore.Http;

namespace PlataformaECommerce.Web.Pages.Admin.Products;

/// <summary>
/// Representa el contrato visual reutilizable del formulario administrativo de productos.
/// </summary>
public sealed class ProductImagesInputModel
{
    /// <summary>
    /// Cantidad fija de slots expuestos por el MVP de galería en el backoffice.
    /// </summary>
    public const int DefaultGallerySlots = 3;

    /// <summary>
    /// Obtiene o establece el contrato de la imagen principal del producto.
    /// </summary>
    public ProductMainImageInputModel MainImage { get; set; } = new();

    /// <summary>
    /// Obtiene o establece la colección editable de imágenes complementarias del producto.
    /// </summary>
    public List<ProductGalleryImageInputModel> Gallery { get; set; } = [];

    /// <summary>
    /// Garantiza que la UI disponga siempre de los slots mínimos definidos para la galería.
    /// </summary>
    public void EnsureGallerySlots()
    {
        if (Gallery.Count > DefaultGallerySlots)
        {
            Gallery = Gallery.Take(DefaultGallerySlots).ToList();
        }

        while (Gallery.Count < DefaultGallerySlots)
        {
            Gallery.Add(new ProductGalleryImageInputModel());
        }
    }

    /// <summary>
    /// Obtiene la galería normalizada que debe persistirse para el producto.
    /// </summary>
    /// <param name="mainImageUrl">Imagen principal actual para excluirla de la galería.</param>
    /// <returns>Colección de URLs normalizadas y sin duplicados.</returns>
    public IReadOnlyCollection<string> GetPersistableGalleryUrls(string? mainImageUrl)
    {
        string? normalizedMainImageUrl = NormalizeUrl(mainImageUrl);

        return Gallery
            .Select(item => NormalizeUrl(item.ImageUrl))
            .Where(imageUrl => !string.IsNullOrWhiteSpace(imageUrl))
            .Where(imageUrl => !string.Equals(imageUrl, normalizedMainImageUrl, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Cast<string>()
            .ToArray();
    }

    private static string? NormalizeUrl(string? imageUrl)
        => string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
}

/// <summary>
/// Representa el estado y la intención de edición de la imagen principal del producto.
/// </summary>
public sealed class ProductMainImageInputModel
{
    /// <summary>
    /// Obtiene o establece la URL actualmente persistida para la imagen principal.
    /// </summary>
    public string? CurrentImageUrl { get; set; }

    /// <summary>
    /// Obtiene o establece la URL externa solicitada por el administrador.
    /// </summary>
    public string? ExternalImageUrl { get; set; }

    /// <summary>
    /// Obtiene o establece el archivo local cargado para la imagen principal.
    /// </summary>
    public IFormFile? UploadedFile { get; set; }

    /// <summary>
    /// Obtiene o establece un valor que indica si la imagen actual debe retirarse cuando no exista reemplazo.
    /// </summary>
    public bool RemoveCurrentImage { get; set; }

    /// <summary>
    /// Obtiene el origen de la imagen actualmente persistida.
    /// </summary>
    public ProductImageOrigin CurrentOrigin => ProductImageOriginResolver.Resolve(CurrentImageUrl);

    /// <summary>
    /// Obtiene el origen solicitado para la imagen principal según la intención actual del formulario.
    /// </summary>
    public ProductImageOrigin RequestedOrigin
        => UploadedFile is not null ? ProductImageOrigin.Local : ProductImageOriginResolver.Resolve(ExternalImageUrl);

    /// <summary>
    /// Resuelve la URL que debe utilizarse para la previsualización de la imagen principal.
    /// </summary>
    public string? ResolvePreviewUrl()
    {
        if (RemoveCurrentImage && UploadedFile is null && string.IsNullOrWhiteSpace(ExternalImageUrl))
        {
            return null;
        }

        return !string.IsNullOrWhiteSpace(ExternalImageUrl) ? ExternalImageUrl.Trim() : CurrentImageUrl;
    }
}

/// <summary>
/// Representa un elemento editable de la galería complementaria del producto.
/// </summary>
public sealed class ProductGalleryImageInputModel
{
    /// <summary>
    /// Obtiene o establece el identificador lógico de la imagen dentro de la galería.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Obtiene o establece la URL persistida de la imagen de galería.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Resuelve la URL utilizable para la previsualización de la imagen de galería.
    /// </summary>
    public string? ResolvePreviewUrl()
        => string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl.Trim();

    /// <summary>
    /// Obtiene el origen de la imagen de galería.
    /// </summary>
    public ProductImageOrigin Origin => ProductImageOriginResolver.Resolve(ImageUrl);
}

/// <summary>
/// Identifica el origen operativo de una imagen de producto dentro del backoffice.
/// </summary>
public enum ProductImageOrigin
{
    None = 0,
    Local = 1,
    External = 2
}

/// <summary>
/// Resuelve el origen funcional de una imagen a partir de su URL persistida.
/// </summary>
internal static class ProductImageOriginResolver
{
    /// <summary>
    /// Determina si una imagen corresponde a almacenamiento local de la aplicación, a un origen externo o si no existe.
    /// </summary>
    public static ProductImageOrigin Resolve(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return ProductImageOrigin.None;
        }

        string normalizedUrl = imageUrl.Trim();
        return Uri.TryCreate(normalizedUrl, UriKind.Absolute, out Uri? absoluteUri)
            && (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps)
            ? ProductImageOrigin.External
            : ProductImageOrigin.Local;
    }

    /// <summary>
    /// Obtiene una etiqueta legible para presentar el origen de la imagen en la UI administrativa.
    /// </summary>
    public static string ToDisplayName(ProductImageOrigin origin)
        => origin switch
        {
            ProductImageOrigin.Local => "Local",
            ProductImageOrigin.External => "Externo",
            _ => "Sin imagen"
        };
}