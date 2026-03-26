namespace PlataformaECommerce.Domain.Exceptions;

/// <summary>
/// Representa el error generado cuando se intenta utilizar un método de pago
/// que no se encuentra soportado por las reglas del sistema.
/// </summary>
/// <remarks>
/// Esta excepción permite proteger el flujo de pagos frente a mecanismos no habilitados,
/// no reconocidos o no compatibles con la operación comercial actual.
/// </remarks>
public class MetodoPagoNoSoportadoException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="MetodoPagoNoSoportadoException"/>.
    /// </summary>
    /// <param name="metodoPago">Nombre o identificador del método de pago no soportado.</param>
    public MetodoPagoNoSoportadoException(string metodoPago)
        : base($"El método de pago '{metodoPago}' no se encuentra soportado por el sistema.")
    {
        MetodoPago = metodoPago;
    }

    /// <summary>
    /// Obtiene el método de pago que originó la excepción.
    /// </summary>
    public string MetodoPago { get; }
}