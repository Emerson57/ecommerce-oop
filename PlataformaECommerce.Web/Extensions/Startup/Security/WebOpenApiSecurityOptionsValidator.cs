using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Valida la exposición de OpenAPI por ambiente y los controles de acceso requeridos.
/// </summary>
internal sealed class WebOpenApiSecurityOptionsValidator : IValidateOptions<WebOpenApiSecurityOptions>
{
    public ValidateOptionsResult Validate(string? name, WebOpenApiSecurityOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.RequireAuthorizationOutsideDevelopment && string.IsNullOrWhiteSpace(options.RequiredPolicy))
        {
            return ValidateOptionsResult.Fail($"La configuración '{WebOpenApiSecurityOptions.SectionName}:RequiredPolicy' es obligatoria cuando se exige autorización fuera de Development.");
        }

        return ValidateOptionsResult.Success;
    }
}
