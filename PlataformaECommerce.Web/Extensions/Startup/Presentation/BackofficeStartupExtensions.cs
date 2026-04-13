using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Services.Products;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Centraliza la configuración tipada específica del backoffice administrativo.
/// </summary>
public static class BackofficeStartupExtensions
{
    /// <summary>
    /// Registra opciones y servicios propios del backoffice sin mezclar otras preocupaciones de UI.
    /// </summary>
    public static IServiceCollection AddBackofficeConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<AdminUsersBackofficeOptions>()
            .Bind(configuration.GetSection(AdminUsersBackofficeOptions.SectionName));

        services
            .AddOptions<ProductImagesOptions>()
            .Bind(configuration.GetSection(ProductImagesOptions.SectionName))
            .Validate(BackofficeProductImagesOptionsValidator.HasValidUploadsDirectory, "La configuración de imágenes de productos requiere un directorio de almacenamiento válido.")
            .Validate(BackofficeProductImagesOptionsValidator.HasValidRequestPath, "La configuración de imágenes de productos requiere una ruta pública válida que comience con '/'.")
            .Validate(BackofficeProductImagesOptionsValidator.HasValidMaxFileSize, "La configuración de imágenes de productos requiere un tamaño máximo de archivo mayor que cero.")
            .Validate(BackofficeProductImagesOptionsValidator.HasAllowedExtensions, "La configuración de imágenes de productos requiere al menos una extensión permitida.")
            .Validate(BackofficeProductImagesOptionsValidator.HasAllowedContentTypes, "La configuración de imágenes de productos requiere al menos un tipo MIME permitido.")
            .Validate(BackofficeProductImagesOptionsValidator.HasSafeStaticFileCacheControlHeader, "La configuración de imágenes de productos contiene un header Cache-Control inválido para la exposición pública de uploads.")
            .ValidateOnStart();

        services.AddScoped<IProductImageStorageService, ProductImageStorageService>();

        return services;
    }
}
