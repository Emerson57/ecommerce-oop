using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Centraliza la exposición controlada de archivos estáticos propios de la aplicación, incluyendo uploads administrados.
/// </summary>
public static class StaticFileStartupExtensions
{
    /// <summary>
    /// Expone la ruta pública de imágenes de producto preservando el comportamiento actual y preparando endurecimientos posteriores.
    /// </summary>
    /// <param name="app">Aplicación web a configurar.</param>
    /// <returns>La misma aplicación web para encadenamiento fluido.</returns>
    public static WebApplication UseUploadStaticFiles(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        ProductImagesOptions productImagesOptions = app.Services.GetRequiredService<IOptions<ProductImagesOptions>>().Value;
        string productImagesPhysicalPath = ProductImageStoragePathResolver.ResolveUploadsPhysicalDirectory(app.Environment, productImagesOptions);

        Directory.CreateDirectory(productImagesPhysicalPath);

        // TODO: Si el negocio requiere media privada o aprobaciones previas, reemplazar esta ruta pública por un endpoint autenticado de descarga controlada.
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(productImagesPhysicalPath),
            RequestPath = productImagesOptions.RequestPath.TrimEnd('/'),
            ContentTypeProvider = CreateRestrictedContentTypeProvider(productImagesOptions),
            ServeUnknownFileTypes = false,
            OnPrepareResponse = context =>
            {
                context.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";

                if (!string.IsNullOrWhiteSpace(productImagesOptions.StaticFileCacheControlHeaderValue))
                {
                    context.Context.Response.Headers.CacheControl = productImagesOptions.StaticFileCacheControlHeaderValue.Trim();
                }
            }
        });

        return app;
    }

    private static FileExtensionContentTypeProvider CreateRestrictedContentTypeProvider(ProductImagesOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        FileExtensionContentTypeProvider contentTypeProvider = new();
        HashSet<string> allowedExtensions = options.AllowedExtensions
            .Where(static extension => !string.IsNullOrWhiteSpace(extension))
            .Select(static extension => NormalizeExtension(extension))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (string extension in contentTypeProvider.Mappings.Keys.ToArray())
        {
            if (!allowedExtensions.Contains(extension))
            {
                contentTypeProvider.Mappings.Remove(extension);
            }
        }

        return contentTypeProvider;
    }

    private static string NormalizeExtension(string extension)
    {
        ArgumentNullException.ThrowIfNull(extension);

        string normalizedExtension = extension.Trim();
        return normalizedExtension.StartsWith('.')
            ? normalizedExtension
            : $".{normalizedExtension}";
    }
}
