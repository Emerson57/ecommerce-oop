namespace PlataformaECommerce.Domain.Exceptions;

/// <summary>
/// Representa la excepción base para todos los errores de negocio generados dentro del dominio.
/// </summary>
/// <remarks>
/// Esta excepción debe utilizarse como raíz común para todas las validaciones, restricciones
/// e inconsistencias propias de las reglas del negocio. Su propósito es diferenciar los errores
/// del dominio frente a errores técnicos, de infraestructura o de configuración.
/// </remarks>
public class DomainException : Exception
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="DomainException"/>.
    /// </summary>
    public DomainException()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="DomainException"/> con un mensaje descriptivo.
    /// </summary>
    /// <param name="message">Mensaje que describe el error del dominio.</param>
    public DomainException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="DomainException"/> con un mensaje descriptivo
    /// y una excepción interna que originó el error.
    /// </summary>
    /// <param name="message">Mensaje que describe el error del dominio.</param>
    /// <param name="innerException">Excepción interna asociada al error actual.</param>
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}