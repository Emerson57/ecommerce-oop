using PlataformaECommerce.Application.Features.Users.DTOs;
using PlataformaECommerce.Domain.Entities.Users;

namespace PlataformaECommerce.Application.Features.Users.Mappings;

/// <summary>
/// Proporciona métodos de mapeo entre entidades del dominio relacionadas con usuarios
/// y los diferentes DTOs utilizados por la capa de aplicación.
/// </summary>
/// <remarks>
/// Esta clase centraliza las conversiones entre el modelo de dominio y los objetos
/// de transferencia de datos (DTOs), evitando duplicación de lógica de mapeo en:
///
/// - servicios de aplicación,
/// - páginas y controladores consumidores,
/// - y otros componentes de orquestación.
///
/// Mantener estos métodos en una ubicación centralizada mejora la mantenibilidad
/// del sistema y facilita la evolución de los modelos sin afectar otras capas.
///
/// Esta clase actúa como un mapper manual, evitando dependencias externas
/// cuando se desea mantener control explícito del proceso de transformación de datos.
/// </remarks>
public static class UserMappings
{
    #region Mapeo a UserDto

    /// <summary>
    /// Convierte una entidad de dominio <see cref="Usuario"/> en un <see cref="UserDto"/>.
    /// </summary>
    /// <param name="user">Entidad del dominio que representa al usuario.</param>
    /// <returns>Un DTO con la información básica del usuario.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando la entidad suministrada es nula.
    /// </exception>
    public static UserDto ToUserDto(this Usuario user)
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
    /// <returns>Un DTO con la información completa del cliente.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando la entidad suministrada es nula.
    /// </exception>
    public static CustomerDto ToCustomerDto(this Cliente customer)
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

    #region Mapeo de colecciones

    /// <summary>
    /// Convierte una colección de entidades <see cref="Usuario"/> en una colección de <see cref="UserDto"/>.
    /// </summary>
    /// <param name="users">Colección de usuarios.</param>
    /// <returns>Lista de DTOs de usuarios.</returns>
    public static IReadOnlyCollection<UserDto> ToUserDtos(this IEnumerable<Usuario> users)
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
    public static IReadOnlyCollection<CustomerDto> ToCustomerDtos(this IEnumerable<Cliente> customers)
    {
        ArgumentNullException.ThrowIfNull(customers);

        return customers
            .Select(ToCustomerDto)
            .ToList()
            .AsReadOnly();
    }

    #endregion
}
