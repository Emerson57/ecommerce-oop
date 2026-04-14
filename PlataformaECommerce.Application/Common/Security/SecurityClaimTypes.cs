namespace PlataformaECommerce.Application.Common.Security;

/// <summary>
/// Centraliza los tipos de claims transversales utilizados por los módulos de autenticación y backoffice.
/// </summary>
/// <remarks>
/// Estos nombres forman parte del contrato de identidad compartido entre Application,
/// Infrastructure y Web para evitar cadenas mágicas y mantener coherencia en la
/// propagación del contexto autenticado.
/// </remarks>
public static class SecurityClaimTypes
{
    /// <summary>
    /// Claim que representa el área organizacional de una cuenta administrativa.
    /// </summary>
    public const string AdminArea = "admin_area";

    /// <summary>
    /// Claim que representa el rol primario del usuario autenticado.
    /// </summary>
    public const string PrimaryRole = "primary_role";

    /// <summary>
    /// Claim que indica si la cuenta autenticada posee privilegios de super usuario.
    /// </summary>
    public const string IsSuperUser = "is_super_user";

    /// <summary>
    /// Claim que representa el tenant efectivo asociado al contexto autenticado.
    /// </summary>
    public const string TenantId = "tenant_id";

    /// <summary>
    /// Claim reservado para permisos finos emitidos por la plataforma.
    /// </summary>
    public const string Permission = "permission";
}
