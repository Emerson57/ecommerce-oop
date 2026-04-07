using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

internal static class ProductImagesStaticFileConfigurator
{
    public static void UseConfiguredStaticFiles(WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        ProductImagesOptions productImagesOptions = app.Services.GetRequiredService<IOptions<ProductImagesOptions>>().Value;
        string webRootPath = string.IsNullOrWhiteSpace(app.Environment.WebRootPath)
            ? Path.Combine(app.Environment.ContentRootPath, "wwwroot")
            : app.Environment.WebRootPath;
        string productImagesPhysicalPath = Path.Combine(webRootPath, productImagesOptions.UploadsDirectory.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(productImagesPhysicalPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(productImagesPhysicalPath),
            RequestPath = productImagesOptions.RequestPath
        });
    }
}
