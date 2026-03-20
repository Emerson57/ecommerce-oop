using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Features.Products.Services;
using PlataformaECommerce.Application.Features.Products.Validators;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Application.Products;

[TestFixture]
public class ProductApplicationServiceTests
{
    [Test]
    public async Task ApplyProductPromotionAsync_ProductoDisponible_ReducePrecio()
    {
        ProductoFisico producto = CrearProductoFisico();
        producto.Activar();

        ProductApplicationService service = CrearServicio(producto);

        var result = await service.ApplyProductPromotionAsync(new ApplyProductPromotionCommand
        {
            ProductId = producto.Id,
            DiscountPercentage = 10m
        });

        Assert.That(result.Value.Price, Is.EqualTo(90m));
    }

    [Test]
    public async Task ApplyProductPromotionAsync_ProductoDisponible_ConservaPrecioBase()
    {
        ProductoFisico producto = CrearProductoFisico();
        producto.Activar();

        ProductApplicationService service = CrearServicio(producto);

        var result = await service.ApplyProductPromotionAsync(new ApplyProductPromotionCommand
        {
            ProductId = producto.Id,
            DiscountPercentage = 10m
        });

        Assert.That(result.Value.BasePrice, Is.EqualTo(100m));
    }

    [Test]
    public async Task RemoveProductPromotionAsync_PromocionActiva_RestauraPrecioBase()
    {
        ProductoFisico producto = CrearProductoFisico();
        producto.Activar();
        ProductApplicationService service = CrearServicio(producto);

        await service.ApplyProductPromotionAsync(new ApplyProductPromotionCommand
        {
            ProductId = producto.Id,
            DiscountPercentage = 10m
        });

        var result = await service.RemoveProductPromotionAsync(new RemoveProductPromotionCommand
        {
            ProductId = producto.Id
        });

        Assert.That(result.Value.Price, Is.EqualTo(100m));
    }

    [Test]
    public async Task RemoveProductPromotionAsync_PromocionActiva_EliminaEstadoPromocional()
    {
        ProductoFisico producto = CrearProductoFisico();
        producto.Activar();
        ProductApplicationService service = CrearServicio(producto);

        await service.ApplyProductPromotionAsync(new ApplyProductPromotionCommand
        {
            ProductId = producto.Id,
            DiscountPercentage = 10m
        });

        var result = await service.RemoveProductPromotionAsync(new RemoveProductPromotionCommand
        {
            ProductId = producto.Id
        });

        Assert.That(result.Value.HasPromotion, Is.False);
    }

