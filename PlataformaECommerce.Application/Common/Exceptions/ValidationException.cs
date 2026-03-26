using PlataformaECommerce.Application.Common.Results;

namespace PlataformaECommerce.Application.Common.Exceptions;

/// <summary>
/// Representa una excepción de la capa de aplicación asociada a errores de validación.
/// </summary>
/// <remarks>
/// Esta excepción debe utilizarse cuando una solicitud de la aplicación no cumple
/// las reglas de validación requeridas antes de ejecutar un caso de uso, comando,
/// consulta o servicio de aplicación.
///
/// Su propósito es estandarizar el manejo de errores de validación y permitir
/// que capas superiores, como Web API o Middleware global, puedan transformar
/// estos errores en respuestas estructuradas y consistentes.
///
/// Esta excepción soporta tanto un mensaje general como una colección detallada
/// de errores por campo o propiedad validada.
/// </remarks>
public class ValidationException : ApplicationLayerException
{
    #region Campos privados

    /// <summary>
    /// Colección interna de errores de validación.
    /// </summary>
    private readonly Dictionary<string, string[]> _validationErrors;

    #endregion

    #region Constructores

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ValidationException"/>
    /// con un mensaje general de validación.
    /// </summary>
    /// <param name="message">Mensaje descriptivo del error de validación.</param>
    public ValidationException(string message)
        : base(Error.Validation("Application.Validation", ValidateMessage(message)))
    {
        _validationErrors = CreateEmptyValidationErrors();
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ValidationException"/>
    /// con un código de error personalizado y un mensaje descriptivo.
    /// </summary>
    /// <param name="errorCode">Código único del error.</param>
    /// <param name="message">Mensaje descriptivo del error de validación.</param>
    public ValidationException(string errorCode, string message)
        : base(Error.Validation(ValidateErrorCode(errorCode), ValidateMessage(message)))
    {
        _validationErrors = CreateEmptyValidationErrors();
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ValidationException"/>
    /// a partir de una colección estructurada de errores por campo.
    /// </summary>
    /// <param name="validationErrors">Errores de validación agrupados por campo o propiedad.</param>
    public ValidationException(IDictionary<string, string[]> validationErrors)
        : base(Error.Validation(
            "Application.Validation",
            "La solicitud contiene uno o más errores de validación."))
    {
        _validationErrors = NormalizeValidationErrors(validationErrors);
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ValidationException"/>
    /// a partir de una colección de errores simples por campo.
    /// </summary>
    /// <param name="validationErrors">Errores de validación agrupados por campo o propiedad.</param>
    public ValidationException(IDictionary<string, IEnumerable<string>> validationErrors)
        : base(Error.Validation(
            "Application.Validation",
            "La solicitud contiene uno o más errores de validación."))
    {
        _validationErrors = NormalizeValidationErrors(validationErrors);
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ValidationException"/>
    /// a partir de un conjunto de mensajes de validación generales.
    /// </summary>
    /// <param name="validationMessages">Mensajes generales de validación.</param>
    public ValidationException(IEnumerable<string> validationMessages)
        : base(Error.Validation(
            "Application.Validation",
            "La solicitud contiene uno o más errores de validación."))
    {
        _validationErrors = BuildGeneralValidationErrors(validationMessages);
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ValidationException"/>
    /// a partir de un error estructurado.
    /// </summary>
    /// <param name="error">Error estructurado asociado a la excepción.</param>
    public ValidationException(Error error)
        : base(ValidateValidationError(error))
    {
        _validationErrors = CreateEmptyValidationErrors();
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Obtiene la colección de errores de validación agrupados por campo o propiedad.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors => _validationErrors;

    /// <summary>
    /// Indica si la excepción contiene errores detallados de validación.
    /// </summary>
    public bool HasErrors => _validationErrors.Count > 0;

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
            return "La solicitud contiene errores de validación.";
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
            return "Application.Validation";
        }

        return errorCode.Trim();
    }

    /// <summary>
    /// Crea una colección vacía de errores de validación.
    /// </summary>
    /// <returns>Diccionario vacío.</returns>
    private static Dictionary<string, string[]> CreateEmptyValidationErrors()
    {
        return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Normaliza una colección de errores de validación ya agrupados por campo.
    /// </summary>
    /// <param name="validationErrors">Errores de validación a normalizar.</param>
    /// <returns>Diccionario normalizado de errores.</returns>
    private static Dictionary<string, string[]> NormalizeValidationErrors(
        IDictionary<string, string[]> validationErrors)
    {
        ArgumentNullException.ThrowIfNull(validationErrors);

        Dictionary<string, string[]> normalized = new(StringComparer.OrdinalIgnoreCase);

        foreach ((string key, string[] messages) in validationErrors)
        {
            string normalizedKey = NormalizeKey(key);
            string[] normalizedMessages = NormalizeMessages(messages);

            if (normalizedMessages.Length == 0)
            {
                continue;
            }

            normalized[normalizedKey] = normalizedMessages;
        }

        return normalized;
    }

    /// <summary>
    /// Normaliza una colección de errores de validación agrupados por campo
    /// con valores enumerables.
    /// </summary>
    /// <param name="validationErrors">Errores de validación a normalizar.</param>
    /// <returns>Diccionario normalizado de errores.</returns>
    private static Dictionary<string, string[]> NormalizeValidationErrors(
        IDictionary<string, IEnumerable<string>> validationErrors)
    {
        ArgumentNullException.ThrowIfNull(validationErrors);

        Dictionary<string, string[]> normalized = new(StringComparer.OrdinalIgnoreCase);

        foreach ((string key, IEnumerable<string> messages) in validationErrors)
        {
            string normalizedKey = NormalizeKey(key);
            string[] normalizedMessages = NormalizeMessages(messages);

            if (normalizedMessages.Length == 0)
            {
                continue;
            }

            normalized[normalizedKey] = normalizedMessages;
        }

        return normalized;
    }

    /// <summary>
    /// Construye una colección de errores generales de validación.
    /// </summary>
    /// <param name="validationMessages">Mensajes generales de validación.</param>
    /// <returns>Diccionario con errores generales.</returns>
    private static Dictionary<string, string[]> BuildGeneralValidationErrors(
        IEnumerable<string> validationMessages)
    {
        ArgumentNullException.ThrowIfNull(validationMessages);

        string[] normalizedMessages = NormalizeMessages(validationMessages);

        if (normalizedMessages.Length == 0)
        {
            return CreateEmptyValidationErrors();
        }

        return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["General"] = normalizedMessages
        };
    }

    /// <summary>
    /// Normaliza la clave de agrupación de errores.
    /// </summary>
    /// <param name="key">Clave a normalizar.</param>
    /// <returns>Clave válida y normalizada.</returns>
    private static string NormalizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return "General";
        }

        return key.Trim();
    }

    /// <summary>
    /// Normaliza un conjunto de mensajes de validación.
    /// </summary>
    /// <param name="messages">Mensajes a normalizar.</param>
    /// <returns>Arreglo de mensajes válidos, distintos y normalizados.</returns>
    private static string[] NormalizeMessages(IEnumerable<string>? messages)
    {
        if (messages is null)
        {
            return Array.Empty<string>();
        }

        return messages
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Select(m => m.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Valida que el error suministrado corresponda a un error de tipo Validation.
    /// </summary>
    /// <param name="error">Error a validar.</param>
    /// <returns>Error válido.</returns>
    private static Error ValidateValidationError(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error.Type != ErrorType.Validation)
        {
            return Error.Validation(
                string.IsNullOrWhiteSpace(error.Code) ? "Application.Validation" : error.Code,
                string.IsNullOrWhiteSpace(error.Message)
                    ? "La solicitud contiene errores de validación."
                    : error.Message);
        }

        return error;
    }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida de la excepción de validación.
    /// </summary>
    /// <returns>Cadena representativa de la excepción.</returns>
    public override string ToString()
    {
        return HasErrors
            ? $"{GetType().Name} | Code: {ErrorCode} | Errors: {string.Join(" | ", _validationErrors.Select(e => $"{e.Key}: [{string.Join(", ", e.Value)}]"))}"
            : $"{GetType().Name} | Code: {ErrorCode} | Message: {Message}";
    }

    #endregion
}