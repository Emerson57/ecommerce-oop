using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Admin;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Web.Configuration;

namespace PlataformaECommerce.Web.Initialization;

/// <summary>
/// Orquesta el bootstrap seguro del primer <see cref="RolUsuario.SuperUsuario"/> en el arranque web.
/// </summary>
/// <remarks>
/// Este servicio centraliza la validación de la configuración de bootstrap,
/// verifica si ya existen cuentas administrativas y ejecuta una creación controlada
/// de una sola vez cuando el entorno lo habilita explícitamente.
/// </remarks>
public sealed class SuperUserBootstrapService
{
    private readonly BootstrapSuperUserOptions _options;
    private readonly IUserRepository _userRepository;
    private readonly IAdminApplicationService _adminApplicationService;
    private readonly ILogger<SuperUserBootstrapService> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="SuperUserBootstrapService"/>.
    /// </summary>
    /// <param name="options">Opciones de bootstrap del super usuario.</param>
    /// <param name="userRepository">Repositorio de usuarios del sistema.</param>
    /// <param name="adminApplicationService">Servicio de aplicación administrativo.</param>
    /// <param name="logger">Registrador estructurado del proceso de bootstrap.</param>
    public SuperUserBootstrapService(
        IOptions<BootstrapSuperUserOptions> options,
        IUserRepository userRepository,
        IAdminApplicationService adminApplicationService,
        ILogger<SuperUserBootstrapService> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value;
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _adminApplicationService = adminApplicationService ?? throw new ArgumentNullException(nameof(adminApplicationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Ejecuta el bootstrap del primer super usuario si la configuración y el estado del sistema lo requieren.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    public async Task BootstrapAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("El bootstrap del super usuario está deshabilitado.");
            return;
        }

        ValidateOptions(_options);

        if (await SuperUserExistsAsync(cancellationToken).ConfigureAwait(false))
        {
            _logger.LogInformation("El bootstrap del super usuario fue omitido porque ya existe una cuenta con rol SuperUsuario.");
            return;
        }

        RegisterAdminCommand command = BuildCommand(_options);
        var result = await _adminApplicationService.RegisterAdminAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"No fue posible bootstrappear el super usuario inicial. {result.Error.Code}: {result.Error.Message}");
        }

        _logger.LogWarning(
            "Se creó el super usuario inicial '{Email}' mediante bootstrap. Deshabilite {SectionName} para evitar nuevas ejecuciones.",
            command.Email,
            BootstrapSuperUserOptions.SectionName);
    }

    private Task<bool> SuperUserExistsAsync(CancellationToken cancellationToken)
    {
        return _userRepository.ExistsByRoleAsync(RolUsuario.SuperUsuario, cancellationToken);
    }

    private static RegisterAdminCommand BuildCommand(BootstrapSuperUserOptions options)
    {
        return new RegisterAdminCommand
        {
            Name = options.Name.Trim(),
            Email = options.Email.Trim(),
            Password = options.Password,
            ConfirmPassword = options.Password,
            Area = options.Area.Trim(),
            Role = RolUsuario.SuperUsuario,
            IsActive = true,
            IsEmailConfirmed = true,
            IsBootstrap = true,
            Source = "Web.Startup.Bootstrap",
            Reason = "Bootstrap seguro del primer super usuario."
        };
    }

    private static void ValidateOptions(BootstrapSuperUserOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.Name))
        {
            throw new InvalidOperationException("El bootstrap del super usuario requiere un nombre válido.");
        }

        if (string.IsNullOrWhiteSpace(options.Email))
        {
            throw new InvalidOperationException("El bootstrap del super usuario requiere un correo electrónico válido.");
        }

        if (string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException("El bootstrap del super usuario requiere una contraseña válida.");
        }

        if (string.IsNullOrWhiteSpace(options.Area))
        {
            throw new InvalidOperationException("El bootstrap del super usuario requiere un área válida.");
        }
    }
}
