namespace PlataformaECommerce.Application.Features.Auth.DTOs;

/// <summary>
/// Representa la información resumida del usuario actualmente autenticado
/// dentro del sistema.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para proyectar hacia la capa superior la información
/// esencial del usuario autenticado, evitando exponer directamente entidades
/// del dominio o detalles internos de infraestructura.
///
/// Su propósito es servir como contrato de lectura seguro y consistente para:
/// - respuestas de autenticación,
/// - consulta del usuario actual,
/// - composición de claims,
/// - personalización de interfaz,
/// - y control contextual de acceso.
///
/// Esta clase no debe contener lógica de autorización ni reglas de negocio.
/// Dichas responsabilidades deben permanecer en servicios de seguridad,
/// políticas de autorización y componentes especializados.
/// </remarks>
public sealed class CurrentUserDto
{
    #region Identificación principal

    /// <summary>
    /// Identificador único del usuario autenticado.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nombre de usuario único dentro del sistema.
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// Correo electrónico principal del usuario.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    #endregion

    #region Información personal y de visualización

    /// <summary>
    /// Nombre completo o nombre para mostrar del usuario.
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Nombres del usuario, cuando el sistema maneje separación nominal.
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// Apellidos del usuario, cuando el sistema maneje separación nominal.
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// URL de la imagen de perfil o avatar del usuario, cuando esté disponible.
    /// </summary>
    public string? ProfileImageUrl { get; init; }

    #endregion

    #region Estado del usuario

    /// <summary>
    /// Indica si la cuenta del usuario se encuentra activa.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indica si el correo electrónico del usuario ha sido verificado.
    /// </summary>
    public bool IsEmailConfirmed { get; init; }

    /// <summary>
    /// Indica si el usuario tiene habilitada autenticación multifactor,
    /// cuando el sistema lo soporte.
    /// </summary>
    public bool IsTwoFactorEnabled { get; init; }

    #endregion

    #region Información de autorización

    /// <summary>
    /// Rol principal del usuario dentro del sistema.
    /// </summary>
    public string? Role { get; init; }

    /// <summary>
    /// Conjunto de roles asignados al usuario.
    /// </summary>
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Conjunto de permisos directos o efectivos asignados al usuario.
    /// </summary>
    public IReadOnlyCollection<string> Permissions { get; init; } = Array.Empty<string>();

    #endregion

    #region Metadatos de auditoría

    /// <summary>
    /// Fecha y hora UTC de creación del usuario, cuando dicho dato esté disponible.
    /// </summary>
    public DateTime? CreatedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC del último acceso exitoso del usuario, cuando esté disponible.
    /// </summary>
    public DateTime? LastLoginAtUtc { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si el usuario posee al menos un rol asignado.
    /// </summary>
    public bool HasRoles => Roles.Count > 0;

    /// <summary>
    /// Indica si el usuario posee al menos un permiso asignado.
    /// </summary>
    public bool HasPermissions => Permissions.Count > 0;

    /// <summary>
    /// Obtiene el nombre visible más apropiado para mostrar en interfaz.
    /// </summary>
    public string DisplayName =>
        !string.IsNullOrWhiteSpace(FullName)
            ? FullName
            : !string.IsNullOrWhiteSpace(UserName)
                ? UserName
                : Email;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del usuario autenticado.
    /// </summary>
    /// <returns>Cadena representativa del DTO.</returns>
    public override string ToString()
    {
        return $"CurrentUserDto | Id: {Id} | UserName: {UserName} | Email: {Email} | Role: {Role} | IsActive: {IsActive}";
    }

    #endregion
}