using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Features.Admin.Validators;

namespace PlataformaECommerce.Tests.Application.Admin;

[TestFixture]
public class RegisterAdminCommandValidatorTests
{
    private readonly RegisterAdminCommandValidator _validator = new();

    [Test]
    public async Task ValidateAsync_NombreVacio_RetornaErrorDeValidacion()
    {
        var result = await _validator.ValidateAsync(CreateValidCommand(name: string.Empty));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(error => error.PropertyName == nameof(RegisterAdminCommand.Name)), Is.True);
    }

    [Test]
    public async Task ValidateAsync_EmailInvalido_RetornaErrorDeValidacion()
    {
        var result = await _validator.ValidateAsync(CreateValidCommand(email: "correo-invalido"));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(error => error.PropertyName == nameof(RegisterAdminCommand.Email)), Is.True);
    }

    [Test]
    public async Task ValidateAsync_ContrasenaDebil_RetornaErrorDeValidacion()
    {
        var result = await _validator.ValidateAsync(CreateValidCommand(password: "password", confirmPassword: "password"));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(error => error.PropertyName == nameof(RegisterAdminCommand.Password)), Is.True);
    }

    [Test]
    public async Task ValidateAsync_ConfirmacionDistinta_RetornaErrorDeValidacion()
    {
        var result = await _validator.ValidateAsync(CreateValidCommand(confirmPassword: "Password#9999"));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(error => error.PropertyName == nameof(RegisterAdminCommand.ConfirmPassword)), Is.True);
    }

    [Test]
    public async Task ValidateAsync_AreaInvalida_RetornaErrorDeValidacion()
    {
        var result = await _validator.ValidateAsync(CreateValidCommand(area: "TI"));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(error => error.PropertyName == nameof(RegisterAdminCommand.Area)), Is.True);
    }

    private static RegisterAdminCommand CreateValidCommand(
        string name = "Admin QA",
        string email = "admin.qa@plataforma.com",
        string password = "Password#2026",
        string confirmPassword = "Password#2026",
        string area = "Operaciones")
    {
        return new RegisterAdminCommand
        {
            Name = name,
            Email = email,
            Password = password,
            ConfirmPassword = confirmPassword,
            Area = area
        };
    }
}
