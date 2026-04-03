using FluentValidation;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin;
using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Mappings;
using PlataformaECommerce.Application.Features.Admin.Queries;
using PlataformaECommerce.Application.Features.Admin.Validators;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Admin;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Application.Features.Admin.Services;

/// <summary>
/// Orquesta los casos de uso administrativos relacionados con aprovisionamiento y gestión de usuarios.
/// </summary>
public sealed class AdminUserService : IAdminUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditTrailService _auditTrailService;
    private readonly AdminAuthService _adminAuthService;
    private readonly IValidator<RegisterAdminCommand> _registerAdminCommandValidator;
    private readonly IValidator<ResetUserPasswordCommand> _resetUserPasswordCommandValidator;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="AdminUserService"/>.
    /// </summary>
    public AdminUserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IAuditTrailService auditTrailService,
        AdminAuthService adminAuthService,
        IValidator<RegisterAdminCommand> registerAdminCommandValidator,
        IValidator<ResetUserPasswordCommand>? resetUserPasswordCommandValidator = null)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _auditTrailService = auditTrailService ?? throw new ArgumentNullException(nameof(auditTrailService));
        _adminAuthService = adminAuthService ?? throw new ArgumentNullException(nameof(adminAuthService));
        _registerAdminCommandValidator = registerAdminCommandValidator ?? throw new ArgumentNullException(nameof(registerAdminCommandValidator));
        _resetUserPasswordCommandValidator = resetUserPasswordCommandValidator ?? new ResetUserPasswordCommandValidator();
    }

    /// <inheritdoc />
    public async Task<Result<AdminDto>> RegisterAdminAsync(RegisterAdminCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await AdminServiceSupport.ValidateAsync(
            command,
            _registerAdminCommandValidator,
            "Admin.Validation",
            "La solicitud contiene errores de validación.",
            cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<AdminDto>(validationError);
        }

        Error? authorizationError = await _adminAuthService.EnsureAdministrativeRegistrationIsAuthorizedAsync(command, cancellationToken);
        if (authorizationError is not null)
        {
            return Result.Failure<AdminDto>(authorizationError);
        }

        return await AdminServiceSupport.ExecuteAsync(async () =>
        {
            Email email = AdminServiceSupport.CreateEmail(command.Email);

            bool emailExists = await _userRepository.ExistsByEmailAsync(email, cancellationToken);
            if (emailExists)
            {
                return Result.Failure<AdminDto>(
                    Error.Conflict("Admin.EmailAlreadyExists", $"Ya existe un usuario registrado con el correo '{command.Email}'."));
            }

            Administrador admin = CreateAdministrator(command, email);
            Error? registrationError = await PersistAndAuditAdminRegistrationAsync(admin, command, cancellationToken);
            if (registrationError is not null)
            {
                return Result.Failure<AdminDto>(registrationError);
            }

            return Result.Success(admin.ToAdminDto());
        }, "Admin.Domain");
    }

    /// <inheritdoc />
    public async Task<Result<AdminRegistrationDefinitionDto>> GetAdminRegistrationDefinitionAsync(
        GetAdminRegistrationDefinitionQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        Error? authorizationError = await _adminAuthService.EnsureSuperUserBackofficeAccessAsync(
            query.RequireSuperUserAccess,
            "Admin.AuthenticationRequired",
            "Se requiere una sesión autenticada para consultar la definición funcional de creación de administradores.",
            "Admin.SuperUserRequiredForAdminCreationDefinition",
            "Solo un super usuario puede consultar la definición funcional de creación de administradores.",
            cancellationToken);

        if (authorizationError is not null)
        {
            return Result.Failure<AdminRegistrationDefinitionDto>(authorizationError);
        }

        DateTime generatedAtUtc = query.ReferenceDateUtc ?? DateTime.UtcNow;

        return Result.Success(new AdminRegistrationDefinitionDto
        {
            GeneratedAtUtc = generatedAtUtc,
            GeneratedByUserId = _adminAuthService.GetCurrentActorUserId(),
            GeneratedByUserName = _adminAuthService.GetCurrentActorName(),
            Source = query.Source ?? "Admin.Backoffice.Users.Create",
            ExternalReference = query.ExternalReference,
            AllowedRole = RolUsuario.Administrador,
            DefaultArea = AdminRegistrationPolicies.DefaultArea,
            DefaultIsActive = AdminRegistrationPolicies.DefaultIsActive,
            DefaultIsEmailConfirmed = AdminRegistrationPolicies.DefaultIsEmailConfirmed,
            RequiresAuthenticatedSuperUser = true,
            AllowsSuperUserCreation = false,
            RequiresUniqueEmail = true,
            RequiresAuditTrail = true,
            SupportsInitialActivationStatus = true,
            SupportsInitialEmailConfirmationStatus = true,
            PasswordMinLength = AdminRegistrationPolicies.MinPasswordLength,
            RequiresUppercase = true,
            RequiresLowercase = true,
            RequiresDigit = true,
            RequiresSpecialCharacter = true,
            RequiredFields = AdminRegistrationPolicies.RequiredFields
        });
    }

    /// <inheritdoc />
    public async Task<Result<AdminUsersBackofficeDto>> GetUsersAsync(GetAdminUsersQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        Error? authorizationError = await _adminAuthService.EnsureUsersBackofficeAccessAsync(query, cancellationToken);
        if (authorizationError is not null)
        {
            return Result.Failure<AdminUsersBackofficeDto>(authorizationError);
        }

        return await AdminServiceSupport.ExecuteAsync(async () =>
        {
            DateTime generatedAtUtc = query.ReferenceDateUtc ?? DateTime.UtcNow;
            DateTime recentAccessWindowStartUtc = generatedAtUtc.AddDays(-query.NormalizedRecentAccessWindowInDays);
            IReadOnlyCollection<Usuario> users = await GetUsersForBackofficeAsync(query, cancellationToken);

            AdminBackofficeUserDto[] projectedUsers = users
                .Where(user => !query.OnlyActiveUsers || user.Activo)
                .Where(user => !query.OnlyAdministrativeUsers || user is Administrador)
                .Select(AdminServiceSupport.MapToBackofficeUser)
                .OrderByDescending(user => user.IsAdministrative)
                .ThenByDescending(user => user.IsSuperUser)
                .ThenBy(user => user.Name, StringComparer.Ordinal)
                .ToArray();

            return Result.Success(new AdminUsersBackofficeDto
            {
                GeneratedAtUtc = generatedAtUtc,
                GeneratedByUserId = _adminAuthService.GetCurrentActorUserId(),
                GeneratedByUserName = _adminAuthService.GetCurrentActorName(),
                Source = query.Source ?? "Admin.Backoffice.Users",
                ExternalReference = query.ExternalReference,
                RecentAccessWindowStartUtc = recentAccessWindowStartUtc,
                TotalUsers = projectedUsers.Length,
                ActiveUsers = projectedUsers.Count(user => user.IsActive),
                InactiveUsers = projectedUsers.Count(user => !user.IsActive),
                EmailConfirmedUsers = projectedUsers.Count(user => user.IsEmailConfirmed),
                EnabledUsers = projectedUsers.Count(user => user.IsEnabled),
                TotalCustomers = projectedUsers.Count(user => !user.IsAdministrative),
                TotalAdministrators = projectedUsers.Count(user => user.IsAdministrative),
                TotalSuperUsers = projectedUsers.Count(user => user.IsSuperUser),
                UsersWithRecentAccess = projectedUsers.Count(user => user.LastAccessAtUtc >= recentAccessWindowStartUtc),
                Users = projectedUsers
            });
        }, "Admin.Users");
    }

    /// <inheritdoc />
    public async Task<Result<AdminBackofficeUserDto>> ResetUserPasswordAsync(ResetUserPasswordCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await AdminServiceSupport.ValidateAsync(
            command,
            _resetUserPasswordCommandValidator,
            "Admin.ResetUserPasswordValidation",
            "La solicitud contiene errores de validación.",
            cancellationToken);
        if (validationError is not null)
        {
            return Result.Failure<AdminBackofficeUserDto>(validationError);
        }

        Error? authorizationError = await _adminAuthService.EnsureAuthenticatedSuperUserActorAsync(
            "Admin.AuthenticationRequired",
            "Se requiere una sesión autenticada para restablecer contraseñas de usuarios.",
            "Admin.SuperUserRequiredForUserPasswordReset",
            "Solo un super usuario puede restablecer contraseñas de usuarios.",
            cancellationToken);
        if (authorizationError is not null)
        {
            return Result.Failure<AdminBackofficeUserDto>(authorizationError);
        }

        return await AdminServiceSupport.ExecuteAsync(async () =>
        {
            Usuario? user = await _userRepository.GetByIdAsync(command.TargetUserId, cancellationToken);
            if (user is null)
            {
                return Result.Failure<AdminBackofficeUserDto>(
                    Error.NotFound("Admin.UserNotFound", $"No se encontró el usuario con identificador '{command.TargetUserId}'."));
            }

            string passwordHash = _passwordHasher.HashPassword(command.NewPassword);
            user.CambiarContrasenaHash(passwordHash);

            try
            {
                await _userRepository.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await AuditUserPasswordResetEventAsync(user, command, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                return Result.Failure<AdminBackofficeUserDto>(
                    Error.Failure("Admin.UserPasswordResetPersistence", "No fue posible completar el restablecimiento administrativo de la contraseña."));
            }

            return Result.Success(AdminServiceSupport.MapToBackofficeUser(user));
        }, "Admin.UserPasswordReset");
    }

    private async Task<IReadOnlyCollection<Usuario>> GetUsersForBackofficeAsync(GetAdminUsersQuery query, CancellationToken cancellationToken)
    {
        if (query.OnlyAdministrativeUsers)
        {
            IReadOnlyCollection<Administrador> administrators = await _userRepository.GetAdministratorsAsync(cancellationToken);
            return administrators.Cast<Usuario>().ToArray();
        }

        return await _userRepository.GetAllAsync(cancellationToken);
    }

    private Administrador CreateAdministrator(RegisterAdminCommand command, Email email)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(email);

        string passwordHash = _passwordHasher.HashPassword(command.Password);
        RolUsuario targetRole = command.IsBootstrap
            ? RolUsuario.SuperUsuario
            : RolUsuario.Administrador;

        Administrador admin = new(
            command.Name,
            email,
            passwordHash,
            command.Area,
            targetRole);

        if (!command.IsActive)
        {
            admin.Desactivar();
        }

        if (command.IsEmailConfirmed)
        {
            admin.ConfirmarCorreoElectronico();
        }

        return admin;
    }

    private async Task<Error?> PersistAndAuditAdminRegistrationAsync(
        Administrador admin,
        RegisterAdminCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            await _userRepository.AddAsync(admin, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await AuditAdminEventAsync(admin, command, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            return null;
        }
        catch (InvalidOperationException)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            return Error.Failure(
                "Admin.Persistence",
                "No fue posible completar el alta administrativa con persistencia y auditoría obligatoria.");
        }
    }

    private Task AuditAdminEventAsync(
        Administrador admin,
        RegisterAdminCommand command,
        CancellationToken cancellationToken)
    {
        return _auditTrailService.RegisterAsync(
            admin.Id,
            nameof(Administrador),
            "Admin",
            "admin.registered",
            $"Se registró un nuevo administrador con correo '{admin.CorreoElectronico.Value}'.",
            new Dictionary<string, string>
            {
                ["role"] = admin.Rol.ToString(),
                ["email"] = admin.CorreoElectronico.Value,
                ["area"] = admin.Area,
                ["isActive"] = admin.Activo.ToString(),
                ["isEmailConfirmed"] = admin.CorreoConfirmado.ToString(),
                ["createdByUserId"] = _adminAuthService.GetCurrentActorUserId()?.ToString() ?? "bootstrap",
                ["createdByRole"] = _adminAuthService.GetCurrentActorRole() ?? RolUsuario.SuperUsuario.ToString(),
                ["creationMode"] = command.IsBootstrap ? "Bootstrap" : "Backoffice",
                ["source"] = command.Source ?? (command.IsBootstrap ? "Web.Startup.Bootstrap" : "Admin.Backoffice.Users"),
                ["externalReference"] = command.ExternalReference ?? string.Empty,
                ["reason"] = command.Reason ?? string.Empty
            },
            cancellationToken);
    }

    private Task AuditUserPasswordResetEventAsync(
        Usuario user,
        ResetUserPasswordCommand command,
        CancellationToken cancellationToken)
    {
        string aggregateType = user switch
        {
            Administrador => nameof(Administrador),
            Cliente => nameof(Cliente),
            _ => nameof(Usuario)
        };

        return _auditTrailService.RegisterAsync(
            user.Id,
            aggregateType,
            "Admin",
            "admin.user-password-reset",
            $"Se restableció administrativamente la contraseña del usuario '{user.CorreoElectronico.Value}'.",
            new Dictionary<string, string>
            {
                ["targetRole"] = user.Rol.ToString(),
                ["targetEmail"] = user.CorreoElectronico.Value,
                ["targetIsAdministrative"] = (user is Administrador).ToString(),
                ["resetByUserId"] = _adminAuthService.GetCurrentActorUserId()?.ToString() ?? string.Empty,
                ["resetByRole"] = _adminAuthService.GetCurrentActorRole() ?? string.Empty,
                ["source"] = command.Source ?? "Admin.Backoffice.Users",
                ["externalReference"] = command.ExternalReference ?? string.Empty,
                ["reason"] = command.Reason ?? string.Empty
            },
            cancellationToken);
    }
}
