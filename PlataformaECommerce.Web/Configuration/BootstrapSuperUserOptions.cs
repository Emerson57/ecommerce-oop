namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Representa la configuración utilizada para bootstrappear el primer super usuario del sistema.
/// </summary>
/// <remarks>
/// Esta opción se consume únicamente desde la composición raíz para habilitar una creación
/// controlada, auditable y de una sola vez del primer usuario con privilegios máximos.
/// </remarks>
public sealed class BootstrapSuperUserOptions
{
    /// <summary>
    /// Nombre de la sección de configuración asociada al bootstrap del super usuario.
    /// </summary>
    public const string SectionName = "Bootstrap:SuperUser";

    /// <summary>
    /// Indica si el bootstrap inicial se encuentra habilitado.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Indica si el bootstrap del super usuario puede ejecutarse explícitamente en producción.
    /// </summary>
    public bool AllowInProduction { get; set; }

    /// <summary>
    /// Nombre completo del super usuario inicial.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tenant objetivo sobre el cual debe ejecutarse el bootstrap inicial.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico del super usuario inicial.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña temporal del super usuario inicial.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Área organizacional asociada al super usuario inicial.
    /// </summary>
    public string Area { get; set; } = "Plataforma";
}
