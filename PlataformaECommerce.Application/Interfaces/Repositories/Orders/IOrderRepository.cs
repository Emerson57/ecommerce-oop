using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Interfaces.Repositories.Orders;

/// <summary>
/// Define el contrato del repositorio responsable de la persistencia
/// y recuperación del agregado <see cref="Pedido"/> dentro del sistema.
/// </summary>
/// <remarks>
/// Este repositorio forma parte de la capa Application y actúa como
/// abstracción de acceso a datos para los pedidos del sistema.
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
/// - consultas de aplicación,
/// - y componentes especializados de orquestación.
///
/// La interfaz se orienta al agregado <see cref="Pedido"/>,
/// por lo que las operaciones deben tratar al pedido como una unidad coherente,
/// incluyendo sus detalles, estado y trazabilidad temporal.
/// </remarks>
public interface IOrderRepository
{
    #region Consultas generales

    /// <summary>
    /// Obtiene todos los pedidos registrados en el sistema.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una colección de pedidos registrados.
    /// </returns>
    Task<IReadOnlyCollection<Pedido>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un pedido por su identificador único.
    /// </summary>
    /// <param name="id">Identificador del pedido.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// El pedido encontrado o <see langword="null"/> si no existe.
    /// </returns>
    Task<Pedido?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los pedidos asociados a un cliente específico.
    /// </summary>
    /// <param name="clienteId">Identificador del cliente propietario de los pedidos.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una colección de pedidos asociados al cliente.
    /// </returns>
    Task<IReadOnlyCollection<Pedido>> GetByCustomerIdAsync(
        Guid clienteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los pedidos que se encuentran en un estado específico.
    /// </summary>
    /// <param name="estado">Estado de los pedidos a consultar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una colección de pedidos en el estado indicado.
    /// </returns>
    Task<IReadOnlyCollection<Pedido>> GetByStatusAsync(
        EstadoPedido estado,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los pedidos de un cliente filtrados por estado.
    /// </summary>
    /// <param name="clienteId">Identificador del cliente.</param>
    /// <param name="estado">Estado de los pedidos a consultar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una colección de pedidos del cliente en el estado indicado.
    /// </returns>
    Task<IReadOnlyCollection<Pedido>> GetByCustomerIdAndStatusAsync(
        Guid clienteId,
        EstadoPedido estado,
        CancellationToken cancellationToken = default);

    #endregion

    #region Validaciones de existencia

    /// <summary>
    /// Verifica si existe un pedido con el identificador indicado.
    /// </summary>
    /// <param name="id">Identificador del pedido.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// <see langword="true"/> si el pedido existe;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    Task<bool> ExistsByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un cliente tiene al menos un pedido registrado.
    /// </summary>
    /// <param name="clienteId">Identificador del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// <see langword="true"/> si el cliente tiene pedidos registrados;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    Task<bool> ExistsByCustomerIdAsync(
        Guid clienteId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Persistencia

    /// <summary>
    /// Agrega un nuevo pedido al repositorio.
    /// </summary>
    /// <param name="pedido">Agregado pedido a persistir.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona.
    /// </returns>
    Task AddAsync(
        Pedido pedido,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un pedido existente en el repositorio.
    /// </summary>
    /// <param name="pedido">Agregado pedido a actualizar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona.
    /// </returns>
    Task UpdateAsync(
        Pedido pedido,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un pedido del repositorio por su identificador.
    /// </summary>
    /// <param name="id">Identificador del pedido a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona.
    /// </returns>
    Task RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    #endregion
}