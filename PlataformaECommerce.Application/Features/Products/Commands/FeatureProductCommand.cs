using PlataformaECommerce.Application.Abstractions;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.DTOs;

namespace PlataformaECommerce.Application.Features.Products.Commands;

/// <summary>
/// Representa el comando de aplicación para destacar un producto dentro del catálogo.
/// </summary>
public sealed class FeatureProductCommand : ICommand<Result<ProductResponseDto>>
{
    /// <summary>
    /// Identificador del producto que será marcado como destacado.
    /// </summary>
    public Guid ProductId { get; init; }

    /// <summary>
    /// Identificador opcional del usuario que solicita el cambio.
    /// </summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Motivo funcional asociado al cambio de destacado.
    /// </summary>
    public string? Reason { get; init; }
}
