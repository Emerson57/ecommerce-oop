namespace PlataformaECommerce.Application.Features.Categories.Queries;

/// <summary>
/// Representa la consulta para obtener el detalle de una categoría por su identificador.
/// </summary>
public sealed class GetCategoryByIdQuery
{
    /// <summary>
    /// Inicializa una nueva instancia de la consulta.
    /// </summary>
    public GetCategoryByIdQuery(Guid categoryId)
    {
        CategoryId = categoryId;
    }

    /// <summary>
    /// Identificador de la categoría solicitada.
    /// </summary>
    public Guid CategoryId { get; }
}
