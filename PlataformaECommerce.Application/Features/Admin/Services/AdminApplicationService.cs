using FluentValidation;
using PlataformaECommerce.Application.Features.Admin;
using PlataformaECommerce.Application.Common.Execution;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Queries;
using FluentValidation;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Audit;
using PlataformaECommerce.Application.Interfaces.Repositories.Cart;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Admin;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using Microsoft.Extensions.DependencyInjection;

namespace PlataformaECommerce.Application.Features.Admin.Services;

/// <summary>
/// Mantiene la frontera pública heredada del módulo administrativo delegando en servicios especializados.
/// </summary>
public sealed class AdminApplicationService : IAdminApplicationService
{
    private readonly IAdminUserService _adminUserService;
    private readonly IAdminDashboardService _adminDashboardService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="AdminApplicationService"/>.
    /// </summary>
    [ActivatorUtilitiesConstructor]
    public AdminApplicationService(
        IAdminUserService adminUserService,
        IAdminDashboardService adminDashboardService)
    {
        _adminUserService = adminUserService ?? throw new ArgumentNullException(nameof(adminUserService));
        _adminDashboardService = adminDashboardService ?? throw new ArgumentNullException(nameof(adminDashboardService));
    }

    /// <summary>
    /// Inicializa una nueva instancia de compatibilidad para pruebas existentes del módulo administrativo.
    /// </summary>
    public AdminApplicationService(
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        ICartRepository cartRepository,
        IAuditRepository auditRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IAuditTrailService auditTrailService,
        ICurrentUserService currentUserService,
        IValidator<RegisterAdminCommand> registerAdminCommandValidator,
        IValidator<ResetUserPasswordCommand>? resetUserPasswordCommandValidator = null)
        : this(
            new AdminUserService(
                userRepository,
                unitOfWork,
                passwordHasher,
                auditTrailService,
                new AdminAuthService(userRepository, currentUserService),
                registerAdminCommandValidator,
                resetUserPasswordCommandValidator),
            new AdminDashboardService(
                productRepository,
                orderRepository,
                userRepository,
                cartRepository,
                auditRepository,
                currentUserService))
    {
    }

    /// <inheritdoc />
    public Task<Result<AdminDto>> RegisterAdminAsync(
        RegisterAdminCommand command,
        CancellationToken cancellationToken = default)
    {
        return _adminUserService.RegisterAdminAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<AdminRegistrationDefinitionDto>> GetAdminRegistrationDefinitionAsync(
        GetAdminRegistrationDefinitionQuery query,
        CancellationToken cancellationToken = default)
    {
        return _adminUserService.GetAdminRegistrationDefinitionAsync(query, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<AdminDashboardDto>> GetDashboardAsync(
        GetAdminDashboardQuery query,
        CancellationToken cancellationToken = default)
    {
        return _adminDashboardService.GetDashboardAsync(query, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<AdminUsersBackofficeDto>> GetUsersAsync(
        GetAdminUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        return _adminUserService.GetUsersAsync(query, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<AdminBackofficeUserDto>> ResetUserPasswordAsync(
        ResetUserPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        return _adminUserService.ResetUserPasswordAsync(command, cancellationToken);
    }
}
