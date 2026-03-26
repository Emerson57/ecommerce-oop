using FluentValidation;
using PlataformaECommerce.Application.Features.Categories.Commands;

namespace PlataformaECommerce.Application.Features.Categories.Validators;

/// <summary>
/// Valida la estructura base del comando de importación XML de categorías.
/// </summary>
public sealed class ImportCategoriesFromXmlCommandValidator : AbstractValidator<ImportCategoriesFromXmlCommand>
{
    /// <summary>
    /// Inicializa una nueva instancia del validador.
    /// </summary>
    public ImportCategoriesFromXmlCommandValidator()
    {
        RuleFor(x => x.XmlContent)
            .NotEmpty()
            .WithMessage("El contenido XML de categorías es obligatorio.");
    }
}
