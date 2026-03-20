using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.DTOs;

namespace PlataformaECommerce.Application.Features.Products.Commands;

/// <summary>
/// Representa el comando de aplicación para aplicar una promoción porcentual a un producto.
/// </summary>
public sealed class ApplyProductPromotionCommand
{
    /// <summary>
    /// Identificador del producto al que se aplicará la promoción.
    /// </summary>
    public Guid ProductId { get; init; }

    /// <summary>
    /// Porcentaje de descuento que debe aplicarse sobre el precio actual del producto.
    /// </summary>
    public decimal DiscountPercentage { get; init; }

    /// <summary>
    /// Identificador opcional del usuario que solicita la promoción.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Motivo funcional asociado a la promoción aplicada.
    /// </summary>
    public string? Reason { get; init; }
}
