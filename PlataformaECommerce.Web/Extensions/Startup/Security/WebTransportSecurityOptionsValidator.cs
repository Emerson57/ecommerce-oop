using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Valida la configuración de transporte seguro HTTP antes del arranque.
/// </summary>
internal sealed class WebTransportSecurityOptionsValidator : IValidateOptions<WebTransportSecurityOptions>
{
    public ValidateOptionsResult Validate(string? name, WebTransportSecurityOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        List<string> failures = [];

        if (options.Preload && !options.IncludeSubDomains)
        {
            failures.Add($"La configuración '{WebTransportSecurityOptions.SectionName}:Preload' requiere '{WebTransportSecurityOptions.SectionName}:IncludeSubDomains' habilitado.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
