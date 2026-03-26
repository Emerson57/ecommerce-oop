namespace PlataformaECommerce.Web.OpenApi;

/// <summary>
/// Centraliza los nombres de grupo utilizados por Swagger para documentar la API web.
/// </summary>
/// <remarks>
/// Esta clase evita la dispersión de cadenas mágicas al separar la documentación
/// de endpoints públicos y administrativos dentro de Swagger y ApiExplorer.
/// </remarks>
public static class SwaggerGroups
{
    /// <summary>
    /// Grupo de documentación para endpoints públicos del catálogo.
    /// </summary>
    public const string Public = "public";

    /// <summary>
    /// Grupo de documentación para endpoints administrativos protegidos.
    /// </summary>
    public const string Admin = "admin";
}
