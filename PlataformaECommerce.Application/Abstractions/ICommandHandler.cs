namespace PlataformaECommerce.Application.Abstractions;

/// <summary>
/// Define el contrato base para un manejador de comandos dentro de la capa de aplicación.
/// </summary>
/// <typeparam name="TCommand">Tipo de comando que será procesado.</typeparam>
/// <remarks>
/// Un <c>CommandHandler</c> es responsable de ejecutar un caso de uso de escritura,
/// es decir, una operación que modifica el estado del sistema.
///
/// Su responsabilidad incluye coordinar:
/// - validaciones de entrada,
/// - invocación del dominio,
/// - acceso a repositorios,
/// - persistencia transaccional,
/// - y retorno controlado hacia capas superiores.
///
/// Esta interfaz forma parte del patrón CQRS y debe utilizarse junto con:
/// - <see cref="ICommand"/>
/// - <c>IQuery</c>
/// - <c>IQueryHandler</c>
///
/// El uso de <see cref="CancellationToken"/> permite que la operación
/// pueda cancelarse de forma segura cuando la infraestructura lo requiera,
/// por ejemplo en peticiones HTTP canceladas o procesos interrumpidos.
/// </remarks>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Ejecuta el comando especificado.
    /// </summary>
    /// <param name="command">Comando a procesar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa la ejecución asíncrona del comando.
    /// </returns>
    Task Handle(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Define el contrato base para un manejador de comandos que retorna un resultado tipado.
/// </summary>
/// <typeparam name="TCommand">Tipo de comando que será procesado.</typeparam>
/// <typeparam name="TResult">Tipo del resultado esperado.</typeparam>
/// <remarks>
/// Esta variante se utiliza cuando la ejecución del comando,
/// además de modificar el estado del sistema,
/// necesita devolver un valor a la capa superior.
///
/// Ejemplos comunes:
/// - crear un recurso y devolver su identificador,
/// - registrar un usuario y devolver su DTO,
/// - autenticar y devolver un token,
/// - crear un pedido y devolver su resumen.
///
/// En proyectos profesionales, <typeparamref name="TResult"/> suele ser:
/// - un <c>Result</c>,
/// - un <c>Result&lt;T&gt;</c>,
/// - un DTO de salida,
/// - o una respuesta específica del caso de uso.
/// </remarks>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    /// <summary>
    /// Ejecuta el comando especificado y retorna un resultado tipado.
    /// </summary>
    /// <param name="command">Comando a procesar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa la ejecución asíncrona del comando
    /// y contiene el resultado del procesamiento.
    /// </returns>
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken = default);
}