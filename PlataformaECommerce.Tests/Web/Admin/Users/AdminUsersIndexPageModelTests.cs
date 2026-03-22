using Microsoft.AspNetCore.Http;
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
public class AdminUsersIndexPageModelTests
{
    [Test]
    public async Task OnGetAsync_ConsultaExitosa_CargaResumenYUsuariosSeparados()
    {
        IndexModel pageModel = CreatePageModel(new FakeAdminApplicationService());

        await pageModel.OnGetAsync(null, CancellationToken.None);

        Assert.That(pageModel.UsersBackoffice.TotalUsers, Is.EqualTo(3));
        Assert.That(pageModel.VisibleUsers.Count, Is.EqualTo(3));
        Assert.That(pageModel.AdministrativeUsers.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task OnGetAsync_ModuloAdministrativo_SolicitaTodosLosUsuariosParaOperacionesSensibles()
    {
        FakeAdminApplicationService service = new();
        IndexModel pageModel = CreatePageModel(service);

        await pageModel.OnGetAsync(null, CancellationToken.None);

        Assert.That(service.LastQuery?.OnlyAdministrativeUsers, Is.False);
    }

    [Test]
    public async Task OnGetAsync_ConsultaExitosa_EnviaMetadataDelActorActual()
    {
        FakeAdminApplicationService service = new();
        Guid requestedByUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        IndexModel pageModel = CreatePageModel(service, requestedByUserId);

        await pageModel.OnGetAsync(null, CancellationToken.None);

        Assert.That(service.LastQuery?.RequestedByUserId, Is.EqualTo(requestedByUserId));
        Assert.That(service.LastQuery?.Source, Is.EqualTo("AdminPortal"));
    }

    [Test]
    public async Task OnGetAsync_UsuarioSeleccionado_CargaElObjetivoDelRestablecimiento()
    {
        FakeAdminApplicationService service = new();
        IndexModel pageModel = CreatePageModel(service);
        Guid selectedUserId = service.Result.Value.Users.Last().Id;

        await pageModel.OnGetAsync(selectedUserId, CancellationToken.None);

        Assert.That(pageModel.SelectedUser?.Id, Is.EqualTo(selectedUserId));
        Assert.That(pageModel.ResetPassword.TargetUserId, Is.EqualTo(selectedUserId));
    }

    [Test]
    public async Task OnGetAsync_ConsultaFallida_PublicaErrorFuncional()
    {
        IndexModel pageModel = CreatePageModel(new FakeAdminApplicationService
        {
            Result = Result.Failure<AdminUsersBackofficeDto>(Error.Unauthorized("Admin.SuperUserRequiredForUsersBackoffice", "Solo un super usuario puede consultar el backoffice de usuarios."))
        });

        await pageModel.OnGetAsync(null, CancellationToken.None);

        Assert.That(pageModel.ErrorMessage, Is.EqualTo("Solo un super usuario puede consultar el backoffice de usuarios."));
    }

    [Test]
    public async Task OnPostResetPasswordAsync_FormularioValido_InvocaCasoDeUsoYRedirige()
    {
        FakeAdminApplicationService service = new();
        Guid requestedByUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        Guid selectedUserId = service.Result.Value.Users.Last().Id;
        IndexModel pageModel = CreatePageModel(service, requestedByUserId, "10.20.30.40");
        pageModel.ResetPassword = new IndexModel.ResetPasswordInputModel
        {
            TargetUserId = selectedUserId,
            NewPassword = "Password#2027",
            ConfirmPassword = "Password#2027",
            Reason = "Mesa de ayuda"
        };

        var result = await pageModel.OnPostResetPasswordAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<Microsoft.AspNetCore.Mvc.RedirectToPageResult>());
        Assert.That(service.LastResetPasswordCommand?.TargetUserId, Is.EqualTo(selectedUserId));
        Assert.That(service.LastResetPasswordCommand?.RequestedByUserId, Is.EqualTo(requestedByUserId));
        Assert.That(service.LastResetPasswordCommand?.IpAddress, Is.EqualTo("10.20.30.40"));
        Assert.That(pageModel.StatusMessage, Does.Contain("fue restablecida correctamente"));
    }

    [Test]
    public async Task OnPostResetPasswordAsync_FalloAplicacion_PublicaErrorFuncionalYMantieneUsuarioSeleccionado()
    {
        FakeAdminApplicationService service = new()
        {
            ResetPasswordResult = Result.Failure<AdminBackofficeUserDto>(Error.NotFound("Admin.UserNotFound", "No se encontró el usuario solicitado."))
        };
        Guid selectedUserId = service.Result.Value.Users.Last().Id;
        IndexModel pageModel = CreatePageModel(service);
        pageModel.ResetPassword = new IndexModel.ResetPasswordInputModel
        {
            TargetUserId = selectedUserId,
            NewPassword = "Password#2027",
            ConfirmPassword = "Password#2027"
        };

        var result = await pageModel.OnPostResetPasswordAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.ErrorMessage, Is.EqualTo("No se encontró el usuario solicitado."));
        Assert.That(pageModel.SelectedUser?.Id, Is.EqualTo(selectedUserId));
    }

