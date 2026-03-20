using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Domain.Entities.Users;

namespace PlataformaECommerce.Application.Features.Admin.Mappings;

/// <summary>
/// Proporciona métodos de mapeo entre entidades del dominio administrativas
/// y los DTOs expuestos por el feature <c>Admin</c>.
/// </summary>
/// <remarks>
/// Esta clase centraliza las conversiones asociadas al módulo administrativo,
/// manteniendo dentro del feature la proyección de <see cref="Administrador"/>
/// hacia contratos de salida consumidos por servicios de aplicación, Razor Pages
/// administrativas y otros componentes del backoffice.
/// </remarks>
public static class AdminMappings
{
    #region Mapeo a AdminDto

    /// <summary>
    /// Convierte una entidad de dominio <see cref="Administrador"/> en un <see cref="AdminDto"/>.
    /// </summary>
    /// <param name="admin">Entidad del dominio que representa al administrador.</param>
    /// <returns>Un DTO con la información del administrador.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando la entidad suministrada es nula.
    /// </exception>
    public static AdminDto ToAdminDto(this Administrador admin)
    {
        ArgumentNullException.ThrowIfNull(admin);

        return new AdminDto
        {
            Id = admin.Id,
            Name = admin.Nombre,
            Email = admin.CorreoElectronico.Value,
            Role = admin.Rol,
            IsActive = admin.Activo,
            IsEmailConfirmed = admin.CorreoConfirmado,
            Area = admin.Area,
            CreatedAtUtc = admin.FechaCreacionUtc,
            UpdatedAtUtc = admin.FechaActualizacionUtc,
            LastAccessAtUtc = admin.FechaUltimoAccesoUtc
        };
    }

    #endregion

    #region Mapeo de colecciones

    /// <summary>
    /// Convierte una colección de entidades <see cref="Administrador"/> en una colección de <see cref="AdminDto"/>.
    /// </summary>
    /// <param name="admins">Colección de administradores.</param>
    /// <returns>Lista de DTOs de administradores.</returns>
    public static IReadOnlyCollection<AdminDto> ToAdminDtos(this IEnumerable<Administrador> admins)
    {
        ArgumentNullException.ThrowIfNull(admins);

        return admins
            .Select(ToAdminDto)
            .ToList()
            .AsReadOnly();
    }

    #endregion
}
