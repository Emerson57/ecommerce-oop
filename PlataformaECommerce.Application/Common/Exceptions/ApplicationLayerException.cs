using PlataformaECommerce.Application.Common.Results;

namespace PlataformaECommerce.Application.Common.Exceptions;

/// <summary>
/// Representa la excepción base de la capa de aplicación.
/// </summary>
/// <remarks>
/// Esta excepción debe utilizarse para modelar errores controlados que ocurren
/// durante la ejecución de casos de uso, handlers, servicios de aplicación
/// o validaciones propias de la capa Application.
///
/// Su propósito es establecer una base común para excepciones derivadas como:
/// - <c>NotFoundException</c>
/// - <c>ValidationException</c>
/// - excepciones funcionales específicas de la aplicación
///
/// Esta clase permite encapsular información estructurada del error,
/// como código y categoría, facilitando:
/// - trazabilidad,
/// - estandarización,
/// - logging,
/// - conversión a respuestas HTTP,
/// - y manejo consistente en middlewares o filtros globales.
///
/// Esta excepción no sustituye a <see cref="Result"/> o <see cref="Result{TValue}"/>,
/// sino que complementa aquellos escenarios donde una excepción controlada
/// resulta más adecuada dentro del flujo de aplicación.
/// </remarks>
public class ApplicationLayerException : Exception
{
    #region Constructores

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ApplicationLayerException"/>.
    /// </summary>
    public ApplicationLayerException()
        : base("Se produjo un error en la capa de aplicación.")
    {
        ErrorCode = "Application.General";
        ErrorType = ErrorType.Unexpected;
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ApplicationLayerException"/>
    /// con un mensaje descriptivo.
    /// </summary>
    /// <param name="message">Mensaje descriptivo del error.</param>
    public ApplicationLayerException(string message)
        : base(ValidateMessage(message))
    {
        ErrorCode = "Application.General";
        ErrorType = ErrorType.Unexpected;
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ApplicationLayerException"/>
    /// con un mensaje descriptivo y una excepción interna.
    /// </summary>
    /// <param name="message">Mensaje descriptivo del error.</param>
    /// <param name="innerException">Excepción interna asociada.</param>
    public ApplicationLayerException(string message, Exception innerException)
        : base(ValidateMessage(message), innerException)
    {
        ErrorCode = "Application.General";
        ErrorType = ErrorType.Unexpected;
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ApplicationLayerException"/>
    /// con código y mensaje de error.
    /// </summary>
    /// <param name="errorCode">Código único del error.</param>
    /// <param name="message">Mensaje descriptivo del error.</param>
    public ApplicationLayerException(string errorCode, string message)
        : base(ValidateMessage(message))
    {
        ErrorCode = ValidateErrorCode(errorCode);
        ErrorType = ErrorType.Failure;
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ApplicationLayerException"/>
    /// con código, mensaje y excepción interna.
    /// </summary>
    /// <param name="errorCode">Código único del error.</param>
    /// <param name="message">Mensaje descriptivo del error.</param>
    /// <param name="innerException">Excepción interna asociada.</param>
    public ApplicationLayerException(string errorCode, string message, Exception innerException)
        : base(ValidateMessage(message), innerException)
    {
        ErrorCode = ValidateErrorCode(errorCode);
        ErrorType = ErrorType.Failure;
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ApplicationLayerException"/>
    /// a partir de un objeto <see cref="Error"/>.
    /// </summary>
    /// <param name="error">Error estructurado de la capa de aplicación.</param>
    public ApplicationLayerException(Error error)
        : base(ValidateError(error).Message)
    {
        ErrorCode = error.Code;
        ErrorType = error.Type;
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ApplicationLayerException"/>
    /// a partir de un objeto <see cref="Error"/> y una excepción interna.
    /// </summary>
    /// <param name="error">Error estructurado de la capa de aplicación.</param>
    /// <param name="innerException">Excepción interna asociada.</param>
    public ApplicationLayerException(Error error, Exception innerException)
        : base(ValidateError(error).Message, innerException)
    {
        ErrorCode = error.Code;
        ErrorType = error.Type;
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Código único del error asociado a la excepción.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Tipo o categoría funcional del error asociado.
    /// </summary>
    public ErrorType ErrorType { get; }

    /// <summary>
    /// Representación estructurada del error asociado a la excepción.
    /// </summary>
    public Error Error => ErrorType switch
    {
        ErrorType.Validation => Results.Error.Validation(ErrorCode, Message),
        ErrorType.Failure => Results.Error.Failure(ErrorCode, Message),
        ErrorType.NotFound => Results.Error.NotFound(ErrorCode, Message),
        ErrorType.Conflict => Results.Error.Conflict(ErrorCode, Message),
        ErrorType.Unauthorized => Results.Error.Unauthorized(ErrorCode, Message),
        ErrorType.Unexpected => Results.Error.Unexpected(ErrorCode, Message),
        _ => Results.Error.None
    };

    #endregion

    #region Métodos privados de validación

    /// <summary>
    /// Valida el mensaje de la excepción.
    /// </summary>
    /// <param name="message">Mensaje a validar.</param>
    /// <returns>Mensaje válido y normalizado.</returns>
    private static string ValidateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "Se produjo un error en la capa de aplicación.";
        }

        return message.Trim();
    }

    /// <summary>
    /// Valida el código del error.
    /// </summary>
    /// <param name="errorCode">Código a validar.</param>
    /// <returns>Código válido y normalizado.</returns>
    private static string ValidateErrorCode(string errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            return "Application.General";
        }

        return errorCode.Trim();
    }

    /// <summary>
    /// Valida el objeto <see cref="Error"/> suministrado.
    /// </summary>
    /// <param name="error">Error a validar.</param>
    /// <returns>Instancia válida del error.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se produce cuando el error es nulo.
    /// </exception>
    private static Error ValidateError(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return error;
    }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida de la excepción de aplicación.
    /// </summary>
    /// <returns>Cadena representativa de la excepción.</returns>
    public override string ToString()
    {
        return $"{GetType().Name} | Code: {ErrorCode} | Type: {ErrorType} | Message: {Message}";
    }

    #endregion
}