using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PlataformaECommerce.Application.Interfaces.Services.Common;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Resuelve claves de partición estables para rate limiting a partir del tenant, el actor efectivo y la superficie lógica del endpoint.
/// </summary>
public sealed class RateLimitPartitionKeyResolver
{
    private readonly ITenantContextAccessor _tenantContextAccessor;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="RateLimitPartitionKeyResolver"/>.
    /// </summary>
    /// <param name="tenantContextAccessor">Accesor al tenant actual del contexto de ejecución.</param>
    public RateLimitPartitionKeyResolver(ITenantContextAccessor tenantContextAccessor)
    {
        _tenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));
    }

    /// <summary>
    /// Construye una clave de partición estable para una política de rate limiting dada.
    /// </summary>
    /// <param name="httpContext">Contexto HTTP de la solicitud actual.</param>
    /// <param name="policyName">Nombre de la política lógica aplicada al endpoint.</param>
    /// <returns>Clave normalizada y estable de partición.</returns>
    public string Resolve(HttpContext httpContext, string policyName)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        if (string.IsNullOrWhiteSpace(policyName))
        {
            throw new ArgumentException("El nombre de la política es obligatorio.", nameof(policyName));
        }

        string tenantSegment = ResolveTenantSegment();
        string actorSegment = ResolveActorSegment(httpContext);
        string endpointGroupSegment = RateLimitEndpointGroupResolver.Resolve(httpContext);

        return $"policy:{policyName.Trim().ToLowerInvariant()}|tenant:{tenantSegment}|actor:{actorSegment}|surface:{endpointGroupSegment}";
    }

    private string ResolveTenantSegment()
    {
        return _tenantContextAccessor.IsAvailable
            ? NormalizeValue(_tenantContextAccessor.TenantId, "default")
            : "default";
    }

    private static string ResolveActorSegment(HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            string? userIdentifier = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContext.User.Identity?.Name
                ?? httpContext.User.FindFirstValue(ClaimTypes.Email);

            return $"user:{NormalizeValue(userIdentifier, "authenticated")}";
        }

        return $"ip:{NormalizeIp(httpContext.Connection.RemoteIpAddress)}";
    }

    private static string NormalizeIp(System.Net.IPAddress? address)
    {
        if (address is null)
        {
            return "unknown";
        }

        System.Net.IPAddress normalizedAddress = address.IsIPv4MappedToIPv6
            ? address.MapToIPv4()
            : address;

        return normalizedAddress.ToString().Trim().ToLowerInvariant();
    }

    private static string NormalizeValue(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback
            : value.Trim().ToLowerInvariant();
    }
}
