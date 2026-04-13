using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Web.Initialization;

namespace PlataformaECommerce.Maintenance;

internal sealed class MaintenanceCommandDispatcher
{
    private readonly IServiceProvider _services;
    private readonly ILogger<MaintenanceCommandDispatcher> _logger;

    public MaintenanceCommandDispatcher(
        IServiceProvider services,
        ILogger<MaintenanceCommandDispatcher> logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task DispatchAsync(MaintenanceCommandRequest commandRequest, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(commandRequest);

        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        IServiceProvider serviceProvider = scope.ServiceProvider;
        IHostEnvironment hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();

        if (!hostEnvironment.IsDevelopment())
        {
            throw new InvalidOperationException("La normalización legacy solo puede ejecutarse desde el proceso de mantenimiento en entorno Development.");
        }

        DevelopmentLegacyTenantDataNormalizer normalizer = serviceProvider.GetRequiredService<DevelopmentLegacyTenantDataNormalizer>();
        ITenantContextAccessor tenantContextAccessor = serviceProvider.GetRequiredService<ITenantContextAccessor>();

        _logger.LogWarning(
            "Se ejecutará el comando de mantenimiento '{CommandName}' fuera del host web. Entorno: {EnvironmentName}. TenantOverride: {TenantOverride}.",
            commandRequest.CommandName,
            hostEnvironment.EnvironmentName,
            commandRequest.TenantOverride ?? "<none>");

        if (!string.IsNullOrWhiteSpace(commandRequest.TenantOverride))
        {
            using IDisposable tenantScope = tenantContextAccessor.BeginTenantScope(commandRequest.TenantOverride);
            await LegacyTenantMaintenanceCommands.ExecuteAsync(normalizer, _logger, commandRequest, cancellationToken).ConfigureAwait(false);
            return;
        }

        await LegacyTenantMaintenanceCommands.ExecuteAsync(normalizer, _logger, commandRequest, cancellationToken).ConfigureAwait(false);
    }
}
