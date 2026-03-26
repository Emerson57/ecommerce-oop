
using global::PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Admin.DTOs;

/// <summary>
/// Representa el objeto de transferencia de datos de un administrador
/// dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para transportar información de administradores desde
/// la capa Application hacia capas superiores como:
/// - Web API,
/// - paneles de administración,
/// - módulos de auditoría,
/// - consultas internas,
/// - procesos de control operativo.
///
/// Su propósito es desacoplar la representación expuesta del administrador
/// respecto de la entidad de dominio <c>Administrador</c>, evitando filtrar
/// directamente detalles internos del modelo.
///
/// Esta clase amplía la representación base de usuario con información
/// relevante del contexto organizacional y operativo del administrador.
/// </remarks>
public sealed class AdminDto
{
    #region Identificación básica

    /// <summary>
    /// Identificador único del administrador.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nombre completo o nombre visible del administrador.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Correo electrónico principal del administrador.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    #endregion

    #region Información funcional

    /// <summary>
    /// Rol funcional del usuario dentro del sistema.
    /// </summary>
    /// <remarks>
    /// Para este DTO, el valor esperado normalmente será <see cref="RolUsuario.Administrador"/>.
    /// </remarks>
    public RolUsuario Role { get; init; } = RolUsuario.Administrador;

    /// <summary>
    /// Indica si el administrador se encuentra activo dentro del sistema.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Indica si el correo electrónico del administrador ya fue confirmado.
    /// </summary>
    public bool IsEmailConfirmed { get; init; }

    #endregion

    #region Información organizacional

    /// <summary>
    /// Área o dependencia organizacional a la que pertenece el administrador.
    /// </summary>
    public string Area { get; init; } = string.Empty;

    #endregion

    #region Información temporal

    /// <summary>
    /// Fecha y hora UTC en que fue creado el administrador.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC de la última actualización relevante del administrador.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC del último acceso registrado del administrador.
    /// </summary>
    public DateTime? LastAccessAtUtc { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si el administrador se encuentra habilitado para operar dentro del sistema.
    /// </summary>
    public bool IsEnabled => IsActive && IsEmailConfirmed;

    /// <summary>
    /// Indica si el administrador tiene un área organizacional informada.
    /// </summary>
    public bool HasArea => !string.IsNullOrWhiteSpace(Area);

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del DTO de administrador.
    /// </summary>
    /// <returns>Cadena representativa del administrador.</returns>
    public override string ToString()
    {
        return $"AdminDto | Id: {Id} | Name: {Name} | Email: {Email} | Role: {Role} | Area: {Area} | Active: {IsActive} | EmailConfirmed: {IsEmailConfirmed}";
    }

    #endregion
}