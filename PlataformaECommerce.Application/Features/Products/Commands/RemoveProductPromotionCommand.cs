using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.DTOs;

namespace PlataformaECommerce.Application.Features.Products.Commands;

/// <summary>
/// Representa el comando de aplicación para retirar una promoción activa de un producto.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de restaurar el precio base del producto,
/// desacoplando la reversión promocional del resto de operaciones de actualización del catálogo.
/// </remarks>
public sealed class RemoveProductPromotionCommand
{
    /// <summary>
    /// Identificador del producto al que se le retirará la promoción vigente.
    /// </summary>
    public Guid ProductId { get; init; }

    /// <summary>
    /// Identificador opcional del usuario que solicita la restauración del precio base.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Motivo funcional asociado al retiro de la promoción.
    /// </summary>
    public string? Reason { get; init; }
}
