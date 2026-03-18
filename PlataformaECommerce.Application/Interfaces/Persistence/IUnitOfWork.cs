namespace PlataformaECommerce.Application.Interfaces.Persistence;

/// <summary>
/// Define el contrato de una unidad de trabajo dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// El patrón Unit of Work tiene como objetivo coordinar la persistencia
/// de múltiples cambios realizados sobre agregados y repositorios
/// dentro de una misma frontera transaccional.
///
/// Su responsabilidad principal es:
/// - agrupar cambios,
/// - asegurar consistencia transaccional,
/// - confirmar o revertir operaciones,
/// - y desacoplar la capa Application de los mecanismos concretos
///   de persistencia e infraestructura.
///
/// En una arquitectura basada en Clean Architecture y DDD,
/// la implementación concreta de esta interfaz reside en la capa Infrastructure,
/// normalmente sobre tecnologías como:
/// - Entity Framework Core,
/// - conexiones ADO.NET,
/// - transacciones SQL,
/// - o estrategias híbridas de persistencia.
///
/// Esta interfaz es especialmente importante en escenarios como:
/// - creación de pedidos,
/// - actualización de stock,
/// - registro de usuarios,
/// - operaciones encadenadas entre múltiples repositorios,
/// - publicación posterior de eventos de dominio.
/// </remarks>
public interface IUnitOfWork : IAsyncDisposable
{
    #region Persistencia

    /// <summary>
    /// Persiste de manera asíncrona todos los cambios pendientes
    /// dentro de la unidad de trabajo actual.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// El número de registros, entidades o cambios afectados durante la persistencia.
    /// </returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Control transaccional

    /// <summary>
    /// Inicia explícitamente una transacción asociada a la unidad de trabajo actual.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa el inicio asíncrono de la transacción.
    /// </returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma explícitamente la transacción activa de la unidad de trabajo actual.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa la confirmación asíncrona de la transacción.
    /// </returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Revierte explícitamente la transacción activa de la unidad de trabajo actual.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa la reversión asíncrona de la transacción.
    /// </returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    #endregion
}