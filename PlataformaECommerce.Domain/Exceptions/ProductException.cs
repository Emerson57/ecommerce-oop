namespace PlataformaECommerce.Domain.Exceptions;

/// <summary>
/// Representa la excepción base para los errores específicos del módulo de productos.
/// </summary>
/// <remarks>
/// Debe utilizarse cuando una regla de negocio relacionada con productos no puede cumplirse,
/// permitiendo encapsular de forma más precisa errores asociados a precio, stock, disponibilidad,
/// datos inválidos o estados no permitidos dentro del ciclo de vida del producto.
/// </remarks>
public class ProductException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ProductException"/>.
    /// </summary>
    public ProductException()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ProductException"/> con un mensaje descriptivo.
    /// </summary>
    /// <param name="message">Mensaje que describe el error del producto.</param>
    public ProductException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ProductException"/> con un mensaje descriptivo
    /// y una excepción interna asociada.
    /// </summary>
    /// <param name="message">Mensaje que describe el error del producto.</param>
    /// <param name="innerException">Excepción interna asociada al error actual.</param>
    public ProductException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}