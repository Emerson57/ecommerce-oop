using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.DTOs;

namespace PlataformaECommerce.Application.Features.Products.Commands;

/// <summary>
/// Representa el comando de aplicación para activar un producto existente dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de habilitar un producto para su operación comercial.
///
/// Su propósito es desacoplar la activación del producto respecto de la actualización general
/// de sus datos, permitiendo que esta acción se trate como una operación funcional específica
/// y claramente expresada dentro de la capa Application.
///
/// La lógica de validación del estado actual del producto y las reglas del dominio
/// deben resolverse en el servicio de aplicación correspondiente y en la entidad de dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene la representación actualizada del producto cuando la ejecución es exitosa.
/// </remarks>
public sealed class ActivateProductCommand
{
    #region Identificación

    /// <summary>
    /// Identificador único del producto que será activado.
    /// </summary>
    public Guid ProductId { get; init; }

    #endregion

    #region Contexto opcional

    /// <summary>
    /// Identificador del usuario que solicita la activación del producto.
    /// </summary>
    /// <remarks>
    /// Este valor puede utilizarse para fines de trazabilidad, auditoría
    /// o control operativo cuando la capa superior desea enviarlo explícitamente.
    /// </remarks>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Motivo funcional o comentario asociado a la activación del producto.
    /// </summary>
    /// <remarks>
    /// Este campo resulta útil para auditoría, seguimiento operativo
    /// o correlación con procesos administrativos internos.
    /// </remarks>
    public string? Reason { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada a la activación del producto.
    /// </summary>
    /// <remarks>
    /// Puede representar un número de ticket, solicitud interna,
    /// orden administrativa o correlación con otros sistemas.
    /// </remarks>
    public string? ExternalReference { get; init; }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando.
    /// </summary>
    /// <returns>Cadena representativa del comando de activación.</returns>
    public override string ToString()
    {
        return $"ActivateProductCommand | ProductId: {ProductId} | RequestedByUserId: {RequestedByUserId} | Reason: {Reason}";
    }

    #endregion
}