using Microsoft.Extensions.Hosting;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Clasifica perfiles de entorno relevantes para endurecimiento de seguridad web.
/// </summary>
internal static class SecurityEnvironmentProfileExtensions
{
    public static bool IsQualityAssuranceLike(this IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        return hostEnvironment.IsStaging()
            || string.Equals(hostEnvironment.EnvironmentName, "QA", StringComparison.OrdinalIgnoreCase)
            || string.Equals(hostEnvironment.EnvironmentName, "QualityAssurance", StringComparison.OrdinalIgnoreCase)
            || string.Equals(hostEnvironment.EnvironmentName, "UAT", StringComparison.OrdinalIgnoreCase);
    }
}
