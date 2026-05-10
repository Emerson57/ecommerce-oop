using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Impide desplegar entornos reales con la UI interactiva de creación de administradores habilitada.
/// </summary>
public static class AdministratorCreationConfigurationGuard
{
    /// <summary>
    /// En Production y Staging, <see cref="AdminUsersBackofficeOptions.EnableAdministratorCreationUi"/> debe ser <c>false</c>.
    /// </summary>
    public static void Validate(IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        if (!RequiresStrictAdministratorCreationPolicy(environment))
        {
            return;
        }

        bool enabled = configuration.GetValue<bool>($"{AdminUsersBackofficeOptions.SectionName}:EnableAdministratorCreationUi");
        if (!enabled)
        {
            return;
        }

        throw new InvalidOperationException(
            $"En el entorno '{environment.EnvironmentName}' no puede estar habilitada la creación interactiva de administradores ({AdminUsersBackofficeOptions.SectionName}:EnableAdministratorCreationUi). " +
            "Use el bootstrap controlado (comando de mantenimiento o sección Bootstrap:SuperUser con secretos/variables de entorno) y deje esta opción en false. Consulte docs/SECURITY.md.");
    }

    private static bool RequiresStrictAdministratorCreationPolicy(IHostEnvironment environment)
    {
        return environment.IsProduction()
            || string.Equals(environment.EnvironmentName, Environments.Staging, StringComparison.OrdinalIgnoreCase);
    }
}
