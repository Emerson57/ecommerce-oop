using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
        ValidateEnvironment(commandRequest, hostEnvironment);

        _logger.LogWarning(
            "Se ejecutará el comando de mantenimiento '{CommandName}' fuera del host web. Entorno: {EnvironmentName}. TenantOverride: {TenantOverride}.",
            commandRequest.CommandName,
            hostEnvironment.EnvironmentName,
            commandRequest.TenantOverride ?? "<none>");

        await using IAsyncDisposable? lockHandle = await TryAcquireExclusiveLockAsync(serviceProvider, commandRequest, cancellationToken).ConfigureAwait(false);
        await ExecuteWithinTenantScopeAsync(serviceProvider, commandRequest, cancellationToken).ConfigureAwait(false);
    }

    private void ValidateEnvironment(MaintenanceCommandRequest commandRequest, IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(commandRequest);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        if (LegacyTenantMaintenanceCommands.RequiresDevelopmentEnvironment(commandRequest.CommandName)
            && !hostEnvironment.IsDevelopment())
        {
            throw new InvalidOperationException("La normalización legacy solo puede ejecutarse desde el proceso de mantenimiento en entorno Development.");
        }
    }

    private async Task<IAsyncDisposable?> TryAcquireExclusiveLockAsync(
        IServiceProvider serviceProvider,
        MaintenanceCommandRequest commandRequest,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(commandRequest);

        if (!RequiresExclusiveLock(commandRequest.CommandName))
        {
            return null;
        }

        SqlServerMaintenanceCommandLock maintenanceCommandLock = serviceProvider.GetRequiredService<SqlServerMaintenanceCommandLock>();
        string resource = GetLockResource(commandRequest);
        return await maintenanceCommandLock.AcquireAsync(resource, cancellationToken).ConfigureAwait(false);
    }

    private async Task ExecuteWithinTenantScopeAsync(
        IServiceProvider serviceProvider,
        MaintenanceCommandRequest commandRequest,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(commandRequest);

        if (!string.IsNullOrWhiteSpace(commandRequest.TenantOverride))
        {
            PlataformaECommerce.Application.Interfaces.Services.Common.ITenantContextAccessor tenantContextAccessor = serviceProvider.GetRequiredService<PlataformaECommerce.Application.Interfaces.Services.Common.ITenantContextAccessor>();
            using IDisposable tenantScope = tenantContextAccessor.BeginTenantScope(commandRequest.TenantOverride);
            await ExecuteCommandAsync(serviceProvider, commandRequest, cancellationToken).ConfigureAwait(false);
            return;
        }

        await ExecuteCommandAsync(serviceProvider, commandRequest, cancellationToken).ConfigureAwait(false);
    }

    private Task ExecuteCommandAsync(
        IServiceProvider serviceProvider,
        MaintenanceCommandRequest commandRequest,
        CancellationToken cancellationToken)
    {
        if (LegacyTenantMaintenanceCommands.IsSupported(commandRequest.CommandName))
        {
            return LegacyTenantMaintenanceCommands.ExecuteAsync(
                serviceProvider.GetRequiredService<PlataformaECommerce.Web.Initialization.DevelopmentLegacyTenantDataNormalizer>(),
                _logger,
                commandRequest,
                cancellationToken);
        }

        if (SaaSBootstrapMaintenanceCommands.IsSupported(commandRequest.CommandName))
        {
            return SaaSBootstrapMaintenanceCommands.ExecuteAsync(serviceProvider, _logger, commandRequest, cancellationToken);
        }

        throw new InvalidOperationException($"El comando de mantenimiento '{commandRequest.CommandName}' no está soportado.");
    }

    private static bool RequiresExclusiveLock(string commandName)
    {
        return LegacyTenantMaintenanceCommands.RequiresExclusiveLock(commandName)
            || SaaSBootstrapMaintenanceCommands.RequiresExclusiveLock(commandName);
    }

    private static string GetLockResource(MaintenanceCommandRequest commandRequest)
    {
        if (LegacyTenantMaintenanceCommands.IsSupported(commandRequest.CommandName))
        {
            return LegacyTenantMaintenanceCommands.GetLockResource(commandRequest);
        }

        if (SaaSBootstrapMaintenanceCommands.IsSupported(commandRequest.CommandName))
        {
            return SaaSBootstrapMaintenanceCommands.GetLockResource(commandRequest);
        }

        throw new InvalidOperationException($"El comando de mantenimiento '{commandRequest.CommandName}' no tiene recurso de lock definido.");
    }
}
