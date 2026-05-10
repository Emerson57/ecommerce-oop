using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Valida la clave de configuración <c>AllowedHosts</c> de ASP.NET Core antes de construir el host,
/// evitando despliegues en Production con comodín global o listas vacías.
/// </summary>
public static class AllowedHostsConfigurationGuard
{
    /// <summary>
    /// En Production exige al menos un host explícito y rechaza el comodín <c>*</c> (cualquier host).
    /// </summary>
    public static void Validate(IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        if (!environment.IsProduction())
        {
            return;
        }

        IReadOnlyList<string> entries = ResolveEffectiveAllowedHosts(configuration);

        if (entries.Count == 0)
        {
            throw new InvalidOperationException(
                "AllowedHosts no está configurado o quedó vacío en Production. Defina hosts explícitos (por ejemplo AllowedHosts=midominio.com;www.midominio.com en variables de entorno) e incluya los dominios de storefront/backoffice y sondas (127.0.0.1) si aplica. Consulte docs/SECURITY.md.");
        }

        if (entries.Any(static host => string.Equals(host, "*", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                "AllowedHosts no puede contener '*' en Production. Ese valor desactiva el filtrado de host y expone a Host Header Injection. Use dominios explícitos o patrones acotados como '*.midominio.com'. Consulte docs/SECURITY.md.");
        }
    }

    /// <summary>
    /// Resuelve la lista efectiva de hosts permitidos tal como la consume el host filtering de ASP.NET Core
    /// (cadena separada por ';' o arreglo en JSON).
    /// </summary>
    public static IReadOnlyList<string> ResolveEffectiveAllowedHosts(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        IConfigurationSection section = configuration.GetSection("AllowedHosts");
        if (!section.Exists())
        {
            return [];
        }

        IConfigurationSection[] children = section.GetChildren().ToArray();
        if (children.Length > 0)
        {
            List<string> fromArray = [];
            foreach (IConfigurationSection child in children)
            {
                if (!string.IsNullOrWhiteSpace(child.Value))
                {
                    fromArray.Add(child.Value.Trim());
                }
            }

            if (fromArray.Count > 0)
            {
                return fromArray;
            }
        }

        string? value = section.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static s => s.Length > 0)
            .ToArray();
    }
}
