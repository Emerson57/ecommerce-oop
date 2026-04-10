using Microsoft.AspNetCore.Hosting;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

internal static class ProductImageStoragePathResolver
{
    public static string ResolveUploadsPhysicalDirectory(IWebHostEnvironment environment, ProductImagesOptions options)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(options);

        string webRootPath = string.IsNullOrWhiteSpace(environment.WebRootPath)
            ? Path.Combine(environment.ContentRootPath, "wwwroot")
            : environment.WebRootPath;
        string relativeUploadsPath = options.UploadsDirectory
            .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);

        return Path.GetFullPath(Path.Combine(webRootPath, relativeUploadsPath));
    }
}
