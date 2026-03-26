using FluentValidation;
using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Application.Features.Auth.Commands;

namespace PlataformaECommerce.Application.Features.Auth.Validators;

/// <summary>
/// Valida la entrada estructural del inicio de recuperación de contraseña.
/// </summary>
public sealed class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    private const int EmailMaxLength = 150;
    private const int IpAddressMaxLength = 64;
    private const int UserAgentMaxLength = 512;
    private const int SourceMaxLength = 50;
    private const int ExternalReferenceMaxLength = 150;

    /// <summary>
    /// Inicializa una nueva instancia del validador.
    /// </summary>
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
                .WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress()
                .WithMessage("El correo electrónico no tiene un formato válido.")
            .MaximumLength(EmailMaxLength)
                .WithMessage($"El correo electrónico no puede superar los {EmailMaxLength} caracteres.");

        RuleFor(command => command.IpAddress)
            .MaximumLength(IpAddressMaxLength)
                .WithMessage($"La dirección IP no puede superar los {IpAddressMaxLength} caracteres.")
            .Must(BeNullOrContainMeaningfulContent)
                .WithMessage("Si se informa la dirección IP, esta no puede contener únicamente espacios en blanco.");

        RuleFor(command => command.UserAgent)
            .MaximumLength(UserAgentMaxLength)
                .WithMessage($"El User-Agent no puede superar los {UserAgentMaxLength} caracteres.")
            .Must(BeNullOrContainMeaningfulContent)
                .WithMessage("Si se informa el User-Agent, este no puede contener únicamente espacios en blanco.");

        RuleFor(command => command.Source)
            .MaximumLength(SourceMaxLength)
                .WithMessage($"El canal de origen no puede superar los {SourceMaxLength} caracteres.")
            .Must(BeNullOrContainMeaningfulContent)
                .WithMessage("Si se informa el canal de origen, este no puede contener únicamente espacios en blanco.");

        RuleFor(command => command.ExternalReference)
            .MaximumLength(ExternalReferenceMaxLength)
                .WithMessage($"La referencia externa no puede superar los {ExternalReferenceMaxLength} caracteres.")
            .Must(BeNullOrContainMeaningfulContent)
                .WithMessage("Si se informa la referencia externa, esta no puede contener únicamente espacios en blanco.");

        RuleFor(command => command.RequestedAtUtc)
            .Must(BeNullOrUtcDate)
            .WithMessage("La fecha de solicitud debe estar expresada en UTC cuando sea informada.");
    }

    private static bool BeNullOrContainMeaningfulContent(string? value)
    {
        return value is null || !string.IsNullOrWhiteSpace(value);
    }

    private static bool BeNullOrUtcDate(DateTime? value)
    {
        return !value.HasValue || value.Value.Kind == DateTimeKind.Utc;
    }
}
