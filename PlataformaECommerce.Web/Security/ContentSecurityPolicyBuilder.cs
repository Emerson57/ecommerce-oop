using System.Text;
using Microsoft.Extensions.Hosting;

namespace PlataformaECommerce.Web.Security;

/// <summary>
/// Construye el valor efectivo de Content Security Policy a partir de opciones tipadas y del nonce por solicitud.
/// </summary>
internal static class ContentSecurityPolicyBuilder
{
    public static string Build(ContentSecurityPolicyOptions options, IHostEnvironment hostEnvironment, string nonce)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(hostEnvironment);
        if (string.IsNullOrWhiteSpace(nonce))
        {
            throw new ArgumentException("El nonce CSP es obligatorio.", nameof(nonce));
        }

        StringBuilder builder = new();
        AppendDirective(builder, "default-src", NormalizeSources(options.DefaultSources));
        AppendDirective(builder, "base-uri", NormalizeSources(options.BaseUriSources));
        AppendDirective(builder, "object-src", NormalizeSources(options.ObjectSources));
        AppendDirective(builder, "frame-ancestors", NormalizeSources(options.FrameAncestorSources));
        AppendDirective(builder, "img-src", NormalizeSources(options.ImageSources));
        AppendDirective(builder, "style-src", NormalizeSources([.. options.StyleSources, $"'nonce-{nonce}'"]));
        AppendDirective(builder, "script-src", NormalizeSources(options.ScriptSources));
        AppendDirective(builder, "font-src", NormalizeSources(options.FontSources));
        AppendDirective(builder, "connect-src", NormalizeSources(options.ConnectSources));
        AppendDirective(builder, "form-action", NormalizeSources(options.FormActionSources));

        if (options.IncludeUpgradeInsecureRequests && !hostEnvironment.IsDevelopment())
        {
            AppendDirective(builder, "upgrade-insecure-requests", []);
        }

        return builder.ToString();
    }

    private static void AppendDirective(StringBuilder builder, string directiveName, IReadOnlyCollection<string> sources)
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (string.IsNullOrWhiteSpace(directiveName))
        {
            throw new ArgumentException("El nombre de la directiva es obligatorio.", nameof(directiveName));
        }

        if (builder.Length > 0)
        {
            builder.Append(' ');
        }

        builder.Append(directiveName.Trim());

        if (sources.Count == 0)
        {
            builder.Append(';');
            return;
        }

        builder.Append(' ');
        builder.Append(string.Join(' ', sources));
        builder.Append(';');
    }

    private static string[] NormalizeSources(IEnumerable<string> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        return sources
            .Where(source => !string.IsNullOrWhiteSpace(source))
            .Select(source => source.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}
