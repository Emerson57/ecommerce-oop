using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Infrastructure.Configurations;

namespace PlataformaECommerce.Infrastructure.Services.Common;

/// <summary>
/// Resuelve el tenant activo a partir del contexto HTTP y de la configuración SaaS declarada.
/// </summary>
public sealed class TenantContextAccessor : ITenantContextAccessor
{
    private static readonly AsyncLocal<string?> TenantOverride = new();
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptionsMonitor<SaaSPlatformOptions> _optionsMonitor;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="TenantContextAccessor"/>.
    /// </summary>
    public TenantContextAccessor(
        IHttpContextAccessor httpContextAccessor,
        IOptionsMonitor<SaaSPlatformOptions> optionsMonitor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
    }

    /// <inheritdoc />
    public string TenantId => ResolveCurrentTenantId();

    /// <inheritdoc />
    public bool IsAvailable => !string.IsNullOrWhiteSpace(TenantId);

    /// <inheritdoc />
    public IDisposable BeginTenantScope(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("El tenant a forzar es obligatorio.", nameof(tenantId));
        }

        string? previousTenantId = TenantOverride.Value;
        TenantOverride.Value = tenantId.Trim();
        return new TenantScope(previousTenantId);
    }

    private string ResolveCurrentTenantId()
    {
        if (!string.IsNullOrWhiteSpace(TenantOverride.Value))
        {
            return TenantOverride.Value;
        }

        SaaSPlatformOptions options = _optionsMonitor.CurrentValue;
        IReadOnlyCollection<SaaSPlatformOptions.TenantOptions> enabledTenants = options.Tenants
            .Where(tenant => tenant.Enabled)
            .ToArray();

        if (enabledTenants.Count == 0)
        {
            throw new InvalidOperationException("La plataforma SaaS requiere al menos un tenant habilitado para resolver el aislamiento de datos.");
        }

        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            string? claimTenantId = httpContext.User.FindFirst(SecurityClaimTypes.TenantId)?.Value;
            string? resolvedFromClaim = TryResolveKnownTenantId(claimTenantId, enabledTenants);
            if (resolvedFromClaim is not null)
            {
                return resolvedFromClaim;
            }

            if (!string.IsNullOrWhiteSpace(options.ResolutionHeaderName)
                && httpContext.Request.Headers.TryGetValue(options.ResolutionHeaderName, out Microsoft.Extensions.Primitives.StringValues headerValues))
            {
                string? resolvedFromHeader = TryResolveKnownTenantId(headerValues.FirstOrDefault(), enabledTenants);
                if (resolvedFromHeader is not null)
                {
                    return resolvedFromHeader;
                }
            }

            if (options.ResolveTenantFromHost)
            {
                string host = httpContext.Request.Host.Host;
                SaaSPlatformOptions.TenantOptions? tenantByHost = enabledTenants.FirstOrDefault(tenant =>
                    tenant.Hostnames.Any(hostname => string.Equals(hostname?.Trim(), host, StringComparison.OrdinalIgnoreCase)));

                if (tenantByHost is not null)
                {
                    return tenantByHost.TenantId.Trim();
                }
            }
        }

        string? activeTenantId = TryResolveKnownTenantId(options.ActiveTenantId, enabledTenants);
        if (activeTenantId is not null)
        {
            return activeTenantId;
        }

        return enabledTenants.First().TenantId.Trim();
    }

    private static string? TryResolveKnownTenantId(
        string? candidate,
        IReadOnlyCollection<SaaSPlatformOptions.TenantOptions> enabledTenants)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return null;
        }

        string normalized = candidate.Trim();
        SaaSPlatformOptions.TenantOptions? tenant = enabledTenants.FirstOrDefault(current =>
            string.Equals(current.TenantId, normalized, StringComparison.OrdinalIgnoreCase));

        return tenant?.TenantId.Trim();
    }

    private sealed class TenantScope(string? previousTenantId) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            TenantOverride.Value = previousTenantId;
            _disposed = true;
        }
    }
}
