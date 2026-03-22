using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Users.Commands;
using PlataformaECommerce.Application.Features.Users.DTOs;
using PlataformaECommerce.Application.Features.Users.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Pages.Auth;

namespace PlataformaECommerce.Tests.Web.Auth;

[TestFixture]
public class RegisterPageModelTests
{
    [Test]
    public async Task OnPostAsync_FormularioValido_RegistraCuentaYRedirigeALogin()
    {
        FakeUserApplicationService service = new();
        RegisterModel pageModel = CreatePageModel(service, remoteIpAddress: "10.20.30.40");
        pageModel.Input = new RegisterModel.InputModel
        {
            Name = "Cliente Demo",
            Email = "cliente@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            PreferencesText = "tecnologia, hogar",
            AcceptTermsAndConditions = true,
            AcceptPrivacyPolicy = true,
            AcceptMarketingCommunications = true
        };

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(pageModel.StatusMessage, Does.Contain("La cuenta fue creada correctamente"));
        Assert.That(service.LastRegisterCommand?.Email, Is.EqualTo("cliente@plataforma.com"));
        Assert.That(service.LastRegisterCommand?.Source, Is.EqualTo("Web.Auth.Register"));
        Assert.That(service.LastRegisterCommand?.IpAddress, Is.EqualTo("10.20.30.40"));
        Assert.That(service.LastRegisterCommand?.Preferences.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task OnPostAsync_ModeloInvalido_NoInvocaRegistro()
    {
        FakeUserApplicationService service = new();
        RegisterModel pageModel = CreatePageModel(service);
        pageModel.ModelState.AddModelError(nameof(RegisterModel.InputModel.Email), "Email inválido.");

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(service.RegisterCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task OnPostAsync_AplicacionRetornaError_PublicaMensajeFuncional()
    {
        FakeUserApplicationService service = new()
        {
            RegisterResult = Result.Failure<CustomerDto>(Error.Conflict("Users.EmailAlreadyExists", "Ya existe un usuario registrado con el correo 'cliente@plataforma.com'."))
        };
        RegisterModel pageModel = CreatePageModel(service);
        pageModel.Input = new RegisterModel.InputModel
        {
            Name = "Cliente Demo",
            Email = "cliente@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            AcceptTermsAndConditions = true,
            AcceptPrivacyPolicy = true
        };

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.ErrorMessage, Does.Contain("cliente@plataforma.com"));
    }

    [Test]
    public async Task OnGetEmailAvailabilityAsync_CorreoDisponible_RetornaDisponibilidadPositiva()
    {
        FakeUserApplicationService service = new()
        {
            GetUserByEmailResult = Result.Failure<UserDto>(Error.NotFound("Users.NotFoundByEmail", "No se encontró un usuario con el correo 'nuevo@plataforma.com'."))
        };
        RegisterModel pageModel = CreatePageModel(service);

        IActionResult actionResult = await pageModel.OnGetEmailAvailabilityAsync("nuevo@plataforma.com", CancellationToken.None);
        JsonResult result = (JsonResult)actionResult;

        Assert.That(GetBoolean(result.Value, "IsAvailable"), Is.True);
        Assert.That(GetString(result.Value, "Code"), Is.EqualTo("Register.EmailAvailable"));
        Assert.That(GetString(result.Value, "Message"), Does.Contain("disponible"));
    }

    [Test]
    public async Task OnGetEmailAvailabilityAsync_CorreoExistente_RetornaDisponibilidadNegativa()
    {
        FakeUserApplicationService service = new()
        {
            GetUserByEmailResult = Result.Success(new UserDto
            {
                Id = Guid.NewGuid(),
                Name = "Cliente Demo",
                Email = "cliente@plataforma.com",
                Role = RolUsuario.Cliente,
                IsActive = true,
                IsEmailConfirmed = true,
                CreatedAtUtc = DateTime.UtcNow
            })
        };
        RegisterModel pageModel = CreatePageModel(service);

        IActionResult actionResult = await pageModel.OnGetEmailAvailabilityAsync("cliente@plataforma.com", CancellationToken.None);
        JsonResult result = (JsonResult)actionResult;

        Assert.That(GetBoolean(result.Value, "IsAvailable"), Is.False);
        Assert.That(GetString(result.Value, "Code"), Is.EqualTo("Register.EmailAlreadyExists"));
        Assert.That(GetString(result.Value, "Message"), Does.Contain("ya se encuentra registrado"));
    }

    [Test]
    public async Task OnGetEmailAvailabilityAsync_CancelacionEsperada_RetornaRespuestaControlada()
    {
        FakeUserApplicationService service = new()
        {
            ThrowOperationCanceledOnGetUserByEmail = true
        };
        RegisterModel pageModel = CreatePageModel(service);
        CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        IActionResult actionResult = await pageModel.OnGetEmailAvailabilityAsync("cliente@plataforma.com", cancellationTokenSource.Token);

        Assert.That(actionResult, Is.TypeOf<NoContentResult>());
    }

    [Test]
    public async Task OnGetEmailAvailabilityAsync_IndisponibilidadDeInfraestructura_RetornaServicioNoDisponible()
    {
        FakeUserApplicationService service = new()
        {
            ThrowTaskCanceledOnGetUserByEmail = true
        };
        RegisterModel pageModel = CreatePageModel(service);

        IActionResult actionResult = await pageModel.OnGetEmailAvailabilityAsync("cliente@plataforma.com", CancellationToken.None);

        Assert.That(actionResult, Is.TypeOf<ObjectResult>());
        ObjectResult result = (ObjectResult)actionResult;
        Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status503ServiceUnavailable));
        Assert.That(GetString(result.Value, "Code"), Is.EqualTo("Register.EmailAvailabilityUnavailable"));
        Assert.That(GetBoolean(result.Value, "IsTransientFailure"), Is.True);
        Assert.That(GetString(result.Value, "Message"), Does.Contain("No fue posible validar"));
    }

    private static RegisterModel CreatePageModel(FakeUserApplicationService userApplicationService, string? remoteIpAddress = null)
    {
        RegisterModel pageModel = new(userApplicationService);
        DefaultHttpContext httpContext = new();

        if (!string.IsNullOrWhiteSpace(remoteIpAddress))
        {
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(remoteIpAddress);
        }

        pageModel.PageContext = new PageContext
        {
            HttpContext = httpContext
        };
        pageModel.TempData = new TempDataDictionary(httpContext, new FakeTempDataProvider());
        return pageModel;
    }

    private static bool GetBoolean(object? instance, string propertyName)
    {
        return (bool)(instance?.GetType().GetProperty(propertyName)?.GetValue(instance)
            ?? throw new InvalidOperationException($"No se encontró la propiedad '{propertyName}'."));
    }

    private static string GetString(object? instance, string propertyName)
    {
        return instance?.GetType().GetProperty(propertyName)?.GetValue(instance)?.ToString()
            ?? throw new InvalidOperationException($"No se encontró la propiedad '{propertyName}'.");
    }

    private sealed class FakeUserApplicationService : IUserApplicationService
    {
        public int RegisterCalls { get; private set; }
        public RegisterCustomerCommand? LastRegisterCommand { get; private set; }

        public Result<CustomerDto> RegisterResult { get; set; } = Result.Success(new CustomerDto
        {
            Id = Guid.NewGuid(),
            Name = "Cliente Demo",
            Email = "cliente@plataforma.com",
            Role = RolUsuario.Cliente,
            IsActive = true,
            IsEmailConfirmed = false,
            CreatedAtUtc = DateTime.UtcNow
        });

        public Result<UserDto> GetUserByEmailResult { get; set; } = Result.Failure<UserDto>(
            Error.NotFound("Users.NotFoundByEmail", "No se encontró un usuario con el correo 'nuevo@plataforma.com'."));

        public bool ThrowOperationCanceledOnGetUserByEmail { get; set; }
        public bool ThrowTaskCanceledOnGetUserByEmail { get; set; }

        public Task<Result<CustomerDto>> RegisterCustomerAsync(RegisterCustomerCommand command, CancellationToken cancellationToken = default)
        {
            RegisterCalls++;
            LastRegisterCommand = command;
            return Task.FromResult(RegisterResult);
        }

        public Task<Result<UserDto>> UpdateUserBasicDataAsync(UpdateUserBasicDataCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> ConfirmUserEmailAsync(ConfirmUserEmailCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> ActivateUserAsync(ActivateUserCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> DeactivateUserAsync(DeactivateUserCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> GetUserByIdAsync(GetUserByIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> GetUserByEmailAsync(GetUserByEmailQuery query, CancellationToken cancellationToken = default)
        {
            if (ThrowOperationCanceledOnGetUserByEmail || cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            if (ThrowTaskCanceledOnGetUserByEmail)
            {
                throw new TaskCanceledException("Infraestructura no disponible.");
            }

            return Task.FromResult(GetUserByEmailResult);
        }
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
