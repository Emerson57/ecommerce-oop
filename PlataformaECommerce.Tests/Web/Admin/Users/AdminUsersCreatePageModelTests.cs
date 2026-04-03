using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Admin;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Pages.Admin.Users;

namespace PlataformaECommerce.Tests.Web.Admin.Users;

[TestFixture]
public class AdminUsersCreatePageModelTests
{
    [Test]
    public async Task OnGetAsync_ConsultaExitosa_CargaDefinicionYValoresPorDefecto()
    {
        CreateModel pageModel = CreatePageModel(new FakeAdminUserService(), enableAdministratorCreationUi: true);

        IActionResult result = await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.Definition.AllowedRole, Is.EqualTo(RolUsuario.Administrador));
        Assert.That(pageModel.Input.Area, Is.EqualTo("Operaciones"));
        Assert.That(pageModel.Input.IsActive, Is.True);
        Assert.That(pageModel.Input.IsEmailConfirmed, Is.False);
    }

    [Test]
    public async Task OnPostAsync_FormularioValido_RegistraAdministradorYRedirigeAlListado()
    {
        FakeAdminUserService service = new();
        CreateModel pageModel = CreatePageModel(service, enableAdministratorCreationUi: true);
        pageModel.Input = new CreateModel.InputModel
        {
            Name = "Admin Nuevo",
            Email = "admin.nuevo@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            Area = "Operaciones",
            IsActive = true,
            IsEmailConfirmed = false,
            Reason = "Alta desde backoffice."
        };

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<RedirectToPageResult>());
        Assert.That(service.LastRegisterCommand?.Email, Is.EqualTo("admin.nuevo@plataforma.com"));
        Assert.That(pageModel.StatusMessage, Is.EqualTo("El administrador 'Admin Nuevo' fue creado correctamente con el correo 'admin.nuevo@plataforma.com'."));
    }

    [Test]
    public async Task OnPostAsync_FormularioValido_EnviaMetadataDeTrazabilidad()
    {
        FakeAdminUserService service = new();
        Guid requestedByUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        CreateModel pageModel = CreatePageModel(service, enableAdministratorCreationUi: true, requestedByUserId: requestedByUserId, remoteIpAddress: "10.20.30.40");
        pageModel.Input = new CreateModel.InputModel
        {
            Name = "Admin Nuevo",
            Email = "admin.nuevo@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            Area = "Operaciones",
            IsActive = true,
            IsEmailConfirmed = false
        };

        await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(service.LastRegisterCommand?.RequestedByUserId, Is.EqualTo(requestedByUserId));
        Assert.That(service.LastRegisterCommand?.IpAddress, Is.EqualTo("10.20.30.40"));
        Assert.That(service.LastRegisterCommand?.Source, Is.EqualTo("AdminPortal"));
    }

    [Test]
    public async Task OnPostAsync_FalloAplicacion_PublicaErrorFuncional()
    {
        CreateModel pageModel = CreatePageModel(new FakeAdminUserService
        {
            RegisterResult = Result.Failure<AdminDto>(Error.Conflict("Admin.EmailAlreadyExists", "Ya existe un usuario registrado con el correo 'admin.nuevo@plataforma.com'."))
        }, enableAdministratorCreationUi: true);
        pageModel.Input = new CreateModel.InputModel
        {
            Name = "Admin Nuevo",
            Email = "admin.nuevo@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            Area = "Operaciones",
            IsActive = true,
            IsEmailConfirmed = false
        };

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.ErrorMessage, Does.Contain("admin.nuevo@plataforma.com"));
    }

    [Test]
    public async Task OnPostAsync_ModeloInvalido_NoInvocaElCasoDeUso()
    {
        FakeAdminUserService service = new();
        CreateModel pageModel = CreatePageModel(service, enableAdministratorCreationUi: true);
        pageModel.ModelState.AddModelError(nameof(CreateModel.InputModel.Email), "Email inválido.");

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(service.RegisterCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task OnPostAsync_FuncionalidadDeshabilitada_RetornaNotFound()
    {
        FakeAdminUserService service = new();
        CreateModel pageModel = CreatePageModel(service, enableAdministratorCreationUi: false);
        pageModel.Input = new CreateModel.InputModel
        {
            Name = "Admin Nuevo",
            Email = "admin.nuevo@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            Area = "Operaciones"
        };

        IActionResult result = await pageModel.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
        Assert.That(service.RegisterCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task OnGetAsync_ConsultaFallida_PublicaErrorFuncional()
    {
        CreateModel pageModel = CreatePageModel(new FakeAdminUserService
        {
            DefinitionResult = Result.Failure<AdminRegistrationDefinitionDto>(Error.Unauthorized("Admin.SuperUserRequiredForAdminCreationDefinition", "Solo un super usuario puede consultar la definición funcional de creación de administradores."))
        }, enableAdministratorCreationUi: true);

        IActionResult result = await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.ErrorMessage, Is.EqualTo("Solo un super usuario puede consultar la definición funcional de creación de administradores."));
    }

    [Test]
    public async Task OnGetAsync_FuncionalidadDeshabilitada_RetornaNotFound()
    {
        CreateModel pageModel = CreatePageModel(new FakeAdminUserService(), enableAdministratorCreationUi: false);

        IActionResult result = await pageModel.OnGetAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    private static CreateModel CreatePageModel(
        FakeAdminUserService adminApplicationService,
        bool enableAdministratorCreationUi,
        Guid? requestedByUserId = null,
        string? remoteIpAddress = null)
    {
        DefaultHttpContext httpContext = new();

        if (requestedByUserId.HasValue)
        {
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, requestedByUserId.Value.ToString()),
                new Claim(ClaimTypes.Name, "Root Demo")
            ],
            authenticationType: "TestAuth"));
        }

        if (!string.IsNullOrWhiteSpace(remoteIpAddress))
        {
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(remoteIpAddress);
        }

        CreateModel pageModel = new(
            adminApplicationService,
            Options.Create(new AdminUsersBackofficeOptions
            {
                EnableAdministratorCreationUi = enableAdministratorCreationUi
            }))
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };

        return pageModel;
    }

    private sealed class FakeAdminUserService : IAdminUserService
    {
        public int RegisterCalls { get; private set; }

        public Result<AdminRegistrationDefinitionDto> DefinitionResult { get; set; } = PlataformaECommerce.Application.Common.Results.Result.Success(new AdminRegistrationDefinitionDto
        {
            GeneratedAtUtc = DateTime.UtcNow,
            AllowedRole = RolUsuario.Administrador,
            DefaultArea = "Operaciones",
            DefaultIsActive = true,
            DefaultIsEmailConfirmed = false,
            RequiredFields = ["Name", "Email", "Password", "ConfirmPassword", "Area"]
        });

        public Result<AdminDto> RegisterResult { get; set; } = PlataformaECommerce.Application.Common.Results.Result.Success(new AdminDto
        {
            Id = Guid.NewGuid(),
            Name = "Admin Nuevo",
            Email = "admin.nuevo@plataforma.com",
            Role = RolUsuario.Administrador,
            Area = "Operaciones",
            IsActive = true,
            IsEmailConfirmed = false,
            CreatedAtUtc = DateTime.UtcNow
        });

        public RegisterAdminCommand? LastRegisterCommand { get; private set; }

        public Task<Result<AdminDto>> RegisterAdminAsync(RegisterAdminCommand command, CancellationToken cancellationToken = default)
        {
            RegisterCalls++;
            LastRegisterCommand = command;
            return Task.FromResult(RegisterResult);
        }

        public Task<Result<AdminRegistrationDefinitionDto>> GetAdminRegistrationDefinitionAsync(GetAdminRegistrationDefinitionQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(DefinitionResult);
        }

        public Task<Result<AdminUsersBackofficeDto>> GetUsersAsync(GetAdminUsersQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<AdminBackofficeUserDto>> ResetUserPasswordAsync(ResetUserPasswordCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
