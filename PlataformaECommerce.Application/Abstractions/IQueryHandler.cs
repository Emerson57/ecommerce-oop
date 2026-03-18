namespace PlataformaECommerce.Application.Abstractions;

/// <summary>
/// Define el contrato base para un manejador de consultas dentro de la capa de aplicación.
/// </summary>
/// <typeparam name="TQuery">Tipo de consulta que será procesada.</typeparam>
/// <typeparam name="TResult">Tipo del resultado esperado.</typeparam>
/// <remarks>
/// Un <c>QueryHandler</c> es responsable de ejecutar un caso de uso de lectura,
/// es decir, una operación que recupera información sin modificar el estado del sistema.
///
/// Su responsabilidad incluye coordinar:
/// - recuperación de datos,
/// - proyección a DTOs,
/// - composición de respuestas,
/// - y retorno controlado hacia capas superiores.
///
/// Esta interfaz forma parte del patrón CQRS y debe utilizarse junto con:
/// - <see cref="IQuery{TResult}"/>
/// - <c>ICommand</c>
/// - <c>ICommandHandler</c>
///
/// En escenarios profesionales, <typeparamref name="TResult"/> suele corresponder a:
/// - un DTO,
/// - una colección de DTOs,
/// - un <c>Result&lt;T&gt;</c>,
/// - o una respuesta específica de lectura.
///
/// El uso de <see cref="CancellationToken"/> permite que la consulta
/// pueda cancelarse de forma segura cuando la infraestructura lo requiera,
/// por ejemplo en peticiones HTTP canceladas o procesos interrumpidos.
/// </remarks>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Ejecuta la consulta especificada y retorna un resultado tipado.
    /// </summary>
    /// <param name="query">Consulta a procesar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa la ejecución asíncrona de la consulta
    /// y contiene el resultado del procesamiento.
    /// </returns>
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken = default);
}