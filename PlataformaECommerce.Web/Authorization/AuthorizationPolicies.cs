namespace PlataformaECommerce.Web.Authorization;

/// <summary>
/// Centraliza los nombres de políticas de autorización utilizadas por la aplicación web.
/// </summary>
/// <remarks>
/// Esta clase evita la dispersión de cadenas mágicas relacionadas con seguridad,
/// facilitando la reutilización consistente de políticas entre Razor Pages,
/// controladores y futuros componentes administrativos.
/// </remarks>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Nombre del esquema de autenticación por cookies utilizado por el backoffice administrativo.
    /// </summary>
    public const string AdminCookieScheme = "AdminCookie";

    /// <summary>
    /// Nombre de la política que restringe el acceso a usuarios con rol administrativo.
    /// </summary>
    public const string AdminOnly = "AdminOnly";
}
