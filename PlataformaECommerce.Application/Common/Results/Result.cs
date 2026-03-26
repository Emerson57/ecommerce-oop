namespace PlataformaECommerce.Application.Common.Results;

/// <summary>
/// Representa el resultado de una operación de la capa de aplicación,
/// indicando de forma explícita si fue exitosa o fallida.
/// </summary>
/// <remarks>
/// Esta clase permite modelar respuestas de casos de uso,
/// servicios de aplicación y procesos internos sin depender exclusivamente
/// de excepciones para comunicar errores esperados.
///
/// Su propósito es estandarizar la salida de la aplicación para escenarios como:
/// - ejecución de comandos,
/// - validaciones de negocio,
/// - búsquedas y consultas,
/// - procesos transaccionales,
/// - flujos de autenticación o autorización.
///
/// Un <see cref="Result"/> válido debe cumplir una de estas dos reglas:
/// - Éxito: <see cref="IsSuccess"/> = <see langword="true"/> y <see cref="Error"/> = <see cref="Results.Error.None"/>
/// - Fallo: <see cref="IsFailure"/> = <see langword="true"/> y <see cref="Error"/> distinto de <see cref="Results.Error.None"/>
/// </remarks>
public class Result
{
    #region Constructores

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="Result"/>.
    /// </summary>
    /// <param name="isSuccess">Indica si la operación fue exitosa.</param>
    /// <param name="error">Error asociado a la operación.</param>
    /// <exception cref="InvalidOperationException">
    /// Se produce cuando la combinación de éxito/error no cumple las reglas de consistencia del resultado.
    /// </exception>
    protected internal Result(bool isSuccess, Error error)
    {
        ValidateState(isSuccess, error);

        IsSuccess = isSuccess;
        Error = error;
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Indica si la operación fue exitosa.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indica si la operación falló.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Error asociado al resultado.
    /// </summary>
    public Error Error { get; }

    #endregion

    #region Métodos de fábrica

    /// <summary>
    /// Crea un resultado exitoso sin valor asociado.
    /// </summary>
    /// <returns>Instancia exitosa de <see cref="Result"/>.</returns>
    public static Result Success()
    {
        return new Result(true, Error.None);
    }

    /// <summary>
    /// Crea un resultado fallido con un error asociado.
    /// </summary>
    /// <param name="error">Error que describe el motivo del fallo.</param>
    /// <returns>Instancia fallida de <see cref="Result"/>.</returns>
    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }

    /// <summary>
    /// Crea un resultado exitoso con un valor asociado.
    /// </summary>
    /// <typeparam name="TValue">Tipo del valor de retorno.</typeparam>
    /// <param name="value">Valor asociado al resultado exitoso.</param>
    /// <returns>Instancia exitosa de <see cref="Result{TValue}"/>.</returns>
    public static Result<TValue> Success<TValue>(TValue value)
    {
        return new Result<TValue>(value, true, Error.None);
    }

    /// <summary>
    /// Crea un resultado fallido con un error asociado para un tipo de valor específico.
    /// </summary>
    /// <typeparam name="TValue">Tipo del valor esperado.</typeparam>
    /// <param name="error">Error que describe el motivo del fallo.</param>
    /// <returns>Instancia fallida de <see cref="Result{TValue}"/>.</returns>
    public static Result<TValue> Failure<TValue>(Error error)
    {
        return new Result<TValue>(default, false, error);
    }

    /// <summary>
    /// Crea un resultado a partir de una condición booleana.
    /// </summary>
    /// <param name="condition">Condición que representa éxito o fallo.</param>
    /// <param name="error">Error a devolver si la condición no se cumple.</param>
    /// <returns>
    /// <see cref="Success"/> si la condición es verdadera;
    /// en caso contrario, <see cref="Failure(Error)"/>.
    /// </returns>
    public static Result Create(bool condition, Error error)
    {
        return condition ? Success() : Failure(error);
    }

    /// <summary>
    /// Crea un resultado con valor a partir de una condición booleana.
    /// </summary>
    /// <typeparam name="TValue">Tipo del valor de retorno.</typeparam>
    /// <param name="condition">Condición que representa éxito o fallo.</param>
    /// <param name="value">Valor a devolver si la condición se cumple.</param>
    /// <param name="error">Error a devolver si la condición no se cumple.</param>
    /// <returns>
    /// Un resultado exitoso con valor si la condición es verdadera;
    /// en caso contrario, un resultado fallido.
    /// </returns>
    public static Result<TValue> Create<TValue>(bool condition, TValue value, Error error)
    {
        return condition ? Success(value) : Failure<TValue>(error);
    }

    #endregion

    #region Métodos utilitarios

    /// <summary>
    /// Ejecuta una acción solamente cuando el resultado es exitoso.
    /// </summary>
    /// <param name="action">Acción a ejecutar.</param>
    /// <returns>La misma instancia actual para permitir encadenamiento.</returns>
    public Result OnSuccess(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (IsSuccess)
        {
            action();
        }

        return this;
    }

    /// <summary>
    /// Ejecuta una acción solamente cuando el resultado es fallido.
    /// </summary>
    /// <param name="action">Acción a ejecutar con el error asociado.</param>
    /// <returns>La misma instancia actual para permitir encadenamiento.</returns>
    public Result OnFailure(Action<Error> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (IsFailure)
        {
            action(Error);
        }

        return this;
    }

    /// <summary>
    /// Convierte el resultado actual en un resultado tipado, preservando su estado.
    /// </summary>
    /// <typeparam name="TValue">Tipo del valor deseado.</typeparam>
    /// <param name="value">Valor a utilizar si el resultado actual es exitoso.</param>
    /// <returns>
    /// Un resultado exitoso tipado cuando el estado actual es exitoso;
    /// en caso contrario, un resultado fallido con el mismo error.
    /// </returns>
    public Result<TValue> ToResult<TValue>(TValue value)
    {
        return IsSuccess
            ? Success(value)
            : Failure<TValue>(Error);
    }

    #endregion

    #region Validaciones internas

    /// <summary>
    /// Valida la coherencia interna del estado del resultado.
    /// </summary>
    /// <param name="isSuccess">Indicador de éxito.</param>
    /// <param name="error">Error asociado.</param>
    /// <exception cref="InvalidOperationException">
    /// Se produce cuando la combinación de éxito/error no es válida.
    /// </exception>
    private static void ValidateState(bool isSuccess, Error error)
    {
        if (error is null)
        {
            throw new InvalidOperationException("El error del resultado no puede ser nulo.");
        }

        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("Un resultado exitoso no puede contener un error distinto de Error.None.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("Un resultado fallido debe contener un error distinto de Error.None.");
        }
    }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del resultado.
    /// </summary>
    /// <returns>Cadena representativa del estado del resultado.</returns>
    public override string ToString()
    {
        return IsSuccess
            ? "Success"
            : $"Failure | {Error}";
    }

    #endregion
}