namespace PlataformaECommerce.Application.Features.Categories.Commands;

/// <summary>
/// Representa el comando para cambiar el estado operativo de una categoría.
/// </summary>
public sealed class ChangeCategoryStatusCommand
{
    /// <summary>
    /// Identificador de la categoría.
    /// </summary>
    public Guid CategoryId { get; init; }

    /// <summary>
    /// Nuevo estado solicitado para la categoría.
    /// </summary>
    public bool IsActive { get; init; }
}
