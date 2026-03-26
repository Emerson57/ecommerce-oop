namespace PlataformaECommerce.Application.Common.Results;

/// <summary>
/// Representa el resultado tipado de una operación de la capa de aplicación,
/// indicando de forma explícita si fue exitosa o fallida.
/// </summary>
/// <typeparam name="TValue">Tipo del valor contenido en el resultado.</typeparam>
/// <remarks>
/// Esta clase permite modelar respuestas de casos de uso,
/// servicios de aplicación y consultas que necesitan retornar un valor
/// cuando la operación es exitosa.
///
/// Su propósito es estandarizar la salida tipada de la aplicación para escenarios como:
/// - consultas que retornan DTOs,
/// - comandos que devuelven identificadores,
/// - procesos de autenticación,
/// - creación de recursos,
/// - búsquedas controladas.
///
/// Un <see cref="Result{TValue}"/> válido debe cumplir una de estas dos reglas:
/// - Éxito: <see cref="Result.IsSuccess"/> = <see langword="true"/> y <see cref="Result.Error"/> = <see cref="Error.None"/>
/// - Fallo: <see cref="Result.IsFailure"/> = <see langword="true"/> y <see cref="Result.Error"/> distinto de <see cref="Error.None"/>
/// </remarks>
public sealed class Result<TValue> : Result
{
    #region Campos privados

    /// <summary>
    /// Valor interno asociado al resultado.
    /// </summary>
    private readonly TValue? _value;

    #endregion

    #region Constructores

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="value">Valor asociado al resultado.</param>
    /// <param name="isSuccess">Indica si la operación fue exitosa.</param>
    /// <param name="error">Error asociado al resultado.</param>
    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Obtiene el valor asociado al resultado cuando la operación fue exitosa.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Se produce cuando se intenta acceder al valor de un resultado fallido.
    /// </exception>
    public TValue Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException("No es posible acceder al valor de un resultado fallido.");
            }

            return _value!;
        }
    }

    /// <summary>
    /// Obtiene el valor asociado al resultado si existe; de lo contrario, devuelve el valor predeterminado.
    /// </summary>
    public TValue? ValueOrDefault => _value;

    #endregion

    #region Métodos utilitarios

    /// <summary>
    /// Ejecuta una acción solamente cuando el resultado es exitoso.
    /// </summary>
    /// <param name="action">Acción a ejecutar con el valor contenido.</param>
    /// <returns>La misma instancia actual para permitir encadenamiento.</returns>
    public Result<TValue> OnSuccess(Action<TValue> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (IsSuccess)
        {
            action(Value);
        }

        return this;
    }

    /// <summary>
    /// Ejecuta una acción solamente cuando el resultado es fallido.
    /// </summary>
    /// <param name="action">Acción a ejecutar con el error asociado.</param>
    /// <returns>La misma instancia actual para permitir encadenamiento.</returns>
    public new Result<TValue> OnFailure(Action<Error> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (IsFailure)
        {
            action(Error);
        }

        return this;
    }

    /// <summary>
    /// Transforma el valor contenido cuando el resultado es exitoso,
    /// preservando el error cuando el resultado es fallido.
    /// </summary>
    /// <typeparam name="TOutput">Tipo del nuevo valor transformado.</typeparam>
    /// <param name="mapper">Función de transformación.</param>
    /// <returns>Un nuevo resultado tipado con el valor transformado.</returns>
    public Result<TOutput> Map<TOutput>(Func<TValue, TOutput> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return IsSuccess
            ? Result.Success(mapper(Value))
            : Result.Failure<TOutput>(Error);
    }

    /// <summary>
    /// Encadena una operación que retorna otro resultado tipado
    /// cuando el resultado actual es exitoso.
    /// </summary>
    /// <typeparam name="TOutput">Tipo del nuevo valor esperado.</typeparam>
    /// <param name="binder">Función que produce el siguiente resultado.</param>
    /// <returns>Resultado encadenado.</returns>
    public Result<TOutput> Bind<TOutput>(Func<TValue, Result<TOutput>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        return IsSuccess
            ? binder(Value)
            : Result.Failure<TOutput>(Error);
    }

    /// <summary>
    /// Convierte el resultado tipado actual en un resultado no tipado,
    /// preservando su estado de éxito o fallo.
    /// </summary>
    /// <returns>Instancia no tipada de <see cref="Result"/>.</returns>
    public Result ToResult()
    {
        return IsSuccess
            ? Result.Success()
            : Result.Failure(Error);
    }

    /// <summary>
    /// Devuelve el valor contenido si el resultado es exitoso;
    /// en caso contrario, devuelve el valor alternativo especificado.
    /// </summary>
    /// <param name="defaultValue">Valor alternativo a retornar en caso de fallo.</param>
    /// <returns>Valor exitoso o valor alternativo.</returns>
    public TValue GetValueOrDefault(TValue defaultValue)
    {
        return IsSuccess ? Value : defaultValue;
    }

    /// <summary>
    /// Devuelve el valor contenido si el resultado es exitoso;
    /// en caso contrario, calcula y devuelve un valor alternativo a partir del error.
    /// </summary>
    /// <param name="defaultFactory">Función que construye el valor alternativo.</param>
    /// <returns>Valor exitoso o valor alternativo calculado.</returns>
    public TValue GetValueOrElse(Func<Error, TValue> defaultFactory)
    {
        ArgumentNullException.ThrowIfNull(defaultFactory);

        return IsSuccess ? Value : defaultFactory(Error);
    }

    #endregion

    #region Operadores implícitos

    /// <summary>
    /// Convierte implícitamente un valor al resultado exitoso correspondiente.
    /// </summary>
    /// <param name="value">Valor a envolver en un resultado exitoso.</param>
    public static implicit operator Result<TValue>(TValue value)
    {
        return Result.Success(value);
    }

    /// <summary>
    /// Convierte implícitamente un error en un resultado fallido del tipo correspondiente.
    /// </summary>
    /// <param name="error">Error a envolver en un resultado fallido.</param>
    public static implicit operator Result<TValue>(Error error)
    {
        return Result.Failure<TValue>(error);
    }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del resultado tipado.
    /// </summary>
    /// <returns>Cadena representativa del estado del resultado.</returns>
    public override string ToString()
    {
        return IsSuccess
            ? $"Success<{typeof(TValue).Name}>"
            : $"Failure<{typeof(TValue).Name}> | {Error}";
    }

    #endregion
}