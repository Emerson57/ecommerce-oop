namespace PlataformaECommerce.Domain.Exceptions;

/// <summary>
/// Representa la excepción base para errores producidos durante la creación controlada de entidades.
/// </summary>
/// <remarks>
/// Debe utilizarse cuando una fábrica del dominio o de infraestructura aplicada al dominio
/// recibe parámetros inválidos, insuficientes o incompatibles con el modelo actual.
/// </remarks>
public sealed class FactoryException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="FactoryException"/>.
    /// </summary>
    public FactoryException()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="FactoryException"/> con un mensaje descriptivo.
    /// </summary>
    /// <param name="message">Mensaje que describe el error de creación.</param>
    public FactoryException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="FactoryException"/> con un mensaje descriptivo
    /// y una excepción interna asociada.
    /// </summary>
    /// <param name="message">Mensaje que describe el error de creación.</param>
    /// <param name="innerException">Excepción interna asociada al error actual.</param>
    public FactoryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}