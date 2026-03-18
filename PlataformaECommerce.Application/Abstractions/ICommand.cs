namespace PlataformaECommerce.Application.Abstractions;

/// <summary>
/// Representa la abstracción base de un comando dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Un comando modela una intención explícita de cambiar el estado del sistema.
/// Su responsabilidad no es contener lógica de negocio, sino transportar
/// la información necesaria para que un <c>CommandHandler</c> ejecute un caso de uso.
///
/// Esta interfaz forma parte del patrón CQRS dentro de la capa Application,
/// permitiendo distinguir claramente entre:
/// - operaciones de escritura (<see cref="ICommand"/>),
/// - y operaciones de lectura (<c>IQuery</c>).
///
/// Ejemplos típicos de comandos:
/// - crear un producto,
/// - actualizar stock,
/// - registrar un usuario,
/// - agregar un producto al carrito,
/// - confirmar un pedido.
/// </remarks>
public interface ICommand
{
}

/// <summary>
/// Representa la abstracción base de un comando que retorna un resultado tipado.
/// </summary>
/// <typeparam name="TResult">Tipo del resultado esperado al ejecutar el comando.</typeparam>
/// <remarks>
/// Esta variante genérica permite modelar comandos que, además de modificar el estado
/// del sistema, retornan información relevante para la capa superior.
///
/// Ejemplos típicos:
/// - crear un recurso y devolver su identificador,
/// - registrar un usuario y devolver un DTO resumido,
/// - autenticar un usuario y devolver un token,
/// - crear un pedido y devolver su resultado de creación.
///
/// El tipo <typeparamref name="TResult"/> suele integrarse con patrones como:
/// - <c>Result&lt;T&gt;</c>,
/// - DTOs de salida,
/// - identificadores,
/// - respuestas específicas del caso de uso.
/// </remarks>
public interface ICommand<out TResult> : ICommand
{
}