    [Test]
    public async Task ApplyProductPromotionAsync_DescuentoInvalido_RetornaFallo()
    {
        ProductoFisico producto = CrearProductoFisico();
        producto.Activar();

        ProductApplicationService service = CrearServicio(producto);

        var result = await service.ApplyProductPromotionAsync(new ApplyProductPromotionCommand
        {
            ProductId = producto.Id,
            DiscountPercentage = 0m
        });

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task FeatureProductAsync_ProductoExistente_MarcaDestacado()
    {
        ProductoFisico producto = CrearProductoFisico();

        ProductApplicationService service = CrearServicio(producto);

        var result = await service.FeatureProductAsync(new FeatureProductCommand
        {
            ProductId = producto.Id
        });

        Assert.That(result.Value.IsFeatured, Is.True);
    }

    [Test]
    public async Task UnfeatureProductAsync_ProductoDestacado_QuitaDestacado()
    {
        ProductoFisico producto = CrearProductoFisico();
        producto.MarcarComoDestacado();

        ProductApplicationService service = CrearServicio(producto);

        var result = await service.UnfeatureProductAsync(new UnfeatureProductCommand
        {
            ProductId = producto.Id
        });

        Assert.That(result.Value.IsFeatured, Is.False);
    }

    [Test]
    public async Task UpdateProductStockAsync_StockInsuficiente_RetornaFalloControlado()
    {
        ProductoFisico producto = CrearProductoFisico();

        ProductApplicationService service = CrearServicio(producto);

        var result = await service.UpdateProductStockAsync(new UpdateProductStockCommand
        {
            ProductId = producto.Id,
            Quantity = producto.Stock + 1,
            UpdateType = StockUpdateType.Decrease,
            Reason = "Ajuste manual"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("Products.Domain"));
    }

    [Test]
    public async Task FeatureProductAsync_OperacionExitosa_RegistraEventoDeAuditoria()
    {
        ProductoFisico producto = CrearProductoFisico();
        FakeAuditTrailService auditTrailService = new();
        ProductApplicationService service = CrearServicio(producto, auditTrailService);

        await service.FeatureProductAsync(new FeatureProductCommand
        {
            ProductId = producto.Id
        });

        Assert.That(auditTrailService.RegisteredEvents.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetProductsAsync_ConsultaPaginada_RetornaMetadatosReales()
    {
        ProductoFisico productoUno = CrearProductoFisico();
        ProductoFisico productoDos = new(
            "Teclado Gamer",
            "Teclado profesional para pruebas.",
            new Sku("TECLADO-001"),
            new Money(200m, "COP"),
            15,
            "teclado-gamer",
            null,
            null,
            null,
            null,
            0.8m,
            5m,
            15m,
            45m,
            true);

        ProductApplicationService service = CrearServicio(productoUno, productoDos);

        var result = await service.GetProductsAsync(new GetProductsQuery
        {
            PageNumber = 1,
            PageSize = 1,
            SortBy = "name"
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.TotalCount, Is.EqualTo(2));
        Assert.That(result.Value.ReturnedCount, Is.EqualTo(1));
        Assert.That(result.Value.TotalPages, Is.EqualTo(2));
        Assert.That(result.Value.HasNextPage, Is.True);
    }

    private static ProductApplicationService CrearServicio(
        Producto producto,
        FakeAuditTrailService? auditTrailService = null)
    {
        FakeProductRepository productRepository = new(producto);
        FakeUnitOfWork unitOfWork = new();

        return new ProductApplicationService(
            productRepository,
            auditTrailService ?? new FakeAuditTrailService(),
            unitOfWork,
            new CreatePhysicalProductCommandValidator(),
            new CreateDigitalProductCommandValidator(),
            new UpdateProductCommandValidator(),
            new UpdateProductStockCommandValidator());
    }

    private static ProductApplicationService CrearServicio(
        Producto productoUno,
        Producto productoDos,
        FakeAuditTrailService? auditTrailService = null)
    {
        FakeProductRepository productRepository = new(productoUno, productoDos);
        FakeUnitOfWork unitOfWork = new();

        return new ProductApplicationService(
            productRepository,
            auditTrailService ?? new FakeAuditTrailService(),
            unitOfWork,
            new CreatePhysicalProductCommandValidator(),
            new CreateDigitalProductCommandValidator(),
            new UpdateProductCommandValidator(),
            new UpdateProductStockCommandValidator());
    }

    private static ProductoFisico CrearProductoFisico()
    {
        return new ProductoFisico(
            "Mouse Gamer",
            "Mouse ergonómico para pruebas.",
            new Sku("MOUSE-001"),
            new Money(100m, "COP"),
            10,
            "mouse-gamer",
            null,
            null,
            null,
            null,
            0.2m,
            4m,
            7m,
            12m,
            true);
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        private readonly List<Producto> _products;

        public FakeProductRepository(params Producto[] products)
        {
            _products = products.ToList();
        }

        public Task<IReadOnlyCollection<Producto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Producto>>(_products.AsReadOnly());
        }

        public Task<Producto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_products.FirstOrDefault(product => product.Id == id));
        }

        public Task<Producto?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_products.FirstOrDefault(product => product.Sku.Value == sku));
        }

        public Task<IReadOnlyCollection<Producto>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<Producto> result = _products.Where(product => product.Activo).ToArray();
            return Task.FromResult(result);
        }

        public Task<IReadOnlyCollection<Producto>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<Producto> result = _products.Where(product => product.Destacado).ToArray();
            return Task.FromResult(result);
        }

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_products.Any(product => product.Id == id));
        }

        public Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_products.Any(product => product.Sku.Value == sku));
        }

        public Task AddAsync(Producto producto, CancellationToken cancellationToken = default)
        {
            _products.Add(producto);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Producto producto, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _products.RemoveAll(product => product.Id == id);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAuditTrailService : IAuditTrailService
    {
        public List<(Guid AggregateId, string AggregateType, string Module, string Action)> RegisteredEvents { get; } = new();

        public Task RegisterAsync(
            Guid aggregateId,
            string aggregateType,
            string module,
            string action,
            string detail,
            IReadOnlyDictionary<string, string>? metadata = null,
            CancellationToken cancellationToken = default)
        {
            RegisteredEvents.Add((aggregateId, aggregateType, module, action));
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
