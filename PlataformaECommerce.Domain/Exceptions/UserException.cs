namespace PlataformaECommerce.Domain.Exceptions;

/// <summary>
/// Representa la excepción base para los errores específicos del módulo de usuarios.
/// </summary>
/// <remarks>
/// Debe utilizarse cuando una regla de negocio asociada al usuario no se cumple,
/// por ejemplo en validaciones de identidad, estado del usuario, datos inválidos
/// o restricciones funcionales dentro del sistema.
/// </remarks>
public class UserException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="UserException"/>.
    /// </summary>
    public UserException()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="UserException"/> con un mensaje descriptivo.
    /// </summary>
    /// <param name="message">Mensaje que describe el error del usuario.</param>
    public UserException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="UserException"/> con un mensaje descriptivo
    /// y una excepción interna asociada.
    /// </summary>
    /// <param name="message">Mensaje que describe el error del usuario.</param>
    /// <param name="innerException">Excepción interna asociada al error actual.</param>
    public UserException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}