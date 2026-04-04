using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Users.Commands;
using PlataformaECommerce.Application.Features.Users.DTOs;
using PlataformaECommerce.Application.Features.Users.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Pages.Auth;

namespace PlataformaECommerce.Tests.Web.Auth;

[TestFixture]
public class ConfirmEmailPageModelTests
{
    [Test]
    public async Task OnGetAsync_TokenValido_ConfirmaCuentaYRetornaPagina()
    {
        FakeUserApplicationService userApplicationService = new();
        ConfirmEmailModel pageModel = CreatePageModel(userApplicationService);
        Guid userId = Guid.NewGuid();

        IActionResult result = await pageModel.OnGetAsync(userId, "token-seguro", CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.IsSuccess, Is.True);
        Assert.That(userApplicationService.LastConfirmCommand?.UserId, Is.EqualTo(userId));
    }

    [Test]
    public async Task OnGetAsync_TokenInvalido_PublicaErrorControlado()
    {
        FakeUserApplicationService userApplicationService = new()
        {
            ConfirmResult = Result.Failure<UserDto>(Error.Unauthorized("Users.InvalidEmailConfirmationToken", "El enlace de confirmación no es válido o ya expiró."))
        };
        ConfirmEmailModel pageModel = CreatePageModel(userApplicationService);

        IActionResult result = await pageModel.OnGetAsync(Guid.NewGuid(), "token-invalido", CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(pageModel.IsSuccess, Is.False);
        Assert.That(pageModel.StatusMessage, Does.Contain("no es válido"));
    }

    private static ConfirmEmailModel CreatePageModel(FakeUserApplicationService userApplicationService)
    {
        DefaultHttpContext httpContext = new();
        return new ConfirmEmailModel(userApplicationService)
        {
            PageContext = new PageContext
            {
                HttpContext = httpContext
            }
        };
    }

    private sealed class FakeUserApplicationService : IUserApplicationService
    {
        public ConfirmUserEmailCommand? LastConfirmCommand { get; private set; }

        public Result<UserDto> ConfirmResult { get; set; } = Result.Success(new UserDto
        {
            Id = Guid.NewGuid(),
            Name = "Cliente Demo",
            Email = "cliente@plataforma.com",
            Role = RolUsuario.Cliente,
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        public Task<Result<CustomerDto>> RegisterCustomerAsync(RegisterCustomerCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> UpdateUserBasicDataAsync(UpdateUserBasicDataCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> ConfirmUserEmailAsync(ConfirmUserEmailCommand command, CancellationToken cancellationToken = default)
        {
            LastConfirmCommand = command;
            return Task.FromResult(ConfirmResult);
        }

        public Task<Result> ResendUserEmailConfirmationAsync(ResendUserEmailConfirmationCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> ActivateUserAsync(ActivateUserCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> DeactivateUserAsync(DeactivateUserCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> GetUserByIdAsync(GetUserByIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<UserDto>> GetUserByEmailAsync(GetUserByEmailQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
