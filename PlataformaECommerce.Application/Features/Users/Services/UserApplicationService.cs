using FluentValidation;
using PlataformaECommerce.Application.Common.Execution;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Users.Commands;
using PlataformaECommerce.Application.Features.Users.DTOs;
using PlataformaECommerce.Application.Features.Users.Queries;
using PlataformaECommerce.Application.Features.Users.Validators;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Application.Interfaces.Services.Users;
using PlataformaECommerce.Application.Features.Users.Mappings;
using PlataformaECommerce.Application.Common.Notifications;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Application.Features.Users.Services;

/// <summary>
/// Proporciona los casos de uso de aplicación relacionados con la gestión de usuarios.
/// </summary>
/// <remarks>
/// Esta clase coordina la ejecución de operaciones de lectura y escritura
/// sobre el agregado de usuarios, actuando como servicio de aplicación.
///
/// Su responsabilidad incluye:
/// - validación de comandos y consultas,
/// - coordinación con repositorios,
/// - control de persistencia mediante unidad de trabajo,
/// - transformación de datos hacia DTOs,
/// - aplicación de servicios transversales como hashing de contraseñas,
/// - y orquestación de acciones de negocio sin invadir el dominio.
///
/// Este servicio constituye la implementación pública de los casos de uso del
/// módulo de usuarios, utilizando comandos y consultas como modelos de entrada
/// para mantener una frontera clara y estable dentro de <c>Application</c>.
/// </remarks>
public sealed class UserApplicationService : IUserApplicationService
{
    private static readonly TimeSpan EmailConfirmationTokenLifetime = TimeSpan.FromHours(24);

    #region Campos privados

    /// <summary>
    /// Repositorio de usuarios.
    /// </summary>
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Unidad de trabajo asociada a la persistencia.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Servicio de hashing y verificación de contraseñas.
    /// </summary>
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// Servicio transversal de auditoría.
    /// </summary>
    private readonly IAuditTrailService _auditTrailService;
    private readonly IEmailConfirmationTokenService _emailConfirmationTokenService;
    private readonly IEmailNotificationService _emailNotificationService;

