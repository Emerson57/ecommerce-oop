using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Users.DTOs;

/// <summary>
/// Representa el objeto de transferencia de datos base de un usuario
/// dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para transportar información de usuarios desde
/// la capa Application hacia capas superiores como:
/// - Web API,
/// - paneles administrativos,
/// - procesos de autenticación,
/// - consultas internas,
/// - flujos de auditoría.
///
/// Su propósito es desacoplar la representación expuesta del usuario
/// respecto de la entidad de dominio <c>Usuario</c>, evitando filtrar
/// directamente detalles internos del modelo.
///
/// Esta clase sirve como DTO base para representar usuarios del sistema
/// de forma general, independientemente de si corresponden a clientes
/// o administradores.
/// </remarks>
public sealed class UserDto
{
    #region Identificación básica

    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nombre completo o nombre visible del usuario.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Correo electrónico principal del usuario.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    #endregion

    #region Información funcional

    /// <summary>
    /// Rol funcional del usuario dentro del sistema.
    /// </summary>
    public RolUsuario Role { get; init; }

    /// <summary>
    /// Indica si el usuario se encuentra activo dentro del sistema.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indica si el correo electrónico del usuario ya fue confirmado.
    /// </summary>
    public bool IsEmailConfirmed { get; init; }

    #endregion

    #region Información temporal

    /// <summary>
    /// Fecha y hora UTC en que fue creado el usuario.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC de la última actualización relevante del usuario.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC del último acceso registrado del usuario.
    /// </summary>
    public DateTime? LastAccessAtUtc { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si el usuario se encuentra habilitado para operar dentro del sistema.
    /// </summary>
    public bool IsEnabled => IsActive && IsEmailConfirmed;

    /// <summary>
    /// Indica si el usuario corresponde a un cliente.
    /// </summary>
    public bool IsCustomer => Role == RolUsuario.Cliente;

    /// <summary>
    /// Indica si el usuario corresponde a un administrador.
    /// </summary>
    public bool IsAdministrator => Role == RolUsuario.Administrador;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del DTO de usuario.
    /// </summary>
    /// <returns>Cadena representativa del usuario.</returns>
    public override string ToString()
    {
        return $"UserDto | Id: {Id} | Name: {Name} | Email: {Email} | Role: {Role} | Active: {IsActive} | EmailConfirmed: {IsEmailConfirmed}";
    }

    #endregion
}