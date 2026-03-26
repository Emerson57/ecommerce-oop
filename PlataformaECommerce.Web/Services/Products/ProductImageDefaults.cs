namespace PlataformaECommerce.Web.Services.Products;

/// <summary>
/// Centraliza los valores por defecto utilizados para la presentación de imágenes de productos.
/// </summary>
internal static class ProductImageDefaults
{
    /// <summary>
    /// Ruta pública de la imagen de respaldo cuando un producto no tiene imagen principal informada.
    /// </summary>
    internal const string PlaceholderImageUrl = "/images/placeholders/product-placeholder.svg";

    /// <summary>
    /// Resuelve la URL visible de una imagen de producto utilizando un respaldo consistente cuando no existe imagen principal.
    /// </summary>
    internal static string ResolveDisplayUrl(string? imageUrl)
        => string.IsNullOrWhiteSpace(imageUrl) ? PlaceholderImageUrl : imageUrl.Trim();

    /// <summary>
    /// Construye la colección visible de imágenes del producto priorizando la imagen principal y eliminando duplicados.
    /// </summary>
    internal static IReadOnlyCollection<string> ResolveDisplayGallery(string? mainImageUrl, IEnumerable<string>? imageGallery)
    {
        List<string> images = [];

        void AddIfValid(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            string normalizedImageUrl = imageUrl.Trim();
            if (images.Contains(normalizedImageUrl, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            images.Add(normalizedImageUrl);
        }

        AddIfValid(mainImageUrl);

        if (imageGallery is not null)
        {
            foreach (string imageUrl in imageGallery)
            {
                AddIfValid(imageUrl);
            }
        }

        return images.Count == 0
            ? [PlaceholderImageUrl]
            : images;
    }
}
