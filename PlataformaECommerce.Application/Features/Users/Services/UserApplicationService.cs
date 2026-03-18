using FluentValidation.Results;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Validators;
using PlataformaECommerce.Application.Features.Users.Commands;
using PlataformaECommerce.Application.Features.Users.DTOs;
using PlataformaECommerce.Application.Features.Users.Queries;
using PlataformaECommerce.Application.Features.Users.Validators;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
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
/// Este servicio no reemplaza a handlers CQRS, pero constituye una capa
/// de orquestación válida y profesional para centralizar los principales
/// casos de uso del módulo de usuarios.
/// </remarks>
public sealed class UserApplicationService
{
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

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="UserApplicationService"/>.
    /// </summary>
    /// <param name="userRepository">Repositorio de usuarios.</param>
    /// <param name="unitOfWork">Unidad de trabajo.</param>
    /// <param name="passwordHasher">Servicio de hashing de contraseñas.</param>
    public UserApplicationService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
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

        ValidationResult validationResult = await new RegisterCustomerCommandValidator()
            .ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Failure<CustomerDto>(BuildValidationError(validationResult, "Users.Validation"));
        }

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

        foreach (string preference in command.Preferences
                     .Where(value => !string.IsNullOrWhiteSpace(value))
                     .Select(value => value.Trim())
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            customer.AgregarPreferencia(preference);
        }

        await _userRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToCustomerDto(customer));
    }

    /// <summary>
    /// Registra un nuevo administrador dentro del sistema.
    /// </summary>
    /// <param name="command">Comando de registro del administrador.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la representación del administrador registrado cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<AdminDto>> RegisterAdminAsync(
        RegisterAdminCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        ValidationResult validationResult = await new RegisterAdminCommandValidator()
            .ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Failure<AdminDto>(BuildValidationError(validationResult, "Admin.Validation"));
        }

        Email email = CreateEmail(command.Email);

        bool emailExists = await _userRepository.ExistsByEmailAsync(email, cancellationToken);
        if (emailExists)
        {
            return Result.Failure<AdminDto>(
                Error.Conflict("Admin.EmailAlreadyExists", $"Ya existe un usuario registrado con el correo '{command.Email}'."));
        }

        string passwordHash = _passwordHasher.HashPassword(command.Password);

        Administrador admin = new(
            command.Name,
            email,
            passwordHash,
            command.Area);

        if (!command.IsActive)
        {
            admin.Desactivar();
        }

        if (command.IsEmailConfirmed)
        {
            admin.ConfirmarCorreoElectronico();
        }

        await _userRepository.AddAsync(admin, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToAdminDto(admin));
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

        ValidationResult validationResult = await new UpdateUserBasicDataCommandValidator()
            .ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Failure<UserDto>(BuildValidationError(validationResult, "Users.Validation"));
        }

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

        return Result.Success(MapToUserDto(user));
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

        Usuario? user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDto>(
                Error.NotFound("Users.NotFound", $"No se encontró un usuario con identificador '{command.UserId}'."));
        }

        user.ConfirmarCorreoElectronico();

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToUserDto(user));
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

        Usuario? user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDto>(
                Error.NotFound("Users.NotFound", $"No se encontró un usuario con identificador '{command.UserId}'."));
        }

        user.Activar();

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToUserDto(user));
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

        Usuario? user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDto>(
                Error.NotFound("Users.NotFound", $"No se encontró un usuario con identificador '{command.UserId}'."));
        }

        user.Desactivar();

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToUserDto(user));
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

        return Result.Success(MapToUserDto(user));
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

        Email email = CreateEmail(query.Email);

        Usuario? user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDto>(
                Error.NotFound("Users.NotFoundByEmail", $"No se encontró un usuario con el correo '{query.Email}'."));
        }

        return Result.Success(MapToUserDto(user));
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

    /// <summary>
    /// Construye un error de validación de aplicación a partir del resultado de FluentValidation.
    /// </summary>
    /// <param name="validationResult">Resultado de validación.</param>
    /// <param name="errorCode">Código base del error.</param>
    /// <returns>Error de validación estructurado.</returns>
    private static Error BuildValidationError(ValidationResult validationResult, string errorCode)
    {
        string message = string.Join(
            " | ",
            validationResult.Errors
                .Where(error => !string.IsNullOrWhiteSpace(error.ErrorMessage))
                .Select(error => error.ErrorMessage.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase));

        return Error.Validation(
            errorCode,
            string.IsNullOrWhiteSpace(message)
                ? "La solicitud contiene errores de validación."
                : message);
    }

    /// <summary>
    /// Proyecta una entidad de dominio <see cref="Usuario"/> hacia un <see cref="UserDto"/>.
    /// </summary>
    /// <param name="user">Usuario a proyectar.</param>
    /// <returns>DTO general del usuario.</returns>
    private static UserDto MapToUserDto(Usuario user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Nombre,
            Email = user.CorreoElectronico.Value,
            Role = user.Rol,
            IsActive = user.Activo,
            IsEmailConfirmed = user.CorreoConfirmado,
            CreatedAtUtc = user.FechaCreacionUtc,
            UpdatedAtUtc = user.FechaActualizacionUtc,
            LastAccessAtUtc = user.FechaUltimoAccesoUtc
        };
    }

    /// <summary>
    /// Proyecta una entidad de dominio <see cref="Cliente"/> hacia un <see cref="CustomerDto"/>.
    /// </summary>
    /// <param name="customer">Cliente a proyectar.</param>
    /// <returns>DTO del cliente.</returns>
    private static CustomerDto MapToCustomerDto(Cliente customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Nombre,
            Email = customer.CorreoElectronico.Value,
            Role = customer.Rol,
            IsActive = customer.Activo,
            IsEmailConfirmed = customer.CorreoConfirmado,
            TotalPurchases = customer.TotalCompras,
            PurchaseHistory = customer.HistorialCompras.ToArray(),
            Preferences = customer.Preferencias.OrderBy(value => value).ToArray(),
            CreatedAtUtc = customer.FechaCreacionUtc,
            UpdatedAtUtc = customer.FechaActualizacionUtc,
            LastAccessAtUtc = customer.FechaUltimoAccesoUtc
        };
    }

    /// <summary>
    /// Proyecta una entidad de dominio <see cref="Administrador"/> hacia un <see cref="AdminDto"/>.
    /// </summary>
    /// <param name="admin">Administrador a proyectar.</param>
    /// <returns>DTO del administrador.</returns>
    private static AdminDto MapToAdminDto(Administrador admin)
    {
        return new AdminDto
        {
            Id = admin.Id,
            Name = admin.Nombre,
            Email = admin.CorreoElectronico.Value,
            Role = admin.Rol,
            IsActive = admin.Activo,
            IsEmailConfirmed = admin.CorreoConfirmado,
            Area = admin.Area,
            CreatedAtUtc = admin.FechaCreacionUtc,
            UpdatedAtUtc = admin.FechaActualizacionUtc,
            LastAccessAtUtc = admin.FechaUltimoAccesoUtc
        };
    }

    #endregion
}