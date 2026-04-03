using FluentValidation;
using PlataformaECommerce.Application.Common.Execution;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Application.Features.Admin.Services;

internal static class AdminServiceSupport
{
    internal static Email CreateEmail(string value)
    {
        return new Email(value);
    }

    internal static AdminBackofficeUserDto MapToBackofficeUser(Usuario user)
    {
        ArgumentNullException.ThrowIfNull(user);

        Administrador? administrativeUser = user as Administrador;

        return new AdminBackofficeUserDto
        {
            Id = user.Id,
            Name = user.Nombre,
            Email = user.CorreoElectronico.Value,
            Role = user.Rol,
            IsAdministrative = administrativeUser is not null,
            IsSuperUser = administrativeUser?.EsSuperUsuario == true,
            IsActive = user.Activo,
            IsEmailConfirmed = user.CorreoConfirmado,
            IsEnabled = user.EstaHabilitado(),
            Area = administrativeUser?.Area,
            CreatedAtUtc = user.FechaCreacionUtc,
            UpdatedAtUtc = user.FechaActualizacionUtc,
            LastAccessAtUtc = user.FechaUltimoAccesoUtc
        };
    }

    internal static Task<Error?> ValidateAsync<TCommand>(
        TCommand command,
        IValidator<TCommand> validator,
        string errorCode,
        string message,
        CancellationToken cancellationToken)
    {
        return ApplicationExecution.ValidateAsync(command, validator, errorCode, message, cancellationToken);
    }

    internal static Task<Result<TResponse>> ExecuteAsync<TResponse>(
        Func<Task<Result<TResponse>>> operation,
        string errorCode)
    {
        return ApplicationExecution.ExecuteAsync(operation, errorCode);
    }
}
