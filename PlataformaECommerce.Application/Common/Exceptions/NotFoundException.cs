using PlataformaECommerce.Application.Common.Results;

namespace PlataformaECommerce.Application.Common.Exceptions;

/// <summary>
/// Representa una excepción de la capa de aplicación que indica
/// que un recurso solicitado no fue encontrado.
/// </summary>
/// <remarks>
/// Esta excepción debe utilizarse cuando un caso de uso, servicio de aplicación
/// o handler intenta recuperar una entidad, agregado, DTO o recurso lógico
/// que no existe dentro del contexto solicitado.
///
/// Su propósito es estandarizar los errores de tipo "no encontrado"
/// y facilitar su traducción a respuestas consistentes en capas superiores,
/// como por ejemplo respuestas HTTP 404 en una API.
///
/// Ejemplos típicos de uso:
/// - producto no encontrado por Id,
/// - usuario no encontrado por correo,
/// - pedido inexistente,
/// - carrito no encontrado para un cliente.
/// </remarks>
public class NotFoundException : ApplicationLayerException
{
    #region Constructores

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="NotFoundException"/>
    /// con un mensaje descriptivo.
    /// </summary>
    /// <param name="message">Mensaje descriptivo del error.</param>
    public NotFoundException(string message)
        : base(Error.NotFound("Application.NotFound", ValidateMessage(message)))
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="NotFoundException"/>
    /// con un código de error personalizado y un mensaje descriptivo.
    /// </summary>
    /// <param name="errorCode">Código único del error.</param>
    /// <param name="message">Mensaje descriptivo del error.</param>
    public NotFoundException(string errorCode, string message)
        : base(Error.NotFound(ValidateErrorCode(errorCode), ValidateMessage(message)))
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="NotFoundException"/>
    /// para un recurso específico.
    /// </summary>
    /// <param name="resourceName">Nombre lógico del recurso no encontrado.</param>
    public NotFoundException(string resourceName, bool useResourceFormat)
        : base(Error.NotFound(
            BuildResourceErrorCode(resourceName),
            BuildResourceNotFoundMessage(resourceName)))
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="NotFoundException"/>
    /// para un recurso específico identificado por una clave.
    /// </summary>
    /// <param name="resourceName">Nombre lógico del recurso no encontrado.</param>
    /// <param name="key">Identificador o clave del recurso solicitado.</param>
    public NotFoundException(string resourceName, object key)
        : base(Error.NotFound(
            BuildResourceErrorCode(resourceName),
            BuildResourceNotFoundMessage(resourceName, key)))
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="NotFoundException"/>
    /// a partir de un error estructurado.
    /// </summary>
    /// <param name="error">Error estructurado asociado a la excepción.</param>
    public NotFoundException(Error error)
        : base(ValidateNotFoundError(error))
    {
    }

    #endregion

    #region Métodos privados auxiliares

    /// <summary>
    /// Valida el mensaje suministrado.
    /// </summary>
    /// <param name="message">Mensaje a validar.</param>
    /// <returns>Mensaje válido y normalizado.</returns>
    private static string ValidateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "El recurso solicitado no fue encontrado.";
        }

        return message.Trim();
    }

    /// <summary>
    /// Valida el código de error suministrado.
    /// </summary>
    /// <param name="errorCode">Código a validar.</param>
    /// <returns>Código válido y normalizado.</returns>
    private static string ValidateErrorCode(string errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            return "Application.NotFound";
        }

        return errorCode.Trim();
    }

    /// <summary>
    /// Construye el código de error estándar a partir del nombre del recurso.
    /// </summary>
    /// <param name="resourceName">Nombre lógico del recurso.</param>
    /// <returns>Código de error normalizado.</returns>
    private static string BuildResourceErrorCode(string resourceName)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            return "Application.NotFound";
        }

        string normalizedResourceName = resourceName.Trim().Replace(" ", string.Empty);
        return $"{normalizedResourceName}.NotFound";
    }

    /// <summary>
    /// Construye un mensaje estándar de recurso no encontrado.
    /// </summary>
    /// <param name="resourceName">Nombre lógico del recurso.</param>
    /// <returns>Mensaje descriptivo.</returns>
    private static string BuildResourceNotFoundMessage(string resourceName)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            return "El recurso solicitado no fue encontrado.";
        }

        return $"El recurso '{resourceName.Trim()}' no fue encontrado.";
    }

    /// <summary>
    /// Construye un mensaje estándar de recurso no encontrado a partir de un identificador.
    /// </summary>
    /// <param name="resourceName">Nombre lógico del recurso.</param>
    /// <param name="key">Clave o identificador del recurso.</param>
    /// <returns>Mensaje descriptivo.</returns>
    private static string BuildResourceNotFoundMessage(string resourceName, object key)
    {
        string normalizedResourceName = string.IsNullOrWhiteSpace(resourceName)
            ? "recurso"
            : resourceName.Trim();

        string normalizedKey = key?.ToString()?.Trim() ?? "desconocido";

        return $"El recurso '{normalizedResourceName}' con identificador '{normalizedKey}' no fue encontrado.";
    }

    /// <summary>
    /// Valida que el error suministrado corresponda a un error de tipo NotFound.
    /// </summary>
    /// <param name="error">Error a validar.</param>
    /// <returns>Error válido.</returns>
    private static Error ValidateNotFoundError(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error.Type != ErrorType.NotFound)
        {
            return Error.NotFound(
                string.IsNullOrWhiteSpace(error.Code) ? "Application.NotFound" : error.Code,
                string.IsNullOrWhiteSpace(error.Message)
                    ? "El recurso solicitado no fue encontrado."
                    : error.Message);
        }

        return error;
    }

    #endregion
}