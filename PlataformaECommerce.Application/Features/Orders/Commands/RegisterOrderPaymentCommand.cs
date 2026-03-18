using PlataformaECommerce.Application.Abstractions;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Orders.DTOs;

namespace PlataformaECommerce.Application.Features.Orders.Commands;

/// <summary>
/// Representa el comando de aplicación para registrar el pago de un pedido
/// dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de confirmación o registro exitoso del pago
/// asociado a un pedido.
///
/// Su responsabilidad es transportar la información del contexto de pago
/// necesaria para que el handler ejecute el caso de uso de manera trazable,
/// segura y consistente, sin contener lógica de negocio ni reglas del dominio.
///
/// La validación de formatos, obligatoriedad y consistencia de importes debe
/// resolverse en la capa Application, mientras que las validaciones definitivas
/// del estado permitido del pedido deben reforzarse en el dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene un <see cref="OrderDetailDto"/> cuando la ejecución es exitosa.
/// </remarks>
public sealed class RegisterOrderPaymentCommand : ICommand<Result<OrderDetailDto>>
{
    #region Identificación principal

    /// <summary>
    /// Identificador único del pedido sobre el cual se registrará el pago.
    /// </summary>
    public Guid OrderId { get; init; }

    #endregion

    #region Información del pago

    /// <summary>
    /// Referencia o identificador único de la transacción de pago.
    /// </summary>
    /// <remarks>
    /// Este valor permite correlacionar el pedido con el evento externo
    /// proveniente de una pasarela de pagos, banco o sistema administrativo.
    /// </remarks>
    public string PaymentReference { get; init; } = string.Empty;

    /// <summary>
    /// Método de pago utilizado para la transacción.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - TarjetaCredito
    /// - TarjetaDebito
    /// - PSE
    /// - Transferencia
    /// - Efectivo
    /// - Wallet
    /// </remarks>
    public string PaymentMethod { get; init; } = string.Empty;

    /// <summary>
    /// Monto efectivamente pagado o aprobado para el pedido.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Código de moneda asociado al monto del pago.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - COP
    /// - USD
    /// - EUR
    /// </remarks>
    public string Currency { get; init; } = "COP";

    /// <summary>
    /// Fecha y hora UTC en la que el sistema externo reportó el pago como exitoso.
    /// </summary>
    public DateTime? PaidAtUtc { get; init; }

    /// <summary>
    /// Nombre del proveedor o pasarela de pago utilizada.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - Wompi
    /// - MercadoPago
    /// - PayU
    /// - Stripe
    /// - Banco
    /// </remarks>
    public string? PaymentProvider { get; init; }

    /// <summary>
    /// Observación funcional o comentario asociado al registro del pago.
    /// </summary>
    public string? Notes { get; init; }

    #endregion

    #region Contexto y trazabilidad

    /// <summary>
    /// Identificador del usuario que solicita o ejecuta el registro del pago.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Dirección IP desde la cual se originó la solicitud, cuando esté disponible.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Canal de origen desde el cual se genera la solicitud.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada al proceso de pago.
    /// </summary>
    public string? ExternalReference { get; init; }

    /// <summary>
    /// Fecha y hora UTC en la que la capa superior registró la solicitud.
    /// </summary>
    public DateTime? RequestedAtUtc { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si el monto informado es estructuralmente válido.
    /// </summary>
    public bool HasValidAmount => Amount > 0;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando de registro de pago.
    /// </summary>
    /// <returns>Cadena representativa del comando.</returns>
    public override string ToString()
    {
        return $"RegisterOrderPaymentCommand | OrderId: {OrderId} | PaymentReference: {PaymentReference} | PaymentMethod: {PaymentMethod} | Amount: {Currency} {Amount:N2}";
    }

    #endregion
}