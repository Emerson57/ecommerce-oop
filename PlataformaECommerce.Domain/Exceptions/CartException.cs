namespace PlataformaECommerce.Domain.Exceptions;

/// <summary>
/// Representa la excepción base para los errores específicos del módulo de carrito de compras.
/// </summary>
/// <remarks>
/// Debe utilizarse para encapsular errores de negocio asociados a operaciones como agregar,
/// quitar, modificar cantidades o procesar el carrito dentro del flujo del e-commerce.
/// </remarks>
public class CartException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CartException"/>.
    /// </summary>
    public CartException()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CartException"/> con un mensaje descriptivo.
    /// </summary>
    /// <param name="message">Mensaje que describe el error del carrito.</param>
    public CartException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CartException"/> con un mensaje descriptivo
    /// y una excepción interna asociada.
    /// </summary>
    /// <param name="message">Mensaje que describe el error del carrito.</param>
    /// <param name="innerException">Excepción interna asociada al error actual.</param>
    public CartException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}