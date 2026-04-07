using Microsoft.EntityFrameworkCore;
using PlataformaECommerce.Infrastructure.Persistence.Context;

namespace PlataformaECommerce.Infrastructure.Services.Common;

/// <summary>
/// Centraliza la validación de disponibilidad del esquema SaaS persistente versionado por migraciones.
/// </summary>
internal static class SaaSPersistenceMigrationGuard
{
    private const string RequiredMigrationName = "FinalizeSaaSProvisioningPhase6";

    /// <summary>
    /// Determina si el esquema persistente SaaS requerido por la solución ya fue aplicado sobre la base de datos actual.
    /// </summary>
    /// <param name="dbContext">Contexto transaccional asociado a la base de datos.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns><see langword="true"/> cuando la migración requerida ya está aplicada.</returns>
    internal static async Task<bool> IsPersistentCatalogReadyAsync(
        ECommerceDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        IEnumerable<string> appliedMigrations = await dbContext.Database
            .GetAppliedMigrationsAsync(cancellationToken)
            .ConfigureAwait(false);

        return appliedMigrations.Any(migrationId =>
            migrationId.EndsWith($"_{RequiredMigrationName}", StringComparison.Ordinal));
    }
}
