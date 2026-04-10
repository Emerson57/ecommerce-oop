using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Services.Products;

namespace PlataformaECommerce.Tests.Web.Products;

[TestFixture]
public class ProductImageStorageServiceTests
{
    [Test]
    public async Task ProcessMainImageAsync_ArchivoValido_GuardaImagenLocalYRetornaRutaPublica()
    {
        string webRootPath = Path.Combine(Path.GetTempPath(), "ecommerce-oop-tests", Guid.NewGuid().ToString("N"));
        ProductImageStorageService service = CreateService(webRootPath);
        await using MemoryStream stream = new([1, 2, 3, 4]);
        FormFile uploadedImage = new(stream, 0, stream.Length, "MainImageFile", "mouse-gamer.webp")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/webp"
        };

        ProductImageProcessResult result = await service.ProcessMainImageAsync(uploadedImage, null, null, "mouse-gamer", false, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ImageUrl, Does.StartWith("/uploads/products/mouse-gamer-"));
        Assert.That(result.ImageUrl, Does.EndWith(".webp"));
        Assert.That(File.Exists(Path.Combine(webRootPath, "uploads", "products", Path.GetFileName(result.ImageUrl))), Is.True);
    }

    [Test]
    public async Task ProcessMainImageAsync_RemoverImagenConRutaSinCambios_RetornaNulo()
    {
        ProductImageStorageService service = CreateService(Path.Combine(Path.GetTempPath(), "ecommerce-oop-tests", Guid.NewGuid().ToString("N")));

        ProductImageProcessResult result = await service.ProcessMainImageAsync(
            uploadedImage: null,
            externalImageUrl: "/uploads/products/mouse-gamer-actual.webp",
            currentImageUrl: "/uploads/products/mouse-gamer-actual.webp",
            productSlug: "mouse-gamer",
            removeCurrentImage: true,
            cancellationToken: CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ImageUrl, Is.Null);
    }

    [Test]
    public async Task ProcessMainImageAsync_UrlExternaInvalida_RetornaFalloControlado()
    {
        ProductImageStorageService service = CreateService(Path.Combine(Path.GetTempPath(), "ecommerce-oop-tests", Guid.NewGuid().ToString("N")));

        ProductImageProcessResult result = await service.ProcessMainImageAsync(
            uploadedImage: null,
            externalImageUrl: "ftp://cdn.invalid/producto.jpg",
            currentImageUrl: null,
            productSlug: "mouse-gamer",
            removeCurrentImage: false,
            cancellationToken: CancellationToken.None);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("http://"));
    }

    private static ProductImageStorageService CreateService(string webRootPath)
    {
        ProductImagesOptions options = new()
        {
            UploadsDirectory = "uploads/products",
            RequestPath = "/uploads/products",
            MaxFileSizeInBytes = 5 * 1024 * 1024,
            AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"],
            AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"]
        };

        return new ProductImageStorageService(
            new FakeWebHostEnvironment
            {
                ContentRootPath = webRootPath,
                WebRootPath = webRootPath
            },
            Options.Create(options),
            NullLogger<ProductImageStorageService>.Instance);
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "PlataformaECommerce.Web";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
