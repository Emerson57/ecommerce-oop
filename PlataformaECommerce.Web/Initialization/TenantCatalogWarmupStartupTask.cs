using Microsoft.Extensions.Logging;
using PlataformaECommerce.Application.Interfaces.Services.Common;

namespace PlataformaECommerce.Web.Initialization;

internal sealed class TenantCatalogWarmupStartupTask : IStartupInitializationTask
{
    private readonly ITenantCatalogService _tenantCatalogService;
    private readonly ILogger<TenantCatalogWarmupStartupTask> _logger;

    public TenantCatalogWarmupStartupTask(
        ITenantCatalogService tenantCatalogService,
        ILogger<TenantCatalogWarmupStartupTask> logger)
    {
        _tenantCatalogService = tenantCatalogService ?? throw new ArgumentNullException(nameof(tenantCatalogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "TenantCatalogWarmup";

    public StartupInitializationCategory Category => StartupInitializationCategory.Warmup;

    public StartupInitializationExecutionMode ExecutionMode => StartupInitializationExecutionMode.Always;

    public int Order => 100;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<PlataformaECommerce.Application.Common.SaaS.TenantDefinition> tenants = await _tenantCatalogService
            .GetConfiguredTenantsAsync(cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Warmup no destructivo del catálogo SaaS completado. Tenants configurados: {TenantCount}.",
            tenants.Count);
    }
}
