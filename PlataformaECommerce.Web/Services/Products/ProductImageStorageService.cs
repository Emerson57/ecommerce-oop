using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Extensions.Startup;

namespace PlataformaECommerce.Web.Services.Products;

/// <summary>
/// Implementa el almacenamiento local validado de imágenes de productos para el backoffice.
/// </summary>
public sealed class ProductImageStorageService : IProductImageStorageService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ProductImagesOptions _options;
    private readonly ILogger<ProductImageStorageService> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ProductImageStorageService"/>.
    /// </summary>
    public ProductImageStorageService(
        IWebHostEnvironment webHostEnvironment,
        IOptions<ProductImagesOptions> options,
        ILogger<ProductImageStorageService> logger)
    {
        _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ProductImageProcessResult> ProcessMainImageAsync(
        IFormFile? uploadedImage,
        string? externalImageUrl,
        string? currentImageUrl,
        string productSlug,
        bool removeCurrentImage,
        CancellationToken cancellationToken = default)
    {
        if (uploadedImage is not null && uploadedImage.Length > 0)
        {
            ProductImageProcessResult validationResult = ValidateUploadedImage(uploadedImage, productSlug);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            string storedImageUrl = await SaveUploadedImageAsync(uploadedImage, productSlug, cancellationToken);
            return ProductImageProcessResult.Success(storedImageUrl);
        }

        ProductImageProcessResult externalImageValidation = ValidateExternalImageUrl(externalImageUrl);
        if (!externalImageValidation.IsSuccess)
        {
            return externalImageValidation;
        }

        if (removeCurrentImage)
        {
            return string.Equals(externalImageValidation.ImageUrl, currentImageUrl, StringComparison.Ordinal)
                ? ProductImageProcessResult.Success(null)
                : ProductImageProcessResult.Success(externalImageValidation.ImageUrl);
        }

        if (!string.IsNullOrWhiteSpace(externalImageValidation.ImageUrl))
        {
            return externalImageValidation;
        }

        return ProductImageProcessResult.Success(currentImageUrl);
    }

    /// <inheritdoc />
    public Task DeleteIfManagedAsync(string? imageUrl, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!TryResolveManagedImagePhysicalPath(imageUrl, out string? physicalPath) || string.IsNullOrWhiteSpace(physicalPath))
        {
            return Task.CompletedTask;
        }

        if (!File.Exists(physicalPath))
        {
            return Task.CompletedTask;
        }

        File.Delete(physicalPath);
        _logger.LogInformation("Se eliminó una imagen de producto administrada localmente: {ImagePath}", physicalPath);
        return Task.CompletedTask;
    }

    private ProductImageProcessResult ValidateUploadedImage(IFormFile uploadedImage, string productSlug)
    {
        ArgumentNullException.ThrowIfNull(uploadedImage);

        if (uploadedImage.Length <= 0)
        {
            return ProductImageProcessResult.Failure("La imagen principal seleccionada está vacía o no pudo leerse correctamente.");
        }

        if (uploadedImage.Length > _options.MaxFileSizeInBytes)
        {
            return ProductImageProcessResult.Failure($"La imagen principal supera el tamaño máximo permitido de {_options.MaxFileSizeInBytes / (1024 * 1024)} MB.");
        }

        if (string.IsNullOrWhiteSpace(productSlug))
        {
            return ProductImageProcessResult.Failure("El slug del producto es obligatorio para almacenar la imagen principal.");
        }

        string extension = Path.GetExtension(uploadedImage.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !_options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return ProductImageProcessResult.Failure("La imagen principal debe estar en formato JPG, PNG o WEBP.");
        }

        if (string.IsNullOrWhiteSpace(uploadedImage.ContentType) || !_options.AllowedContentTypes.Contains(uploadedImage.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return ProductImageProcessResult.Failure("El tipo de archivo seleccionado no corresponde a una imagen compatible para el catálogo.");
        }

        return ProductImageProcessResult.Success(null);
    }

    private static ProductImageProcessResult ValidateExternalImageUrl(string? externalImageUrl)
    {
        if (string.IsNullOrWhiteSpace(externalImageUrl))
        {
            return ProductImageProcessResult.Success(null);
        }

        string normalizedUrl = externalImageUrl.Trim();
        if (normalizedUrl.StartsWith("/", StringComparison.Ordinal))
        {
            return ProductImageProcessResult.Success(normalizedUrl);
        }

        if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out Uri? absoluteUri)
            || (absoluteUri.Scheme != Uri.UriSchemeHttps && absoluteUri.Scheme != Uri.UriSchemeHttp))
        {
            return ProductImageProcessResult.Failure("La URL externa de la imagen principal debe comenzar con http://, https:// o con una ruta relativa válida de la aplicación.");
        }

        return ProductImageProcessResult.Success(absoluteUri.ToString());
    }

    private async Task<string> SaveUploadedImageAsync(IFormFile uploadedImage, string productSlug, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string fileExtension = Path.GetExtension(uploadedImage.FileName).ToLowerInvariant();
        string fileName = $"{BuildSlugSegment(productSlug)}-{Guid.NewGuid():N}{fileExtension}";
        string uploadsPhysicalDirectory = GetUploadsPhysicalDirectory();
        Directory.CreateDirectory(uploadsPhysicalDirectory);

        string filePhysicalPath = Path.Combine(uploadsPhysicalDirectory, fileName);
        await using FileStream fileStream = new(filePhysicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await uploadedImage.CopyToAsync(fileStream, cancellationToken);

        string requestPath = _options.RequestPath.TrimEnd('/');
        string storedImageUrl = $"{requestPath}/{fileName}";
        _logger.LogInformation("Se almacenó una imagen principal de producto en {ImageUrl}", storedImageUrl);
        return storedImageUrl;
    }

    private string GetUploadsPhysicalDirectory()
    {
        return ProductImageStoragePathResolver.ResolveUploadsPhysicalDirectory(_webHostEnvironment, _options);
    }

    private bool TryResolveManagedImagePhysicalPath(string? imageUrl, out string? physicalPath)
    {
        physicalPath = null;
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return false;
        }

        string requestPath = _options.RequestPath.TrimEnd('/');
        if (!imageUrl.StartsWith(requestPath, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string fileName = Path.GetFileName(imageUrl);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        physicalPath = Path.Combine(GetUploadsPhysicalDirectory(), fileName);
        return true;
    }

    private static string BuildSlugSegment(string productSlug)
    {
        ArgumentNullException.ThrowIfNull(productSlug);

        StringBuilder builder = new(productSlug.Length);
        bool previousWasSeparator = false;

        foreach (char character in productSlug.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasSeparator = false;
                continue;
            }

            if (previousWasSeparator)
            {
                continue;
            }

            builder.Append('-');
            previousWasSeparator = true;
        }

        string normalizedValue = builder.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(normalizedValue) ? "producto" : normalizedValue;
    }
}
