using Microsoft.AspNetCore.Http;

namespace PlataformaECommerce.Web.Services.Products;

/// <summary>
/// Define el contrato responsable de validar, almacenar y limpiar imágenes de productos gestionadas por la UI administrativa.
/// </summary>
public interface IProductImageStorageService
{
    /// <summary>
    /// Procesa la imagen principal de un producto a partir de un archivo cargado, una URL externa o una solicitud de eliminación.
    /// </summary>
    Task<ProductImageProcessResult> ProcessMainImageAsync(
        IFormFile? uploadedImage,
        string? externalImageUrl,
        string? currentImageUrl,
        string productSlug,
        bool removeCurrentImage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina una imagen gestionada localmente cuando existe y pertenece al almacenamiento administrado por la aplicación.
    /// </summary>
    Task DeleteIfManagedAsync(string? imageUrl, CancellationToken cancellationToken = default);
}
