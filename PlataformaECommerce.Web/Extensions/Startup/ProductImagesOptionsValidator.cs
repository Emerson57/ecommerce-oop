using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

internal static class ProductImagesOptionsValidator
{
    public static bool HasValidUploadsDirectory(ProductImagesOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return !string.IsNullOrWhiteSpace(options.UploadsDirectory);
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
        return options.AllowedExtensions.Count > 0;
    }
}
