using PlataformaECommerce.Domain.Entities.Cart;

namespace PlataformaECommerce.Application.Interfaces.Repositories.Cart;

/// <summary>
/// Define el contrato del repositorio responsable de la persistencia
/// y recuperación del agregado <see cref="CarritoCompra"/> dentro del sistema.
/// </summary>
/// <remarks>
/// Este repositorio forma parte de la capa Application y actúa como
/// abstracción de acceso a datos para los carritos de compra.
///
/// Su responsabilidad es proporcionar operaciones de consulta
/// y persistencia sin exponer detalles de infraestructura como:
/// - ORM utilizados,
/// - motores de base de datos,
/// - mecanismos de almacenamiento,
/// - estrategias de serialización.
///
/// En una arquitectura basada en DDD y Clean Architecture,
/// el repositorio es implementado en la capa Infrastructure
/// y consumido por:
/// - servicios de aplicación,
/// - consultas de aplicación,
/// - y componentes especializados de orquestación.
///
/// La interfaz se orienta al agregado <see cref="CarritoCompra"/>,
/// por lo que las operaciones deben tratar el carrito como una unidad coherente,
/// incluyendo sus ítems y su estado operativo.
/// </remarks>
public interface ICartRepository
{
    #region Consultas generales

    /// <summary>
    /// Obtiene todos los carritos registrados en el sistema.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una colección de carritos registrados.
    /// </returns>
    Task<IReadOnlyCollection<CarritoCompra>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un carrito por su identificador único.
    /// </summary>
    /// <param name="id">Identificador del carrito.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// El carrito encontrado o <see langword="null"/> si no existe.
    /// </returns>
    Task<CarritoCompra?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el carrito activo asociado a un cliente específico.
    /// </summary>
    /// <param name="clienteId">Identificador del cliente propietario del carrito.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// El carrito del cliente o <see langword="null"/> si no existe.
    /// </returns>
    Task<CarritoCompra?> GetByCustomerIdAsync(
        Guid clienteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los carritos asociados a un cliente específico.
    /// </summary>
    /// <param name="clienteId">Identificador del cliente propietario de los carritos.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una colección de carritos asociados al cliente.
    /// </returns>
    Task<IReadOnlyCollection<CarritoCompra>> GetAllByCustomerIdAsync(
        Guid clienteId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Validaciones de existencia

    /// <summary>
    /// Verifica si existe un carrito con el identificador indicado.
    /// </summary>
    /// <param name="id">Identificador del carrito.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// <see langword="true"/> si el carrito existe;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    Task<bool> ExistsByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un cliente tiene al menos un carrito registrado.
    /// </summary>
    /// <param name="clienteId">Identificador del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// <see langword="true"/> si el cliente tiene un carrito registrado;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    Task<bool> ExistsByCustomerIdAsync(
        Guid clienteId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Persistencia

    /// <summary>
    /// Agrega un nuevo carrito al repositorio.
    /// </summary>
    /// <param name="carrito">Agregado carrito a persistir.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona.
    /// </returns>
    Task AddAsync(
        CarritoCompra carrito,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un carrito existente en el repositorio.
    /// </summary>
    /// <param name="carrito">Agregado carrito a actualizar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona.
    /// </returns>
    Task UpdateAsync(
        CarritoCompra carrito,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un carrito del repositorio por su identificador.
    /// </summary>
    /// <param name="id">Identificador del carrito a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona.
    /// </returns>
    Task RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    #endregion
}