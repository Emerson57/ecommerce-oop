using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Impide configuraciones peligrosas de EF Core en entornos reales (migraciones o recreación automática al iniciar).
/// </summary>
public static class EntityFrameworkProductionGuard
{
    /// <summary>
    /// Nombre de sección opcional para operaciones de base de datos en tiempo de ejecución.
    /// </summary>
    public const string SectionName = "DatabaseOperations";

    /// <summary>
    /// En Production rechaza migraciones automáticas al arranque; el esquema debe aplicarse en pipeline o ventana controlada.
    /// </summary>
    public static void Validate(IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        if (!environment.IsProduction())
        {
            return;
        }

        if (configuration.GetValue($"{SectionName}:ApplyEfMigrationsOnStartup", false))
        {
            throw new InvalidOperationException(
                $"{SectionName}:ApplyEfMigrationsOnStartup no está permitido en Production. "
                + "Aplique migraciones antes o durante el despliegue con 'dotnet ef database update' o un script idempotente generado con 'dotnet ef migrations script --idempotent'. "
                + "Consulte docs/database-migrations.md.");
        }
    }
}
