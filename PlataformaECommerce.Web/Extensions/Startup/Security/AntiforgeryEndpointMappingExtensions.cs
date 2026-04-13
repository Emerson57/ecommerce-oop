using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Expone endpoints auxiliares relacionados con antiforgery para clientes autenticados del backoffice.
/// </summary>
public static class AntiforgeryEndpointMappingExtensions
{
    /// <summary>
    /// Mapea un endpoint same-origin para obtener un token antiforgery reutilizable en solicitudes AJAX o JSON protegidas por cookies.
    /// </summary>
    public static IEndpointConventionBuilder MapAntiforgeryTokenEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapGet("/security/antiforgery/token", static (HttpContext context, IAntiforgery antiforgery, IOptions<WebAntiforgeryOptions> optionsAccessor) =>
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(antiforgery);
            ArgumentNullException.ThrowIfNull(optionsAccessor);

            AntiforgeryTokenSet tokenSet = antiforgery.GetAndStoreTokens(context);
            WebAntiforgeryOptions options = optionsAccessor.Value;

            return Results.Ok(new AntiforgeryTokenResponse(
                tokenSet.RequestToken ?? string.Empty,
                options.HeaderName,
                options.FormFieldName));
        })
        .RequireAuthorization();
    }

    private sealed record AntiforgeryTokenResponse(string RequestToken, string HeaderName, string FormFieldName);
}
