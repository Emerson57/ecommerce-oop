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
    public static IServiceCollection AddConfiguredBackoffice(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<AdminUsersBackofficeOptions>()
            .Bind(configuration.GetSection(AdminUsersBackofficeOptions.SectionName));

        services
            .AddOptions<ProductImagesOptions>()
            .Bind(configuration.GetSection(ProductImagesOptions.SectionName))
            .Validate(ProductImagesOptionsValidator.HasValidUploadsDirectory, "La configuración de imágenes de productos requiere un directorio de almacenamiento válido.")
            .Validate(ProductImagesOptionsValidator.HasValidRequestPath, "La configuración de imágenes de productos requiere una ruta pública válida que comience con '/'.")
            .Validate(ProductImagesOptionsValidator.HasValidMaxFileSize, "La configuración de imágenes de productos requiere un tamaño máximo de archivo mayor que cero.")
            .Validate(ProductImagesOptionsValidator.HasAllowedExtensions, "La configuración de imágenes de productos requiere al menos una extensión permitida.")
            .ValidateOnStart();

        services.AddScoped<IProductImageStorageService, ProductImageStorageService>();

        return services;
    }
}
