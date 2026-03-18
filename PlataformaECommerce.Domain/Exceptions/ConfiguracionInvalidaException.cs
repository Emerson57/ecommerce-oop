namespace PlataformaECommerce.Domain.Exceptions;

/// <summary>
/// Representa un error de validación asociado a configuración funcional del sistema.
/// </summary>
/// <remarks>
/// Debe utilizarse cuando una configuración requerida por la aplicación contiene
/// valores vacíos, incompatibles o fuera de los rangos admitidos por el negocio.
/// </remarks>
public sealed class ConfiguracionInvalidaException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ConfiguracionInvalidaException"/>.
    /// </summary>
    public ConfiguracionInvalidaException()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ConfiguracionInvalidaException"/> con un mensaje descriptivo.
    /// </summary>
    /// <param name="message">Mensaje que describe la configuración inválida.</param>
    public ConfiguracionInvalidaException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ConfiguracionInvalidaException"/> con un mensaje descriptivo
    /// y una excepción interna asociada.
    /// </summary>
    /// <param name="message">Mensaje que describe la configuración inválida.</param>
    /// <param name="innerException">Excepción interna asociada al error actual.</param>
    public ConfiguracionInvalidaException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}