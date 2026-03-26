using FluentValidation;
using PlataformaECommerce.Application.Features.Categories.Commands;

namespace PlataformaECommerce.Application.Features.Categories.Validators;

/// <summary>
/// Valida la estructura de entrada para actualizar categorías.
/// </summary>
public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    private const int MaxNameLength = 120;
    private const int MaxSlugLength = 140;
    private const int MaxDescriptionLength = 500;

    /// <summary>
    /// Inicializa una nueva instancia del validador.
    /// </summary>
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El identificador de la categoría es obligatorio.");

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("El nombre de la categoría es obligatorio.")
            .MaximumLength(MaxNameLength)
                .WithMessage($"El nombre de la categoría no puede superar los {MaxNameLength} caracteres.");

        RuleFor(x => x.Slug)
            .NotEmpty()
                .WithMessage("El slug de la categoría es obligatorio.")
            .MaximumLength(MaxSlugLength)
                .WithMessage($"El slug de la categoría no puede superar los {MaxSlugLength} caracteres.")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .WithMessage("El slug de la categoría debe contener solo letras minúsculas, números y guiones.");

        RuleFor(x => x.Description)
            .MaximumLength(MaxDescriptionLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage($"La descripción de la categoría no puede superar los {MaxDescriptionLength} caracteres.");

        RuleFor(x => x.ParentCategoryId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("La categoría padre no puede usar un identificador vacío.");
    }
}
