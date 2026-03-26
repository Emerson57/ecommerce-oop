using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Contracts.Products;
using PlataformaECommerce.Web.Controllers;
using PlataformaECommerce.Web.OpenApi;

namespace PlataformaECommerce.Tests.Web.Controllers;

[TestFixture]
public class AdminProductsControllerTests
{
    [Test]
    public void Controller_ClaseProtegida_UsaPoliticaAdminOnly()
    {
        AuthorizeAttribute? attribute = typeof(AdminProductsController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .OfType<AuthorizeAttribute>()
            .SingleOrDefault();

        Assert.That(attribute?.Policy, Is.EqualTo(AuthorizationPolicies.AdminOnly));
    }

    [Test]
    public void Controller_GrupoSwaggerEsAdmin()
    {
        ApiExplorerSettingsAttribute? attribute = typeof(AdminProductsController)
            .GetCustomAttributes(typeof(ApiExplorerSettingsAttribute), inherit: true)
            .OfType<ApiExplorerSettingsAttribute>()
            .SingleOrDefault();

        Assert.That(attribute?.GroupName, Is.EqualTo(SwaggerGroups.Admin));
    }

    [Test]
    public async Task Activate_ComandoValido_RetornaOk()
    {
        FakeProductApplicationService service = new();
        AdminProductsController controller = new(service);
        Guid routeId = Guid.NewGuid();

        ActionResult<ProductResponseDto> result = await controller.Activate(
            routeId,
            new ActivateProductRequest
            {
                Reason = "Reactivación administrativa"
            },
            CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        Assert.That(service.LastActivateCommand?.ProductId, Is.EqualTo(routeId));
    }

    [Test]
    public async Task Activate_ComandoNulo_RetornaBadRequest()
    {
        FakeProductApplicationService service = new();
        AdminProductsController controller = new(service);

        ActionResult<ProductResponseDto> result = await controller.Activate(Guid.NewGuid(), null!, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Deactivate_ComandoOpcional_RetornaOk()
    {
        FakeProductApplicationService service = new();
        AdminProductsController controller = new(service);
        Guid routeId = Guid.NewGuid();

        ActionResult<ProductResponseDto> result = await controller.Deactivate(routeId, null, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        Assert.That(service.LastDeactivateCommand?.ProductId, Is.EqualTo(routeId));
    }

    [Test]
    public async Task Feature_ComandoValido_RetornaOk()
    {
        FakeProductApplicationService service = new();
        AdminProductsController controller = new(service);
        Guid routeId = Guid.NewGuid();

        ActionResult<ProductResponseDto> result = await controller.Feature(
            routeId,
            new FeatureProductRequest
            {
                Reason = "Campaña principal"
            },
            CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        Assert.That(service.LastFeatureCommand?.ProductId, Is.EqualTo(routeId));
    }

    [Test]
    public async Task Unfeature_ComandoOpcional_RetornaOk()
    {
        FakeProductApplicationService service = new();
        AdminProductsController controller = new(service);
        Guid routeId = Guid.NewGuid();

        ActionResult<ProductResponseDto> result = await controller.Unfeature(routeId, null, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        Assert.That(service.LastUnfeatureCommand?.ProductId, Is.EqualTo(routeId));
    }

    [Test]
    public async Task UpdateStock_ComandoValido_RetornaOk()
    {
        FakeProductApplicationService service = new();
        AdminProductsController controller = new(service);
        Guid routeId = Guid.NewGuid();

        ActionResult<ProductResponseDto> result = await controller.UpdateStock(
            routeId,
            new UpdateProductStockRequest
            {
                UpdateType = StockUpdateType.Increase,
                Quantity = 5,
                Reason = "Ajuste operativo"
            },
            CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        Assert.That(service.LastUpdateStockCommand?.ProductId, Is.EqualTo(routeId));
    }

    [Test]
    public async Task UpdateStock_ComandoNulo_RetornaBadRequest()
    {
        FakeProductApplicationService service = new();
        AdminProductsController controller = new(service);

        ActionResult<ProductResponseDto> result = await controller.UpdateStock(Guid.NewGuid(), null!, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task ApplyPromotion_ComandoValido_RetornaOk()
    {
        FakeProductApplicationService service = new();
        AdminProductsController controller = new(service);
        Guid routeId = Guid.NewGuid();

        ActionResult<ProductResponseDto> result = await controller.ApplyPromotion(
            routeId,
            new ApplyProductPromotionRequest
            {
                DiscountPercentage = 10m,
                Reason = "Campaña"
            },
            CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        Assert.That(service.LastApplyPromotionCommand?.ProductId, Is.EqualTo(routeId));
    }

    [Test]
    public async Task ApplyPromotion_ComandoNulo_RetornaBadRequest()
    {
        FakeProductApplicationService service = new();
        AdminProductsController controller = new(service);

        ActionResult<ProductResponseDto> result = await controller.ApplyPromotion(Guid.NewGuid(), null!, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task RemovePromotion_ComandoOpcional_RetornaOk()
    {
        FakeProductApplicationService service = new();
        AdminProductsController controller = new(service);
        Guid routeId = Guid.NewGuid();

        ActionResult<ProductResponseDto> result = await controller.RemovePromotion(routeId, null, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        Assert.That(service.LastRemovePromotionCommand?.ProductId, Is.EqualTo(routeId));
    }

    private sealed class FakeProductApplicationService : IProductApplicationService
    {
        public ActivateProductCommand? LastActivateCommand { get; private set; }
        public DeactivateProductCommand? LastDeactivateCommand { get; private set; }
        public FeatureProductCommand? LastFeatureCommand { get; private set; }
        public UnfeatureProductCommand? LastUnfeatureCommand { get; private set; }
        public UpdateProductStockCommand? LastUpdateStockCommand { get; private set; }
        public ApplyProductPromotionCommand? LastApplyPromotionCommand { get; private set; }
        public RemoveProductPromotionCommand? LastRemovePromotionCommand { get; private set; }

        public Task<Result<Guid>> CreatePhysicalProductAsync(CreatePhysicalProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(Guid.NewGuid()));

        public Task<Result<Guid>> CreateDigitalProductAsync(CreateDigitalProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(Guid.NewGuid()));

        public Task<Result<ProductImportResultDto>> ImportProductsAsync(ImportProductsCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<ProductResponseDto>> UpdateProductAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(CreateResponse()));

        public Task<Result<ProductResponseDto>> UpdateProductStockAsync(UpdateProductStockCommand command, CancellationToken cancellationToken = default)
        {
            LastUpdateStockCommand = command;
            return Task.FromResult(Result.Success(CreateResponse()));
        }

        public Task<Result<ProductResponseDto>> ActivateProductAsync(ActivateProductCommand command, CancellationToken cancellationToken = default)
        {
            LastActivateCommand = command;
            return Task.FromResult(Result.Success(CreateResponse()));
        }

        public Task<Result<ProductResponseDto>> DeactivateProductAsync(DeactivateProductCommand command, CancellationToken cancellationToken = default)
        {
            LastDeactivateCommand = command;
            return Task.FromResult(Result.Success(CreateResponse()));
        }

        public Task<Result<ProductResponseDto>> ApplyProductPromotionAsync(ApplyProductPromotionCommand command, CancellationToken cancellationToken = default)
        {
            LastApplyPromotionCommand = command;
            return Task.FromResult(Result.Success(CreateResponse()));
        }

        public Task<Result<ProductResponseDto>> RemoveProductPromotionAsync(RemoveProductPromotionCommand command, CancellationToken cancellationToken = default)
        {
            LastRemovePromotionCommand = command;
            return Task.FromResult(Result.Success(CreateResponse()));
        }

        public Task<Result<ProductResponseDto>> FeatureProductAsync(FeatureProductCommand command, CancellationToken cancellationToken = default)
        {
            LastFeatureCommand = command;
            return Task.FromResult(Result.Success(CreateResponse()));
        }

        public Task<Result<ProductResponseDto>> UnfeatureProductAsync(UnfeatureProductCommand command, CancellationToken cancellationToken = default)
        {
            LastUnfeatureCommand = command;
            return Task.FromResult(Result.Success(CreateResponse()));
        }

        public Task<Result<ProductDetailDto>> GetProductByIdAsync(GetProductByIdQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new ProductDetailDto()));

        public Task<Result<ProductQueryResultDto>> GetProductsAsync(GetProductsQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new ProductQueryResultDto()));

        private static ProductResponseDto CreateResponse()
        {
            return new ProductResponseDto
            {
                Id = Guid.NewGuid(),
                Name = "Producto prueba",
                Description = "Producto de prueba.",
                Sku = "PROD-001",
                Slug = "producto-prueba",
                Price = 90m,
                BasePrice = 100m,
                PromotionalPrice = 90m,
                Currency = "COP",
                Stock = 10,
                IsActive = true,
                IsFeatured = false,
                HasPromotion = true,
                CurrentDiscountPercentage = 10m,
                ProductType = TipoProducto.Fisico
            };
        }
    }
}
