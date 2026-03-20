using FluentValidation;
using PlataformaECommerce.Application.Common.Execution;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Auth.Commands;
using PlataformaECommerce.Application.Features.Auth.DTOs;
using PlataformaECommerce.Application.Features.Auth.Queries;
using PlataformaECommerce.Application.Features.Auth.Validators;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Application.Features.Auth.Services;

/// <summary>
/// Proporciona los casos de uso de aplicación relacionados con autenticación
/// y consulta del usuario autenticado dentro del sistema.
/// </summary>
/// <remarks>
/// Este servicio coordina el proceso de autenticación y la construcción
/// de respuestas desacopladas para la capa superior, manteniendo separadas
/// las responsabilidades de:
/// - validación estructural,
/// - acceso a usuarios mediante repositorio,
/// - verificación segura de contraseñas,
/// - emisión de tokens,
/// - actualización de trazabilidad de acceso,
/// - y proyección hacia DTOs de autenticación.
///
/// La clase se apoya en contratos definidos en la capa Application,
/// permitiendo que los detalles técnicos de hashing y tokenización
/// permanezcan encapsulados en Infrastructure.
/// </remarks>
public sealed class AuthApplicationService : IAuthApplicationService
{
    #region Campos privados

    /// <summary>
    /// Repositorio de usuarios del sistema.
    /// </summary>
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Servicio responsable de verificar y generar hashes de contraseñas.
    /// </summary>
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// Servicio responsable de emitir tokens de autenticación.
    /// </summary>
    private readonly ITokenService _tokenService;

    /// <summary>
    /// Unidad de trabajo responsable de confirmar de forma transaccional
    /// los cambios realizados durante el proceso de autenticación.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    private readonly IValidator<LoginCommand> _loginCommandValidator;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="AuthApplicationService"/>.
    /// </summary>
    /// <param name="userRepository">Repositorio de usuarios.</param>
    /// <param name="passwordHasher">Servicio de hashing de contraseñas.</param>
    /// <param name="tokenService">Servicio de tokens.</param>
    /// <param name="unitOfWork">Unidad de trabajo para confirmar los cambios del proceso de autenticación.</param>
    public AuthApplicationService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IValidator<LoginCommand> loginCommandValidator)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _loginCommandValidator = loginCommandValidator ?? throw new ArgumentNullException(nameof(loginCommandValidator));
    }

    #endregion

    #region Casos de uso públicos

    /// <summary>
    /// Ejecuta el proceso de autenticación de un usuario.
    /// </summary>
    /// <param name="command">Comando de autenticación.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la respuesta de autenticación cuando el proceso es exitoso.
    /// </returns>
    public async Task<Result<AuthResponseDto>> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ValidateAsync(command, _loginCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<AuthResponseDto>(validationError);
        }

        return await ExecuteAsync(async () =>
        {
            string emailAddress = command.Email.Trim();

            Usuario? user = await FindUserByEmailAsync(emailAddress, cancellationToken);

            if (user is null)
            {
                return Result.Failure<AuthResponseDto>(
                    Error.Unauthorized("Auth.InvalidCredentials", "Las credenciales suministradas no son válidas."));
            }

            if (!user.Activo)
            {
                return Result.Failure<AuthResponseDto>(
                    Error.Unauthorized("Auth.UserInactive", "La cuenta del usuario se encuentra inactiva."));
            }

            if (!_passwordHasher.VerifyPassword(command.Password, user.ContrasenaHash))
            {
                return Result.Failure<AuthResponseDto>(
                    Error.Unauthorized("Auth.InvalidCredentials", "Las credenciales suministradas no son válidas."));
            }

            user.RegistrarAcceso();
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            string accessToken = _tokenService.GenerateAccessToken(user);
            string refreshToken = _tokenService.GenerateRefreshToken(user);
            DateTime expiresAtUtc = _tokenService.GetAccessTokenExpirationUtc(accessToken);
            int expiresInSeconds = Convert.ToInt32(Math.Max(0, (expiresAtUtc - DateTime.UtcNow).TotalSeconds));

            AuthResponseDto response = new()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresAtUtc = expiresAtUtc,
                ExpiresInSeconds = expiresInSeconds,
                User = MapToCurrentUserDto(user),
                RequiresPasswordChange = false,
                IsPersistentSession = command.RememberMe,
                IssuedAtUtc = DateTime.UtcNow,
                ExternalReference = command.ExternalReference
            };

            return Result.Success(response);
        }, "Auth.Domain");
    }

    /// <summary>
    /// Obtiene la información del usuario autenticado actual.
    /// </summary>
    /// <param name="query">Consulta del usuario actual.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la información del usuario autenticado cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<CurrentUserDto>> GetCurrentUserAsync(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.AuthenticatedUserId == Guid.Empty)
        {
            return Result.Failure<CurrentUserDto>(
                Error.Validation("Auth.InvalidUserId", "El identificador del usuario autenticado es obligatorio."));
        }

        Usuario? user = await _userRepository.GetByIdAsync(query.AuthenticatedUserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<CurrentUserDto>(
                Error.NotFound("Auth.UserNotFound", $"No se encontró un usuario con identificador '{query.AuthenticatedUserId}'."));
        }

        CurrentUserDto dto = MapToCurrentUserDto(user);

        return Result.Success(dto);
    }

    #endregion

    #region Métodos privados auxiliares

    /// <summary>
    /// Busca un usuario por su correo electrónico.
    /// </summary>
    /// <param name="emailAddress">Correo electrónico del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Usuario encontrado o <see langword="null"/>.</returns>
    private async Task<Usuario?> FindUserByEmailAsync(
        string emailAddress,
        CancellationToken cancellationToken)
    {
        try
        {
            Email email = new(emailAddress);
            return await _userRepository.GetByEmailAsync(email, cancellationToken);
        }
        catch (DomainException)
        {
            return null;
        }
    }

    private static Task<Error?> ValidateAsync<TCommand>(
        TCommand command,
        IValidator<TCommand> validator,
        CancellationToken cancellationToken)
    {
        return ApplicationExecution.ValidateAsync(
            command,
            validator,
            "Auth.Validation",
            "La solicitud de autenticación contiene errores de validación.",
            cancellationToken);
    }

    private static Task<Result<TResponse>> ExecuteAsync<TResponse>(
        Func<Task<Result<TResponse>>> operation,
        string errorCode)
    {
        return ApplicationExecution.ExecuteAsync(operation, errorCode);
    }

    /// <summary>
    /// Proyecta una entidad <see cref="Usuario"/> hacia un <see cref="CurrentUserDto"/>.
    /// </summary>
    /// <param name="user">Usuario de origen.</param>
    /// <returns>DTO del usuario autenticado.</returns>
    private static CurrentUserDto MapToCurrentUserDto(Usuario user)
    {
        ArgumentNullException.ThrowIfNull(user);

        string role = user.Rol.ToString();

        return new CurrentUserDto
        {
            Id = user.Id,
            UserName = user.CorreoElectronico.Value,
            Email = user.CorreoElectronico.Value,
            FullName = user.Nombre,
            FirstName = null,
            LastName = null,
            ProfileImageUrl = null,
            IsActive = user.Activo,
            IsEmailConfirmed = user.CorreoConfirmado,
            IsTwoFactorEnabled = false,
            Role = role,
            Roles = new[] { role },
            Permissions = Array.Empty<string>(),
            CreatedAtUtc = user.FechaCreacionUtc,
            LastLoginAtUtc = user.FechaUltimoAccesoUtc
        };
    }

    #endregion
}