    private readonly IValidator<RegisterCustomerCommand> _registerCustomerCommandValidator;
    private readonly IValidator<UpdateUserBasicDataCommand> _updateUserBasicDataCommandValidator;
    private readonly IValidator<ResendUserEmailConfirmationCommand> _resendUserEmailConfirmationCommandValidator;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="UserApplicationService"/>.
    /// </summary>
    /// <param name="userRepository">Repositorio de usuarios.</param>
    /// <param name="unitOfWork">Unidad de trabajo.</param>
    /// <param name="passwordHasher">Servicio de hashing de contraseñas.</param>
    /// <param name="auditTrailService">Servicio transversal de auditoría.</param>
    public UserApplicationService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IAuditTrailService auditTrailService,
        IEmailConfirmationTokenService emailConfirmationTokenService,
        IEmailNotificationService emailNotificationService,
        IValidator<RegisterCustomerCommand> registerCustomerCommandValidator,
        IValidator<UpdateUserBasicDataCommand> updateUserBasicDataCommandValidator,
        IValidator<ResendUserEmailConfirmationCommand> resendUserEmailConfirmationCommandValidator)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _auditTrailService = auditTrailService ?? throw new ArgumentNullException(nameof(auditTrailService));
        _emailConfirmationTokenService = emailConfirmationTokenService ?? throw new ArgumentNullException(nameof(emailConfirmationTokenService));
        _emailNotificationService = emailNotificationService ?? throw new ArgumentNullException(nameof(emailNotificationService));
        _registerCustomerCommandValidator = registerCustomerCommandValidator ?? throw new ArgumentNullException(nameof(registerCustomerCommandValidator));
        _updateUserBasicDataCommandValidator = updateUserBasicDataCommandValidator ?? throw new ArgumentNullException(nameof(updateUserBasicDataCommandValidator));
        _resendUserEmailConfirmationCommandValidator = resendUserEmailConfirmationCommandValidator ?? throw new ArgumentNullException(nameof(resendUserEmailConfirmationCommandValidator));
    }

    #endregion

    #region Casos de uso de registro

    /// <summary>
    /// Registra un nuevo cliente dentro del sistema.
    /// </summary>
    /// <param name="command">Comando de registro del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación del cliente registrado cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<CustomerDto>> RegisterCustomerAsync(
        RegisterCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ValidateAsync(command, _registerCustomerCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<CustomerDto>(validationError);
        }

        return await ExecuteAsync(async () =>
        {
            Email email = CreateEmail(command.Email);

            bool emailExists = await _userRepository.ExistsByEmailAsync(email, cancellationToken);
            if (emailExists)
            {
                return Result.Failure<CustomerDto>(
                    Error.Conflict("Users.EmailAlreadyExists", $"Ya existe un usuario registrado con el correo '{command.Email}'."));
            }

            string passwordHash = _passwordHasher.HashPassword(command.Password);

            Cliente customer = new(
                command.Name,
                email,
                passwordHash);

            if (string.IsNullOrWhiteSpace(command.EmailConfirmationUrl))
            {
                return Result.Failure<CustomerDto>(
                    Error.Validation("Users.EmailConfirmationUrlRequired", "La URL de confirmación de correo es obligatoria para completar el registro."));
            }

            foreach (string preference in command.Preferences
                         .Where(value => !string.IsNullOrWhiteSpace(value))
                         .Select(value => value.Trim())
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                customer.AgregarPreferencia(preference);
            }

            await _userRepository.AddAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditUserEventAsync(
                customer,
                "Users",
                "user.customer.registered",
                $"Se registró un nuevo cliente con correo '{customer.CorreoElectronico.Value}'.",
                new Dictionary<string, string>
                {
                    ["role"] = customer.Rol.ToString(),
                    ["email"] = customer.CorreoElectronico.Value,
                    ["preferencesCount"] = customer.Preferencias.Count.ToString()
                },
                cancellationToken);

            Result emailResult = await SendEmailConfirmationNotificationAsync(customer, command.EmailConfirmationUrl, cancellationToken);

            return Result.Success(customer.ToCustomerDto());
        }, "Users.Domain");
    }

    #endregion

    #region Casos de uso de actualización

    /// <summary>
    /// Actualiza la información básica de un usuario existente.
    /// </summary>
    /// <param name="command">Comando de actualización de datos básicos.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación actualizada del usuario cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<UserDto>> UpdateUserBasicDataAsync(
        UpdateUserBasicDataCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ValidateAsync(command, _updateUserBasicDataCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<UserDto>(validationError);
        }

        return await ExecuteAsync(async () =>
        {
            Usuario? user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
            if (user is null)
            {
                return Result.Failure<UserDto>(
                    Error.NotFound("Users.NotFound", $"No se encontró un usuario con identificador '{command.UserId}'."));
            }

            Email email = CreateEmail(command.Email);

            Usuario? userWithSameEmail = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (userWithSameEmail is not null && userWithSameEmail.Id != user.Id)
            {
                return Result.Failure<UserDto>(
                    Error.Conflict("Users.EmailAlreadyExists", $"Ya existe un usuario registrado con el correo '{command.Email}'."));
            }

            user.ActualizarDatosBasicos(command.Name, email);

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditUserEventAsync(
                user,
                "Users",
                "user.basic-data.updated",
                $"Se actualizó la información básica del usuario con correo '{user.CorreoElectronico.Value}'.",
                new Dictionary<string, string>
                {
                    ["email"] = user.CorreoElectronico.Value,
                    ["role"] = user.Rol.ToString()
                },
                cancellationToken);

            return Result.Success(user.ToUserDto());
        }, "Users.Domain");
    }

    /// <summary>
    /// Reenvía el correo de confirmación para una cuenta no confirmada.
    /// </summary>
    /// <param name="command">Comando de reenvío de confirmación.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado de la operación.</returns>
    public async Task<Result> ResendUserEmailConfirmationAsync(
        ResendUserEmailConfirmationCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ValidateAsync(command, _resendUserEmailConfirmationCommandValidator, cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure(validationError);
        }

        return await ExecuteAsync(async () =>
        {
            Email email = CreateEmail(command.Email);
            Usuario? user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            if (user is null)
            {
                return Result.Success();
            }

            if (user.CorreoConfirmado)
            {
                return Result.Success();
            }

            return await SendEmailConfirmationNotificationAsync(user, command.EmailConfirmationUrl, cancellationToken);
        }, "Users.Domain");
    }

    /// <summary>
    /// Confirma el correo electrónico de un usuario existente.
    /// </summary>
    /// <param name="command">Comando de confirmación de correo.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación actualizada del usuario cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<UserDto>> ConfirmUserEmailAsync(
        ConfirmUserEmailCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.UserId == Guid.Empty)
        {
            return Result.Failure<UserDto>(
                Error.Validation("Users.InvalidId", "El identificador del usuario es obligatorio."));
        }

        if (string.IsNullOrWhiteSpace(command.ConfirmationToken) &&
            string.IsNullOrWhiteSpace(command.ConfirmationCode))
        {
            return Result.Failure<UserDto>(
                Error.Validation("Users.ConfirmationRequired", "Debe suministrarse un token o código de confirmación."));
        }

        return await ExecuteAsync(async () =>
        {
            Usuario? user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
            if (user is null)
            {
                return Result.Failure<UserDto>(
                    Error.NotFound("Users.NotFound", $"No se encontró un usuario con identificador '{command.UserId}'."));
            }

            EmailConfirmationTokenValidationDto? tokenData = _emailConfirmationTokenService.ValidateToken(command.ConfirmationToken);
            if (!IsEmailConfirmationTokenValid(user, tokenData))
            {
                return Result.Failure<UserDto>(
                    Error.Unauthorized("Users.InvalidEmailConfirmationToken", "El enlace de confirmación no es válido o ya expiró."));
            }

            user.ConfirmarCorreoElectronico();

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditUserEventAsync(
                user,
                "Users",
                "user.email.confirmed",
                $"Se confirmó el correo electrónico del usuario '{user.CorreoElectronico.Value}'.",
                new Dictionary<string, string>
                {
                    ["emailConfirmed"] = user.CorreoConfirmado.ToString(),
                    ["role"] = user.Rol.ToString()
                },
                cancellationToken);

            return Result.Success(user.ToUserDto());
        }, "Users.Domain");
    }

    /// <summary>
    /// Activa un usuario existente dentro del sistema.
    /// </summary>
    /// <param name="command">Comando de activación del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación actualizada del usuario cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<UserDto>> ActivateUserAsync(
        ActivateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.UserId == Guid.Empty)
        {
            return Result.Failure<UserDto>(
                Error.Validation("Users.InvalidId", "El identificador del usuario es obligatorio."));
        }

        return await ExecuteAsync(async () =>
        {
            Usuario? user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
            if (user is null)
            {
                return Result.Failure<UserDto>(
                    Error.NotFound("Users.NotFound", $"No se encontró un usuario con identificador '{command.UserId}'."));
            }

            user.Activar();

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditUserEventAsync(
                user,
                "Users",
                "user.activated",
                $"Se activó el usuario con correo '{user.CorreoElectronico.Value}'.",
                new Dictionary<string, string>
                {
                    ["isActive"] = user.Activo.ToString(),
                    ["role"] = user.Rol.ToString()
                },
                cancellationToken);

            return Result.Success(user.ToUserDto());
        }, "Users.Domain");
    }

    /// <summary>
    /// Desactiva un usuario existente dentro del sistema.
    /// </summary>
    /// <param name="command">Comando de desactivación del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación actualizada del usuario cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<UserDto>> DeactivateUserAsync(
        DeactivateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.UserId == Guid.Empty)
        {
            return Result.Failure<UserDto>(
                Error.Validation("Users.InvalidId", "El identificador del usuario es obligatorio."));
        }

        return await ExecuteAsync(async () =>
        {
            Usuario? user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
            if (user is null)
            {
                return Result.Failure<UserDto>(
                    Error.NotFound("Users.NotFound", $"No se encontró un usuario con identificador '{command.UserId}'."));
            }

            user.Desactivar();

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditUserEventAsync(
                user,
                "Users",
                "user.deactivated",
                $"Se desactivó el usuario con correo '{user.CorreoElectronico.Value}'.",
                new Dictionary<string, string>
                {
                    ["isActive"] = user.Activo.ToString(),
                    ["role"] = user.Rol.ToString()
                },
                cancellationToken);

            return Result.Success(user.ToUserDto());
        }, "Users.Domain");
    }

    #endregion

    #region Casos de uso de consulta

    /// <summary>
    /// Obtiene un usuario por su identificador único.
    /// </summary>
    /// <param name="query">Consulta del usuario por identificador.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación del usuario cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<UserDto>> GetUserByIdAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.UserId == Guid.Empty)
        {
            return Result.Failure<UserDto>(
                Error.Validation("Users.InvalidId", "El identificador del usuario es obligatorio."));
        }

        Usuario? user = await _userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDto>(
                Error.NotFound("Users.NotFound", $"No se encontró un usuario con identificador '{query.UserId}'."));
        }

        return Result.Success(user.ToUserDto());
    }

    /// <summary>
    /// Obtiene un usuario por su correo electrónico.
    /// </summary>
    /// <param name="query">Consulta del usuario por correo electrónico.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación del usuario cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<UserDto>> GetUserByEmailAsync(
        GetUserByEmailQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(query.Email))
        {
            return Result.Failure<UserDto>(
                Error.Validation("Users.InvalidEmail", "El correo electrónico del usuario es obligatorio."));
        }

        return await ExecuteAsync(async () =>
        {
            Email email = CreateEmail(query.Email);

            Usuario? user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user is null)
            {
                return Result.Failure<UserDto>(
                    Error.NotFound("Users.NotFoundByEmail", $"No se encontró un usuario con el correo '{query.Email}'."));
            }

            return Result.Success(user.ToUserDto());
        }, "Users.Validation", Error.Validation);
    }

    #endregion

    #region Métodos privados auxiliares

    /// <summary>
    /// Construye un value object <see cref="Email"/> a partir de un valor textual.
    /// </summary>
    /// <param name="value">Valor textual del correo electrónico.</param>
    /// <returns>Instancia de <see cref="Email"/>.</returns>
    private static Email CreateEmail(string value)
    {
        return new Email(value);
    }

    private static Task<Error?> ValidateAsync<TCommand>(
        TCommand command,
        IValidator<TCommand> validator,
        CancellationToken cancellationToken)
    {
        return ApplicationExecution.ValidateAsync(
            command,
            validator,
            "Users.Validation",
            "La solicitud contiene errores de validación.",
            cancellationToken);
    }

    private static Task<Result<TResponse>> ExecuteAsync<TResponse>(
        Func<Task<Result<TResponse>>> operation,
        string errorCode,
        Func<string, string, Error>? errorFactory = null)
    {
        return ApplicationExecution.ExecuteAsync(operation, errorCode, errorFactory);
    }

    private static Task<Result> ExecuteAsync(
        Func<Task<Result>> operation,
        string errorCode,
        Func<string, string, Error>? errorFactory = null)
    {
        return ApplicationExecution.ExecuteAsync(operation, errorCode, errorFactory);
    }

    private static bool IsEmailConfirmationTokenValid(Usuario user, EmailConfirmationTokenValidationDto? tokenData)
    {
        if (tokenData is null)
        {
            return false;
        }

        return tokenData.UserId == user.Id
            && string.Equals(tokenData.Email, user.CorreoElectronico.Value, StringComparison.OrdinalIgnoreCase)
            && tokenData.UserVersionTicks == ResolveUserVersionTicks(user);
    }

    private static long ResolveUserVersionTicks(Usuario user)
    {
        return (user.FechaActualizacionUtc ?? user.FechaCreacionUtc).Ticks;
    }

    /// <summary>
    /// Registra un evento de auditoría asociado a una operación exitosa sobre usuarios.
    /// </summary>
    /// <param name="user">Usuario afectado por la operación.</param>
    /// <param name="module">Módulo funcional asociado al evento.</param>
    /// <param name="action">Acción semántica auditada.</param>
    /// <param name="detail">Detalle legible del evento.</param>
    /// <param name="metadata">Metadatos complementarios del evento.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    private Task AuditUserEventAsync(
        Usuario user,
        string module,
        string action,
        string detail,
        IReadOnlyDictionary<string, string>? metadata,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        return _auditTrailService.RegisterAsync(
            user.Id,
            nameof(Usuario),
            module,
            action,
            detail,
            metadata,
            cancellationToken);
    }

    private async Task<Result> SendEmailConfirmationNotificationAsync(
        Usuario user,
        string emailConfirmationUrl,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        string confirmationToken = _emailConfirmationTokenService.GenerateToken(user, EmailConfirmationTokenLifetime);
        string confirmationUrl = emailConfirmationUrl
            .Replace("%7BuserId%7D", Uri.EscapeDataString(user.Id.ToString()), StringComparison.OrdinalIgnoreCase)
            .Replace("{userId}", Uri.EscapeDataString(user.Id.ToString()), StringComparison.Ordinal)
            .Replace("%7Btoken%7D", Uri.EscapeDataString(confirmationToken), StringComparison.OrdinalIgnoreCase)
            .Replace("{token}", Uri.EscapeDataString(confirmationToken), StringComparison.Ordinal);

        Result emailResult = await _emailNotificationService.SendAccountEmailConfirmationAsync(
            new AccountEmailConfirmationNotification
            {
                ToEmail = user.CorreoElectronico.Value,
                RecipientName = user.Nombre,
                ConfirmationUrl = confirmationUrl
            },
            cancellationToken);

        await AuditUserEventAsync(
            user,
            "Users",
            emailResult.IsSuccess ? "user.email-confirmation.sent" : "user.email-confirmation.failed",
            emailResult.IsSuccess
                ? $"Se envió el correo de confirmación para el usuario '{user.CorreoElectronico.Value}'."
                : $"No fue posible entregar el correo de confirmación para el usuario '{user.CorreoElectronico.Value}'.",
            new Dictionary<string, string>
            {
                ["email"] = user.CorreoElectronico.Value,
                ["deliveryStatus"] = emailResult.IsSuccess ? "sent" : "failed",
                ["errorCode"] = emailResult.IsSuccess ? string.Empty : emailResult.Error.Code
            },
            cancellationToken);

        return emailResult;
    }

    #endregion
}