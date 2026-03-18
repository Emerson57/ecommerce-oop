namespace PlataformaECommerce.Application.Common.Results;

/// <summary>
/// Representa un error controlado dentro de la capa de aplicación.
/// </summary>
/// <remarks>
/// Esta clase encapsula la información mínima y necesaria para expresar
/// fallos de negocio, validación, autorización o procesamiento dentro
/// de la aplicación sin depender directamente de excepciones.
///
/// Su propósito es estandarizar la forma en que los casos de uso,
/// servicios de aplicación y handlers retornan errores hacia capas
/// superiores, permitiendo una comunicación clara, consistente y trazable.
///
/// La clase se modela como un objeto inmutable y comparable por valor,
/// lo cual la hace adecuada para integrarse con patrones como:
/// - Result
/// - Result&lt;T&gt;
/// - CQRS
/// - Validación de comandos y consultas
/// </remarks>
public sealed class Error : IEquatable<Error>
{
    #region Constructores

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="Error"/>.
    /// </summary>
    /// <param name="code">Código único del error.</param>
    /// <param name="message">Mensaje descriptivo del error.</param>
    /// <param name="type">Categoría funcional del error.</param>
    private Error(string code, string message, ErrorType type)
    {
        Code = ValidateCode(code);
        Message = ValidateMessage(message);
        Type = type;
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Código único que identifica el error dentro del sistema.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Mensaje descriptivo y legible del error.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Tipo o categoría funcional del error.
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    /// Representa la ausencia de error.
    /// </summary>
    public static Error None => new(string.Empty, string.Empty, ErrorType.None);

    #endregion

    #region Métodos de fábrica

    /// <summary>
    /// Crea un error de validación.
    /// </summary>
    /// <param name="code">Código único del error.</param>
    /// <param name="message">Mensaje descriptivo del error.</param>
    /// <returns>Instancia de error de validación.</returns>
    public static Error Validation(string code, string message)
    {
        return new Error(code, message, ErrorType.Validation);
    }

    /// <summary>
    /// Crea un error de negocio o dominio aplicado a la capa de aplicación.
    /// </summary>
    /// <param name="code">Código único del error.</param>
    /// <param name="message">Mensaje descriptivo del error.</param>
    /// <returns>Instancia de error de negocio.</returns>
    public static Error Failure(string code, string message)
    {
        return new Error(code, message, ErrorType.Failure);
    }

    /// <summary>
    /// Crea un error de recurso no encontrado.
    /// </summary>
    /// <param name="code">Código único del error.</param>
    /// <param name="message">Mensaje descriptivo del error.</param>
    /// <returns>Instancia de error de recurso no encontrado.</returns>
    public static Error NotFound(string code, string message)
    {
        return new Error(code, message, ErrorType.NotFound);
    }

    /// <summary>
    /// Crea un error de conflicto.
    /// </summary>
    /// <param name="code">Código único del error.</param>
    /// <param name="message">Mensaje descriptivo del error.</param>
    /// <returns>Instancia de error de conflicto.</returns>
    public static Error Conflict(string code, string message)
    {
        return new Error(code, message, ErrorType.Conflict);
    }

    /// <summary>
    /// Crea un error de autorización o acceso denegado.
    /// </summary>
    /// <param name="code">Código único del error.</param>
    /// <param name="message">Mensaje descriptivo del error.</param>
    /// <returns>Instancia de error de autorización.</returns>
    public static Error Unauthorized(string code, string message)
    {
        return new Error(code, message, ErrorType.Unauthorized);
    }

    /// <summary>
    /// Crea un error inesperado o interno de la aplicación.
    /// </summary>
    /// <param name="code">Código único del error.</param>
    /// <param name="message">Mensaje descriptivo del error.</param>
    /// <returns>Instancia de error inesperado.</returns>
    public static Error Unexpected(string code, string message)
    {
        return new Error(code, message, ErrorType.Unexpected);
    }

    #endregion

    #region Errores comunes predefinidos

    /// <summary>
    /// Error genérico para entradas inválidas.
    /// </summary>
    public static Error InvalidInput =>
        Validation("General.InvalidInput", "La solicitud contiene datos inválidos.");

    /// <summary>
    /// Error genérico para recurso no encontrado.
    /// </summary>
    public static Error ResourceNotFound =>
        NotFound("General.NotFound", "El recurso solicitado no fue encontrado.");

    /// <summary>
    /// Error genérico para conflictos de operación.
    /// </summary>
    public static Error ConflictDetected =>
        Conflict("General.Conflict", "La operación no puede completarse debido a un conflicto de estado.");

    /// <summary>
    /// Error genérico para acceso no autorizado.
    /// </summary>
    public static Error AccessDenied =>
        Unauthorized("General.AccessDenied", "No tiene permisos suficientes para realizar esta operación.");

    /// <summary>
    /// Error genérico inesperado.
    /// </summary>
    public static Error Internal =>
        Unexpected("General.Internal", "Se produjo un error interno en la aplicación.");

    #endregion

    #region Igualdad por valor

    /// <summary>
    /// Determina si el error actual es igual a otro error.
    /// </summary>
    /// <param name="other">Otro error a comparar.</param>
    /// <returns>
    /// <see langword="true"/> si ambos errores son equivalentes por valor;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool Equals(Error? other)
    {
        if (other is null)
        {
            return false;
        }

        return Code == other.Code &&
               Message == other.Message &&
               Type == other.Type;
    }

    /// <summary>
    /// Determina si el objeto actual es igual a otro objeto.
    /// </summary>
    /// <param name="obj">Objeto a comparar.</param>
    /// <returns>
    /// <see langword="true"/> si el objeto representa un error equivalente;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return obj is Error other && Equals(other);
    }

