using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PlataformaECommerce.Infrastructure.Configurations;

namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Valida que la configuración SaaS no contenga datos de demostración ni valores inseguros en entornos reales.
/// </summary>
public static class SaaSPlatformProductionGuard
{
    private static readonly string[] ForbiddenTenantIds =
    [
        "novashop-default",
        "tenant-default",
        "sample",
        "test-tenant"
    ];

    /// <summary>
    /// En Production exige tenants comerciales coherentes (sin dominios reservados ni identidades de ejemplo).
    /// </summary>
    public static void Validate(IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        if (!environment.IsProduction())
        {
            return;
        }

        SaaSPlatformOptions? options = configuration.GetSection(SaaSPlatformOptions.SectionName).Get<SaaSPlatformOptions>();
        if (options is null)
        {
            throw new InvalidOperationException(
                $"La sección '{SaaSPlatformOptions.SectionName}' no está definida o no pudo enlazarse. Configure el tenant de producción. Consulte docs/SECURITY.md.");
        }

        List<string> errors = [];
        ValidateCore(options, errors);

        string? clientId = configuration[$"{ClientExperienceOptions.SectionName}:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            errors.Add($"Debe definirse '{ClientExperienceOptions.SectionName}:ClientId' y coincidir con el tenant activo.");
        }
        else if (!string.Equals(clientId.Trim(), options.ActiveTenantId.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(
                $"'{ClientExperienceOptions.SectionName}:ClientId' ('{clientId.Trim()}') debe coincidir con '{SaaSPlatformOptions.SectionName}:ActiveTenantId' ('{options.ActiveTenantId.Trim()}').");
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "La configuración SaaS / experiencia de cliente no cumple las reglas de entorno real: "
                + string.Join(" ", errors)
                + " Consulte docs/SECURITY.md y las variables SaaS__Tenants__0__*.");
        }
    }

    private static void ValidateCore(SaaSPlatformOptions options, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(options.ActiveTenantId))
        {
            errors.Add($"{SaaSPlatformOptions.SectionName}:ActiveTenantId es obligatorio.");
        }

        IReadOnlyList<SaaSPlatformOptions.TenantOptions> enabledTenants = options.Tenants.Where(t => t.Enabled).ToArray();
        if (enabledTenants.Count == 0)
        {
            errors.Add($"Debe existir al menos un tenant habilitado en {SaaSPlatformOptions.SectionName}:Tenants.");
        }

