using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Features.Admin.Queries;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Admin.Services;

/// <summary>
/// Centraliza la autorización y el contexto de actor del módulo administrativo.
/// </summary>
public sealed class AdminAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="AdminAuthService"/>.
    /// </summary>
    public AdminAuthService(
        IUserRepository userRepository,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    /// <summary>
    /// Obtiene el identificador del actor actual cuando existe sesión autenticada.
    /// </summary>
    public Guid? GetCurrentActorUserId()
    {
        return _currentUserService.IsAuthenticated
            ? _currentUserService.UserId
            : null;
    }

    /// <summary>
    /// Obtiene el nombre visible del actor actual cuando existe sesión autenticada.
    /// </summary>
    public string? GetCurrentActorName()
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return null;
        }

        return _currentUserService.UserName ?? _currentUserService.Email;
    }

    /// <summary>
    /// Obtiene el rol funcional del actor actual cuando existe sesión autenticada.
    /// </summary>
    public string? GetCurrentActorRole()
    {
        return _currentUserService.IsAuthenticated
            ? _currentUserService.Role
            : null;
    }

    /// <summary>
    /// Valida si el alta administrativa solicitada está autorizada para el actor actual.
    /// </summary>
    public async Task<Error?> EnsureAdministrativeRegistrationIsAuthorizedAsync(
        RegisterAdminCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.IsBootstrap)
        {
            bool superUserExists = await SuperUserExistsAsync(cancellationToken).ConfigureAwait(false);

            return superUserExists
                ? Error.Conflict("Admin.BootstrapAlreadyCompleted", "El bootstrap del super usuario ya fue completado previamente.")
                : null;
        }

        if (!_currentUserService.IsAuthenticated)
        {
            return Error.Unauthorized("Admin.AuthenticationRequired", "Se requiere una sesión autenticada para registrar cuentas administrativas.");
        }

        return await EnsureAuthenticatedSuperUserActorAsync(
            "Admin.AuthenticationRequired",
            "Se requiere una sesión autenticada para registrar cuentas administrativas.",
            "Admin.SuperUserRequired",
            "Solo un super usuario puede crear o aprovisionar cuentas administrativas.",
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Valida si el actor actual puede consultar el backoffice de usuarios.
    /// </summary>
    public Task<Error?> EnsureUsersBackofficeAccessAsync(GetAdminUsersQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        return EnsureSuperUserBackofficeAccessAsync(
            query.RequireSuperUserAccess,
            "Admin.AuthenticationRequired",
            "Se requiere una sesión autenticada para consultar el backoffice de usuarios.",
            "Admin.SuperUserRequiredForUsersBackoffice",
            "Solo un super usuario puede consultar el backoffice de usuarios.",
            cancellationToken);
    }

    /// <summary>
    /// Valida si el actor actual dispone de acceso de super usuario cuando el caso de uso lo requiere.
    /// </summary>
    public async Task<Error?> EnsureSuperUserBackofficeAccessAsync(
        bool requireSuperUserAccess,
        string authenticationErrorCode,
        string authenticationErrorMessage,
        string authorizationErrorCode,
        string authorizationErrorMessage,
        CancellationToken cancellationToken)
    {
        if (!requireSuperUserAccess)
        {
            return null;
        }

        if (!_currentUserService.IsAuthenticated)
        {
            return Error.Unauthorized(authenticationErrorCode, authenticationErrorMessage);
        }

        return await EnsureAuthenticatedSuperUserActorAsync(
            authenticationErrorCode,
            authenticationErrorMessage,
            authorizationErrorCode,
            authorizationErrorMessage,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Valida que el actor actual sea un super usuario autenticado y habilitado.
    /// </summary>
    public async Task<Error?> EnsureAuthenticatedSuperUserActorAsync(
        string authenticationErrorCode,
        string authenticationErrorMessage,
        string authorizationErrorCode,
        string authorizationErrorMessage,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Error.Unauthorized(authenticationErrorCode, authenticationErrorMessage);
        }

        Guid? actorUserId = _currentUserService.UserId;
        if (!actorUserId.HasValue || actorUserId == Guid.Empty)
        {
            return Error.Unauthorized(authenticationErrorCode, authenticationErrorMessage);
        }

        Administrador? actor = await _userRepository.GetAdministratorByIdAsync(actorUserId.Value, cancellationToken).ConfigureAwait(false);

        return actor is { EsSuperUsuario: true } && actor.EstaHabilitado()
            ? null
            : Error.Unauthorized(authorizationErrorCode, authorizationErrorMessage);
    }

    /// <summary>
    /// Determina si ya existe un super usuario persistido en el sistema.
    /// </summary>
    public Task<bool> SuperUserExistsAsync(CancellationToken cancellationToken)
    {
        return _userRepository.ExistsByRoleAsync(RolUsuario.SuperUsuario, cancellationToken);
    }
}
