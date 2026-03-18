namespace PlataformaECommerce.Application.Abstractions;

/// <summary>
/// Representa la abstracción base de una consulta dentro de la capa de aplicación.
/// </summary>
/// <typeparam name="TResult">Tipo del resultado esperado al ejecutar la consulta.</typeparam>
/// <remarks>
/// Una query modela una intención explícita de lectura del sistema.
/// Su responsabilidad no es modificar el estado del dominio, sino transportar
/// la información necesaria para que un <c>QueryHandler</c> recupere y construya
/// la respuesta solicitada.
///
/// Esta interfaz forma parte del patrón CQRS dentro de la capa Application,
/// permitiendo distinguir claramente entre:
/// - operaciones de lectura (<see cref="IQuery{TResult}"/>),
/// - y operaciones de escritura (<c>ICommand</c>).
///
/// Ejemplos típicos de queries:
/// - obtener un producto por Id,
/// - listar productos del catálogo,
/// - consultar un carrito por cliente,
/// - obtener pedidos de un usuario,
/// - consultar el usuario autenticado.
///
/// El tipo <typeparamref name="TResult"/> suele corresponder a:
/// - un DTO,
/// - una colección de DTOs,
/// - un <c>Result&lt;T&gt;</c>,
/// - o una respuesta especializada de lectura.
///
/// Esta interfaz se mantiene intencionalmente minimalista para conservar
/// bajo acoplamiento y máxima flexibilidad en los casos de uso de lectura.
/// </remarks>
public interface IQuery<out TResult>
{
}