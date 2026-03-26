using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Features.Admin.Validators;

namespace PlataformaECommerce.Tests.Application.Admin;

[TestFixture]
public class ResetUserPasswordCommandValidatorTests
{
    private readonly ResetUserPasswordCommandValidator _validator = new();

    [Test]
    public async Task ValidateAsync_UsuarioObjetivoVacio_RetornaErrorDeValidacion()
    {
        var result = await _validator.ValidateAsync(CreateValidCommand(targetUserId: Guid.Empty));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(error => error.PropertyName == nameof(ResetUserPasswordCommand.TargetUserId)), Is.True);
    }

    [Test]
    public async Task ValidateAsync_ContrasenaDebil_RetornaErrorDeValidacion()
    {
        var result = await _validator.ValidateAsync(CreateValidCommand(newPassword: "password", confirmPassword: "password"));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(error => error.PropertyName == nameof(ResetUserPasswordCommand.NewPassword)), Is.True);
    }

    [Test]
    public async Task ValidateAsync_ConfirmacionDistinta_RetornaErrorDeValidacion()
    {
        var result = await _validator.ValidateAsync(CreateValidCommand(confirmPassword: "Password#9999"));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(error => error.PropertyName == nameof(ResetUserPasswordCommand.ConfirmPassword)), Is.True);
    }

    private static ResetUserPasswordCommand CreateValidCommand(
        Guid? targetUserId = null,
        string newPassword = "Password#2027",
        string confirmPassword = "Password#2027")
    {
        return new ResetUserPasswordCommand
        {
            TargetUserId = targetUserId ?? Guid.Parse("44444444-4444-4444-4444-444444444444"),
            NewPassword = newPassword,
            ConfirmPassword = confirmPassword,
            Source = "AdminPortal"
        };
    }
}
