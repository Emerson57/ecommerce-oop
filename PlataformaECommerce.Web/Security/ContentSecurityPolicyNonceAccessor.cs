using System.Security.Cryptography;

namespace PlataformaECommerce.Web.Security;

/// <summary>
/// Resuelve y reutiliza un nonce CSP por solicitud HTTP para estilos o scripts inline controlados.
/// </summary>
internal static class ContentSecurityPolicyNonceAccessor
{
    private const string NonceItemKey = "ContentSecurityPolicyNonce";

    public static string GetOrCreateNonce(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (httpContext.Items.TryGetValue(NonceItemKey, out object? value) && value is string nonce && !string.IsNullOrWhiteSpace(nonce))
        {
            return nonce;
        }

        Span<byte> buffer = stackalloc byte[32];
        RandomNumberGenerator.Fill(buffer);
        string generatedNonce = Convert.ToBase64String(buffer);
        httpContext.Items[NonceItemKey] = generatedNonce;
        return generatedNonce;
    }
}
