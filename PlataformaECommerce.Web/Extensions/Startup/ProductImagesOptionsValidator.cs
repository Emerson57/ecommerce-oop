using PlataformaECommerce.Web.Configuration;
using Microsoft.AspNetCore.StaticFiles;

namespace PlataformaECommerce.Web.Extensions.Startup;

internal static class ProductImagesOptionsValidator
{
    public static bool HasValidUploadsDirectory(ProductImagesOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.UploadsDirectory))
        {
            return false;
        }

        string normalizedPath = options.UploadsDirectory.Trim().Replace('\\', '/');
        return !Path.IsPathRooted(normalizedPath) && !normalizedPath.Contains("..", StringComparison.Ordinal);
    }

    public static bool HasValidRequestPath(ProductImagesOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return !string.IsNullOrWhiteSpace(options.RequestPath) && options.RequestPath.StartsWith('/');
    }

    public static bool HasValidMaxFileSize(ProductImagesOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.MaxFileSizeInBytes > 0;
    }

    public static bool HasAllowedExtensions(ProductImagesOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.AllowedExtensions.Count == 0)
        {
            return false;
        }

        FileExtensionContentTypeProvider contentTypeProvider = new();
        return options.AllowedExtensions.All(extension =>
            !string.IsNullOrWhiteSpace(extension)
            && extension.Trim().StartsWith('.')
            && contentTypeProvider.Mappings.ContainsKey(extension.Trim()));
    }

    public static bool HasAllowedContentTypes(ProductImagesOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.AllowedContentTypes.Count > 0
            && options.AllowedContentTypes.All(contentType => !string.IsNullOrWhiteSpace(contentType));
    }

    public static bool HasSafeStaticFileCacheControlHeader(ProductImagesOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.StaticFileCacheControlHeaderValue))
        {
            return true;
        }

        return !options.StaticFileCacheControlHeaderValue.Contains('\r')
            && !options.StaticFileCacheControlHeaderValue.Contains('\n');
    }
}