        HashSet<string> tenantIds = new(StringComparer.OrdinalIgnoreCase);
        foreach (SaaSPlatformOptions.TenantOptions tenant in options.Tenants.Where(t => t.Enabled))
        {
            string tid = tenant.TenantId.Trim();
            if (string.IsNullOrWhiteSpace(tid))
            {
                errors.Add("Cada tenant habilitado requiere TenantId no vacío.");
                continue;
            }

            if (!tenantIds.Add(tid))
            {
                errors.Add($"TenantId duplicado o conflictivo: '{tid}'.");
            }

            if (ForbiddenTenantIds.Contains(tid, StringComparer.OrdinalIgnoreCase))
            {
                errors.Add($"El TenantId '{tid}' está reservado para demostración y no puede usarse en este entorno.");
            }

            if (tid.Length < 3 || tid.Length > 80 || tid.Contains(' ', StringComparison.Ordinal))
            {
                errors.Add($"El TenantId '{tid}' tiene un formato inválido (use 3-80 caracteres sin espacios).");
            }

            if (string.IsNullOrWhiteSpace(tenant.Currency) || tenant.Currency.Trim().Length != 3
                || !tenant.Currency.Trim().All(static c => char.IsAsciiLetter(c)))
            {
                errors.Add($"El tenant '{tid}' requiere Currency en formato ISO 4217 de tres letras (p. ej. COP).");
            }

            if (string.IsNullOrWhiteSpace(tenant.Country) || tenant.Country.Trim().Length != 2
                || !tenant.Country.Trim().All(static c => char.IsAsciiLetter(c)))
            {
                errors.Add($"El tenant '{tid}' requiere Country en formato ISO 3166-1 alpha-2 (p. ej. CO).");
            }

            if (!TryValidateSupportEmail(tenant.SupportEmail, out string? emailError))
            {
                errors.Add($"El tenant '{tid}' tiene SupportEmail inválido o no permitido: {emailError}");
            }

            if (options.ResolveTenantFromHost)
            {
                if (tenant.Hostnames is null || tenant.Hostnames.Count == 0
                    || tenant.Hostnames.All(static h => string.IsNullOrWhiteSpace(h)))
                {
                    errors.Add(
                        $"El tenant '{tid}' requiere al menos un hostname en {SaaSPlatformOptions.SectionName}:Tenants:*:Hostnames cuando ResolveTenantFromHost es true.");
                }
                else
                {
                    foreach (string? hostname in tenant.Hostnames)
                    {
                        if (!IsProductionSafeHostname(hostname, out string? hostError))
                        {
                            errors.Add($"Hostname no permitido para '{tid}': {hostError}");
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(tenant.Provisioning.BootstrapSuperUserEmail)
                && !IsProductionSafeEmail(tenant.Provisioning.BootstrapSuperUserEmail, out string? bootError))
            {
                errors.Add($"BootstrapSuperUserEmail del tenant '{tid}' no es válido para producción: {bootError}");
            }

            if (tenant.Provisioning.SeedDemoCatalog)
            {
                errors.Add(
                    $"En Production el tenant '{tid}' no puede tener {nameof(tenant.Provisioning.SeedDemoCatalog)} en true. "
                    + "Desactívelo en configuración; el catálogo demo solo es aceptable en entornos no productivos. "
                    + "La siembra controlada se ejecuta con el proyecto Maintenance, no al publicar sin revisión.");
            }
        }

        SaaSPlatformOptions.TenantOptions? active = enabledTenants.FirstOrDefault(t =>
            string.Equals(t.TenantId.Trim(), options.ActiveTenantId.Trim(), StringComparison.OrdinalIgnoreCase));
        if (active is null && !string.IsNullOrWhiteSpace(options.ActiveTenantId))
        {
            errors.Add(
                $"{SaaSPlatformOptions.SectionName}:ActiveTenantId ('{options.ActiveTenantId.Trim()}') no coincide con ningún tenant habilitado.");
        }
    }

    private static bool TryValidateSupportEmail(string? email, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(email))
        {
            error = "correo vacío.";
            return false;
        }

        if (!new EmailAddressAttribute().IsValid(email.Trim()))
        {
            error = "formato inválido.";
            return false;
        }

        if (!IsProductionSafeEmail(email, out string? unsafeReason))
        {
            error = unsafeReason;
            return false;
        }

        return true;
    }

    private static bool IsProductionSafeEmail(string? email, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(email))
        {
            error = "valor vacío.";
            return false;
        }

        string normalized = email.Trim();
        int at = normalized.LastIndexOf('@');
        if (at <= 0 || at >= normalized.Length - 1)
        {
            error = "dominio ausente.";
            return false;
        }

        string domain = normalized[(at + 1)..].ToLowerInvariant();
        if (domain.EndsWith(".example", StringComparison.Ordinal)
            || domain.EndsWith(".example.com", StringComparison.Ordinal)
            || domain.EndsWith(".test", StringComparison.Ordinal)
            || domain.EndsWith(".invalid", StringComparison.Ordinal)
            || domain.EndsWith(".local", StringComparison.Ordinal)
            || domain.EndsWith(".localhost", StringComparison.Ordinal)
            || string.Equals(domain, "example.com", StringComparison.Ordinal)
            || domain.Contains("novashop.", StringComparison.Ordinal))
        {
            error = "dominio reservado o de ejemplo.";
            return false;
        }

        return true;
    }

    private static bool IsProductionSafeHostname(string? hostname, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(hostname))
        {
            error = "hostname vacío.";
            return false;
        }

        string host = hostname.Trim().TrimEnd('.').ToLowerInvariant();
        if (host.Contains(' ', StringComparison.Ordinal) || host.Contains('/', StringComparison.Ordinal))
        {
            error = "caracteres no permitidos.";
            return false;
        }

        if (host == "localhost" || host == "127.0.0.1" || host == "::1")
        {
            error = "localhost / loopback no son dominios de producción.";
            return false;
        }

        if (host.EndsWith(".local", StringComparison.Ordinal)
            || host.EndsWith(".example", StringComparison.Ordinal)
            || host.EndsWith(".test", StringComparison.Ordinal)
            || host.EndsWith(".invalid", StringComparison.Ordinal)
            || host.EndsWith(".localhost", StringComparison.Ordinal))
        {
            error = "sufijo reservado (.local, .example, .test, .invalid).";
            return false;
        }

        if (host.Contains("novashop", StringComparison.Ordinal))
        {
            error = "identidad de demostración (novashop).";
            return false;
        }

        return true;
    }
}
