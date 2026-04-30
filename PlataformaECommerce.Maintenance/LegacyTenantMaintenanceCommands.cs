using Microsoft.Extensions.Logging;
using PlataformaECommerce.Web.Initialization;

namespace PlataformaECommerce.Maintenance;

internal static class LegacyTenantMaintenanceCommands
{
    public const string NormalizeLegacyTenantData = "normalize-legacy-tenant-data";
    public const string InspectLegacyTenantData = "inspect-legacy-tenant-data";
    public const string Help = "help";

    public static bool IsSupported(string commandName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);

        return string.Equals(commandName, NormalizeLegacyTenantData, StringComparison.Ordinal)
            || string.Equals(commandName, InspectLegacyTenantData, StringComparison.Ordinal);
    }

    public static bool RequiresDevelopmentEnvironment(string commandName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        return IsSupported(commandName);
    }

    public static bool RequiresExclusiveLock(string commandName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        return string.Equals(commandName, NormalizeLegacyTenantData, StringComparison.Ordinal);
    }

    public static string GetLockResource(MaintenanceCommandRequest commandRequest)
    {
        ArgumentNullException.ThrowIfNull(commandRequest);

        return commandRequest.CommandName switch
        {
            NormalizeLegacyTenantData => BuildTenantScopedResource("maintenance:normalize-legacy-tenant-data", commandRequest.TenantOverride),
            _ => throw new InvalidOperationException($"El comando legacy '{commandRequest.CommandName}' no requiere lock soportado.")
        };
    }

    public static bool IsHelpToken(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return string.Equals(value, Help, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "--help", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "-h", StringComparison.OrdinalIgnoreCase);
    }

    public static Task ExecuteAsync(
        DevelopmentLegacyTenantDataNormalizer normalizer,
        ILogger logger,
        MaintenanceCommandRequest commandRequest,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(normalizer);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(commandRequest);

        return commandRequest.CommandName switch
        {
            NormalizeLegacyTenantData => normalizer.NormalizeAsync(cancellationToken),
            InspectLegacyTenantData => InspectLegacyTenantDataAsync(normalizer, logger, cancellationToken),
            _ => throw new InvalidOperationException($"El comando de mantenimiento '{commandRequest.CommandName}' no está soportado.")
        };
    }

    public static void WriteHelp(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteLine("PlataformaECommerce.Maintenance commands:");
        writer.WriteLine($"  {NormalizeLegacyTenantData} [--tenant=<tenantId>]  Ejecuta la corrección explícita de filas legacy sin tenant (requiere Maintenance:LegacyTenantNormalization:Enabled=true).");
        writer.WriteLine($"  {InspectLegacyTenantData} [--tenant=<tenantId>]    Inspecciona de forma no destructiva si existen filas legacy pendientes.");
    }

    private static string BuildTenantScopedResource(string prefix, string? tenantOverride)
    {
        string normalizedTenant = string.IsNullOrWhiteSpace(tenantOverride)
            ? "all-tenants"
            : tenantOverride.Trim().ToLowerInvariant();

        return $"{prefix}:{normalizedTenant}";
    }

    private static async Task InspectLegacyTenantDataAsync(
        DevelopmentLegacyTenantDataNormalizer normalizer,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        LegacyTenantNormalizationInspectionResult inspectionResult = await normalizer.InspectAsync(cancellationToken).ConfigureAwait(false);
        if (!inspectionResult.EnvironmentAllowsExecution)
        {
            logger.LogWarning("La inspección legacy se omite porque solo puede ejecutarse en Development.");
            return;
        }

        if (!inspectionResult.TenantResolved)
        {
            logger.LogWarning("La inspección legacy se omitió porque no se pudo resolver un tenant activo.");
            return;
        }

        logger.LogInformation(
            "Inspección legacy completada para el tenant '{TenantId}'. TotalRows: {TotalRows}. Products: {Products}. Categories: {Categories}. Users: {Users}. Orders: {Orders}. OrderItems: {OrderItems}. Carts: {Carts}. CartItems: {CartItems}.",
            inspectionResult.TenantId,
            inspectionResult.TotalRows,
            inspectionResult.Products,
            inspectionResult.Categories,
            inspectionResult.Users,
            inspectionResult.Orders,
            inspectionResult.OrderItems,
            inspectionResult.Carts,
            inspectionResult.CartItems);
    }
}
