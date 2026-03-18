using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Users.DTOs;
using PlataformaECommerce.Domain.Entities.Users;

namespace PlataformaECommerce.Application.Mappings;

/// <summary>
/// Proporciona métodos de mapeo entre entidades del dominio relacionadas con usuarios
/// y los diferentes DTOs utilizados por la capa de aplicación.
/// </summary>
/// <remarks>
/// Esta clase centraliza las conversiones entre el modelo de dominio y los objetos
/// de transferencia de datos (DTOs), evitando duplicación de lógica de mapeo en:
///
/// - servicios de aplicación
/// - handlers de comandos
/// - handlers de consultas
/// - controladores API
///
/// Mantener estos métodos en una ubicación centralizada mejora la mantenibilidad
/// del sistema y facilita la evolución de los modelos sin afectar otras capas.
///
/// Esta clase actúa como un "mapper manual", evitando dependencias externas
/// como AutoMapper cuando se desea mantener control explícito del proceso
/// de transformación de datos.
/// </remarks>
public static class UserMappings
{
    #region Mapeo a UserDto

    /// <summary>
    /// Convierte una entidad de dominio <see cref="Usuario"/> en un <see cref="UserDto"/>.
    /// </summary>
    /// <param name="user">Entidad del dominio que representa al usuario.</param>
    /// <returns>
    /// Un DTO con la información básica del usuario.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando la entidad suministrada es nula.
    /// </exception>
    public static UserDto ToUserDto(Usuario user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new UserDto
        {
            Id = user.Id,
            Name = user.Nombre,
            Email = user.CorreoElectronico.Value,
            Role = user.Rol,
            IsActive = user.Activo,
            IsEmailConfirmed = user.CorreoConfirmado,
            CreatedAtUtc = user.FechaCreacionUtc,
            UpdatedAtUtc = user.FechaActualizacionUtc,
            LastAccessAtUtc = user.FechaUltimoAccesoUtc
        };
    }

    #endregion

    #region Mapeo a CustomerDto

    /// <summary>
    /// Convierte una entidad de dominio <see cref="Cliente"/> en un <see cref="CustomerDto"/>.
    /// </summary>
    /// <param name="customer">Entidad del dominio que representa al cliente.</param>
    /// <returns>
    /// Un DTO con la información completa del cliente.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando la entidad suministrada es nula.
    /// </exception>
    public static CustomerDto ToCustomerDto(Cliente customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Nombre,
            Email = customer.CorreoElectronico.Value,
            Role = customer.Rol,
            IsActive = customer.Activo,
            IsEmailConfirmed = customer.CorreoConfirmado,
            TotalPurchases = customer.TotalCompras,
            PurchaseHistory = customer.HistorialCompras.ToArray(),
            Preferences = customer.Preferencias
                .OrderBy(value => value)
                .ToArray(),
            CreatedAtUtc = customer.FechaCreacionUtc,
            UpdatedAtUtc = customer.FechaActualizacionUtc,
            LastAccessAtUtc = customer.FechaUltimoAccesoUtc
        };
    }

    #endregion

    #region Mapeo a AdminDto

    /// <summary>
    /// Convierte una entidad de dominio <see cref="Administrador"/> en un <see cref="AdminDto"/>.
    /// </summary>
    /// <param name="admin">Entidad del dominio que representa al administrador.</param>
    /// <returns>
    /// Un DTO con la información del administrador.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando la entidad suministrada es nula.
    /// </exception>
    public static AdminDto ToAdminDto(Administrador admin)
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
    /// Convierte una colección de entidades <see cref="Usuario"/> en una colección de <see cref="UserDto"/>.
    /// </summary>
    /// <param name="users">Colección de usuarios.</param>
    /// <returns>Lista de DTOs de usuarios.</returns>
    public static IReadOnlyCollection<UserDto> ToUserDtos(IEnumerable<Usuario> users)
    {
        ArgumentNullException.ThrowIfNull(users);

        return users
            .Select(ToUserDto)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Convierte una colección de entidades <see cref="Cliente"/> en una colección de <see cref="CustomerDto"/>.
    /// </summary>
    /// <param name="customers">Colección de clientes.</param>
    /// <returns>Lista de DTOs de clientes.</returns>
    public static IReadOnlyCollection<CustomerDto> ToCustomerDtos(IEnumerable<Cliente> customers)
    {
        ArgumentNullException.ThrowIfNull(customers);

        return customers
            .Select(ToCustomerDto)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Convierte una colección de entidades <see cref="Administrador"/> en una colección de <see cref="AdminDto"/>.
    /// </summary>
    /// <param name="admins">Colección de administradores.</param>
    /// <returns>Lista de DTOs de administradores.</returns>
    public static IReadOnlyCollection<AdminDto> ToAdminDtos(IEnumerable<Administrador> admins)
    {
        ArgumentNullException.ThrowIfNull(admins);

        return admins
            .Select(ToAdminDto)
            .ToList()
            .AsReadOnly();
    }

    #endregion
}