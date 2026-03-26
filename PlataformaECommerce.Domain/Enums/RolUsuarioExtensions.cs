namespace PlataformaECommerce.Domain.Enums;

/// <summary>
/// Proporciona operaciones auxiliares sobre <see cref="RolUsuario"/> para mantener una semántica consistente del modelo de autorización.
/// </summary>
public static class RolUsuarioExtensions
{
    /// <summary>
    /// Obtiene los roles considerados administrativos dentro del sistema.
    /// </summary>
    public static IReadOnlyCollection<RolUsuario> RolesAdministrativos { get; } =
    [
        RolUsuario.Administrador,
        RolUsuario.SuperUsuario
    ];

    /// <summary>
    /// Determina si el rol pertenece al ámbito administrativo del backoffice.
    /// </summary>
    /// <param name="rol">Rol a evaluar.</param>
    /// <returns><see langword="true"/> cuando el rol es administrativo.</returns>
    public static bool EsAdministrativo(this RolUsuario rol)
    {
        return rol is RolUsuario.Administrador or RolUsuario.SuperUsuario;
    }

    /// <summary>
    /// Obtiene los roles efectivos que deben propagarse a mecanismos externos de autorización.
    /// </summary>
    /// <param name="rol">Rol principal del usuario.</param>
    /// <returns>Colección ordenada de roles efectivos.</returns>
    public static IReadOnlyCollection<string> ObtenerRolesEfectivos(this RolUsuario rol)
    {
        return rol switch
        {
            RolUsuario.SuperUsuario =>
            [
                RolUsuario.SuperUsuario.ToString(),
                RolUsuario.Administrador.ToString()
            ],
            _ => [rol.ToString()]
        };
    }

    /// <summary>
    /// Determina si el valor suministrado corresponde a un rol administrativo soportado.
    /// </summary>
    /// <param name="roleValue">Valor persistido o recibido desde un claim.</param>
    /// <returns><see langword="true"/> cuando el valor representa un rol administrativo.</returns>
    public static bool EsValorDeRolAdministrativo(string? roleValue)
    {
        if (string.IsNullOrWhiteSpace(roleValue))
        {
            return false;
        }

        return Enum.TryParse(roleValue.Trim(), ignoreCase: true, out RolUsuario rol)
            && rol.EsAdministrativo();
    }
}
