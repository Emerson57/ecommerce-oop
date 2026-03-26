using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.DTOs;

namespace PlataformaECommerce.Application.Features.Products.Commands;

/// <summary>
/// Representa el comando de aplicación para retirar la marca de destacado de un producto.
/// </summary>
public sealed class UnfeatureProductCommand
{
    /// <summary>
    /// Identificador del producto al que se le retirará la marca de destacado.
    /// </summary>
    public Guid ProductId { get; init; }

    /// <summary>
    /// Identificador opcional del usuario que solicita el cambio.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Motivo funcional asociado al retiro del destacado.
    /// </summary>
    public string? Reason { get; init; }
}
