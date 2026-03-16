namespace PlataformaECommerce.Domain.Exceptions;

/// <summary>
/// Representa el error generado cuando una operación de pago no puede completarse satisfactoriamente.
/// </summary>
/// <remarks>
/// Esta excepción encapsula fallos funcionales del proceso de pago desde la perspectiva del negocio,
/// permitiendo registrar el motivo del rechazo o del error ocurrido durante la confirmación
/// o procesamiento de la transacción.
/// </remarks>
public class PagoFallidoException : DomainException
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="PagoFallidoException"/>
    /// con un mensaje descriptivo.
    /// </summary>
    /// <param name="message">Motivo del fallo en el proceso de pago.</param>
    public PagoFallidoException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="PagoFallidoException"/>
    /// usando el identificador del pedido y el motivo del fallo.
    /// </summary>
    /// <param name="orderId">Identificador del pedido afectado.</param>
    /// <param name="motivo">Motivo funcional del fallo del pago.</param>
    public PagoFallidoException(Guid orderId, string motivo)
        : base($"El pago del pedido con identificador '{orderId}' ha fallado. Motivo: {motivo}.")
    {
        OrderId = orderId;
        Motivo = motivo;
    }

    /// <summary>
    /// Obtiene el identificador del pedido afectado, si fue suministrado.
    /// </summary>
    public Guid? OrderId { get; }

    /// <summary>
    /// Obtiene el motivo del fallo del pago, si fue suministrado.
    /// </summary>
    public string? Motivo { get; }
}