    private static IndexModel CreatePageModel(FakeAdminApplicationService adminApplicationService, Guid? requestedByUserId = null, string? remoteIpAddress = null)
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

        IndexModel pageModel = new(
            adminApplicationService,
            Options.Create(new AdminUsersBackofficeOptions
            {
                EnableAdministratorCreationUi = false
            }))
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };

        return pageModel;
    }

    private sealed class FakeAdminApplicationService : IAdminApplicationService
    {
        public Result<AdminUsersBackofficeDto> Result { get; set; } = PlataformaECommerce.Application.Common.Results.Result.Success(new AdminUsersBackofficeDto
        {
            GeneratedAtUtc = DateTime.UtcNow,
            RecentAccessWindowStartUtc = DateTime.UtcNow.AddDays(-30),
            TotalUsers = 3,
            ActiveUsers = 3,
            EmailConfirmedUsers = 3,
            EnabledUsers = 3,
            TotalCustomers = 1,
            TotalAdministrators = 2,
            TotalSuperUsers = 1,
            Users =
            [
                new AdminBackofficeUserDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Root Demo",
                    Email = "root@plataforma.com",
                    Role = RolUsuario.SuperUsuario,
                    IsAdministrative = true,
                    IsSuperUser = true,
                    IsActive = true,
                    IsEmailConfirmed = true,
                    IsEnabled = true,
                    Area = "Plataforma",
                    CreatedAtUtc = DateTime.UtcNow,
                    LastAccessAtUtc = DateTime.UtcNow
                },
                new AdminBackofficeUserDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin Demo",
                    Email = "admin@plataforma.com",
                    Role = RolUsuario.Administrador,
                    IsAdministrative = true,
                    IsSuperUser = false,
                    IsActive = true,
                    IsEmailConfirmed = true,
                    IsEnabled = true,
                    Area = "Operaciones",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new AdminBackofficeUserDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Cliente Demo",
                    Email = "cliente@plataforma.com",
                    Role = RolUsuario.Cliente,
                    IsAdministrative = false,
                    IsSuperUser = false,
                    IsActive = true,
                    IsEmailConfirmed = true,
                    IsEnabled = true,
                    CreatedAtUtc = DateTime.UtcNow
                }
            ]
        });

        public Result<AdminBackofficeUserDto> ResetPasswordResult { get; set; } = PlataformaECommerce.Application.Common.Results.Result.Success(new AdminBackofficeUserDto
        {
            Id = Guid.NewGuid(),
            Name = "Cliente Demo",
            Email = "cliente@plataforma.com",
            Role = RolUsuario.Cliente,
            IsAdministrative = false,
            IsSuperUser = false,
            IsActive = true,
            IsEmailConfirmed = true,
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        public GetAdminUsersQuery? LastQuery { get; private set; }
        public ResetUserPasswordCommand? LastResetPasswordCommand { get; private set; }

        public Task<Result<AdminDto>> RegisterAdminAsync(RegisterAdminCommand command, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<AdminRegistrationDefinitionDto>> GetAdminRegistrationDefinitionAsync(GetAdminRegistrationDefinitionQuery query, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<AdminDashboardDto>> GetDashboardAsync(GetAdminDashboardQuery query, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Result<AdminUsersBackofficeDto>> GetUsersAsync(GetAdminUsersQuery query, CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            return Task.FromResult(Result);
        }

        public Task<Result<AdminBackofficeUserDto>> ResetUserPasswordAsync(ResetUserPasswordCommand command, CancellationToken cancellationToken = default)
        {
            LastResetPasswordCommand = command;
            return Task.FromResult(ResetPasswordResult);
        }
    }
}
