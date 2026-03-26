using FluentValidation;
using PlataformaECommerce.Application.Features.Products.Commands;

namespace PlataformaECommerce.Application.Features.Products.Validators;

/// <summary>
/// Valida la estructura base de una importación masiva de productos.
/// </summary>
public sealed class ImportProductsCommandValidator : AbstractValidator<ImportProductsCommand>
{
    /// <summary>
    /// Inicializa una nueva instancia del validador.
    /// </summary>
    public ImportProductsCommandValidator()
    {
        RuleFor(x => x.Rows)
            .NotNull()
            .WithMessage("La importación de productos requiere una colección de filas válida.")
            .Must(rows => rows.Count > 0)
            .WithMessage("La importación de productos requiere al menos una fila válida.");

        RuleForEach(x => x.Rows)
            .NotNull()
            .WithMessage("La importación de productos no admite filas nulas.");
    }
}
