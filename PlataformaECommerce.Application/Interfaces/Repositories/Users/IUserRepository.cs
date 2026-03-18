using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Application.Interfaces.Repositories.Users;

/// <summary>
/// Define el contrato del repositorio responsable de la persistencia
/// y recuperación del agregado <see cref="Usuario"/> dentro del sistema.
/// </summary>
/// <remarks>
/// Este repositorio forma parte de la capa Application y actúa como
/// abstracción de acceso a datos para los usuarios del sistema.
///
/// Su responsabilidad es proporcionar operaciones de consulta
/// y persistencia sin exponer detalles de infraestructura como:
/// - ORM utilizados,
/// - motores de base de datos,
/// - mecanismos de almacenamiento,
/// - estrategias de serialización o mapeo.
///
/// En una arquitectura basada en DDD y Clean Architecture,
/// el repositorio es implementado en la capa Infrastructure
/// y consumido por:
/// - servicios de aplicación,
/// - command handlers,
/// - query handlers,
/// - servicios de autenticación.
///
/// La interfaz se orienta al agregado <see cref="Usuario"/>,
/// permitiendo trabajar con sus especializaciones <see cref="Cliente"/>
/// y <see cref="Administrador"/> sin acoplar la capa Application
/// a detalles concretos de persistencia.
/// </remarks>
public interface IUserRepository
{
    #region Consultas generales

    /// <summary>
    /// Obtiene todos los usuarios registrados en el sistema.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una colección de usuarios registrados.
    /// </returns>
    Task<IReadOnlyCollection<Usuario>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un usuario por su identificador único.
    /// </summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// El usuario encontrado o <see langword="null"/> si no existe.
    /// </returns>
    Task<Usuario?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un usuario a partir de su correo electrónico.
    /// </summary>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// El usuario encontrado o <see langword="null"/> si no existe.
    /// </returns>
    Task<Usuario?> GetByEmailAsync(
        Email email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los usuarios que pertenecen a un rol específico.
    /// </summary>
    /// <param name="rol">Rol a consultar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una colección de usuarios pertenecientes al rol indicado.
    /// </returns>
    Task<IReadOnlyCollection<Usuario>> GetByRoleAsync(
        RolUsuario rol,
        CancellationToken cancellationToken = default);

    #endregion

    #region Consultas especializadas

    /// <summary>
    /// Obtiene todos los clientes registrados en el sistema.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una colección de clientes registrados.
    /// </returns>
    Task<IReadOnlyCollection<Cliente>> GetCustomersAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los administradores registrados en el sistema.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una colección de administradores registrados.
    /// </returns>
    Task<IReadOnlyCollection<Administrador>> GetAdministratorsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un cliente por su identificador único.
    /// </summary>
    /// <param name="id">Identificador del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// El cliente encontrado o <see langword="null"/> si no existe o no corresponde a un cliente.
    /// </returns>
    Task<Cliente?> GetCustomerByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un administrador por su identificador único.
    /// </summary>
    /// <param name="id">Identificador del administrador.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// El administrador encontrado o <see langword="null"/> si no existe o no corresponde a un administrador.
    /// </returns>
    Task<Administrador?> GetAdministratorByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    #endregion

    #region Validaciones de existencia

    /// <summary>
    /// Verifica si existe un usuario con el identificador indicado.
    /// </summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// <see langword="true"/> si el usuario existe;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    Task<bool> ExistsByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe un usuario con el correo electrónico indicado.
    /// </summary>
    /// <param name="email">Correo electrónico a validar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// <see langword="true"/> si el usuario existe;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    Task<bool> ExistsByEmailAsync(
        Email email,
        CancellationToken cancellationToken = default);

    #endregion

    #region Persistencia

    /// <summary>
    /// Agrega un nuevo usuario al repositorio.
    /// </summary>
    /// <param name="usuario">Agregado usuario a persistir.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona.
    /// </returns>
    Task AddAsync(
        Usuario usuario,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un usuario existente en el repositorio.
    /// </summary>
    /// <param name="usuario">Agregado usuario a actualizar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona.
    /// </returns>
    Task UpdateAsync(
        Usuario usuario,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un usuario del repositorio por su identificador.
    /// </summary>
    /// <param name="id">Identificador del usuario a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona.
    /// </returns>
    Task RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    #endregion
}