    /// <summary>
    /// Devuelve el código hash del error actual.
    /// </summary>
    /// <returns>Código hash del error.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Code, Message, Type);
    }

    /// <summary>
    /// Evalúa si dos instancias de <see cref="Error"/> son iguales.
    /// </summary>
    public static bool operator ==(Error? left, Error? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Evalúa si dos instancias de <see cref="Error"/> son diferentes.
    /// </summary>
    public static bool operator !=(Error? left, Error? right)
    {
        return !Equals(left, right);
    }

    #endregion

    #region Métodos privados de validación

    /// <summary>
    /// Valida el código del error.
    /// </summary>
    /// <param name="code">Código a validar.</param>
    /// <returns>Código válido y normalizado.</returns>
    private static string ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return string.Empty;
        }

        return code.Trim();
    }

    /// <summary>
    /// Valida el mensaje del error.
    /// </summary>
    /// <param name="message">Mensaje a validar.</param>
    /// <returns>Mensaje válido y normalizado.</returns>
    private static string ValidateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return string.Empty;
        }

        return message.Trim();
    }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del error.
    /// </summary>
    /// <returns>Cadena representativa del error.</returns>
    public override string ToString()
    {
        return $"{Type} | {Code} | {Message}";
    }

    #endregion
}

/// <summary>
/// Define las categorías funcionales de error utilizadas por la capa de aplicación.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Indica ausencia de error.
    /// </summary>
    None = 0,

    /// <summary>
    /// Error relacionado con validación de entrada o reglas de formato.
    /// </summary>
    Validation = 1,

    /// <summary>
    /// Error de negocio o fallo funcional de la operación.
    /// </summary>
    Failure = 2,

    /// <summary>
    /// Error por recurso no encontrado.
    /// </summary>
    NotFound = 3,

    /// <summary>
    /// Error por conflicto de estado o de concurrencia lógica.
    /// </summary>
    Conflict = 4,

    /// <summary>
    /// Error por falta de autorización o permisos.
    /// </summary>
    Unauthorized = 5,

    /// <summary>
    /// Error inesperado o interno.
    /// </summary>
    Unexpected = 6
}
