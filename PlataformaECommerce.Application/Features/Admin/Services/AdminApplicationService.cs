using FluentValidation;
using PlataformaECommerce.Application.Features.Admin;
using PlataformaECommerce.Application.Common.Execution;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Mappings;
using PlataformaECommerce.Application.Features.Admin.Queries;
using PlataformaECommerce.Application.Features.Admin.Validators;
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
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Application.Features.Admin.Services;

/// <summary>
/// Proporciona los casos de uso de aplicación relacionados con la gestión de administradores.
/// </summary>
/// <remarks>
/// Este servicio constituye la implementación pública de los casos de uso administrativos
/// del backoffice, utilizando comandos y consultas como modelos de entrada para coordinar
/// validación, persistencia, seguridad, auditoría y construcción del dashboard desde <c>Application</c>.
/// </remarks>
public sealed class AdminApplicationService : IAdminApplicationService
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditTrailService _auditTrailService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<RegisterAdminCommand> _registerAdminCommandValidator;
    private readonly IValidator<ResetUserPasswordCommand> _resetUserPasswordCommandValidator;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="AdminApplicationService"/>.
    /// </summary>
    /// <param name="productRepository">Repositorio de productos.</param>
    /// <param name="orderRepository">Repositorio de pedidos.</param>
    /// <param name="userRepository">Repositorio de usuarios.</param>
    /// <param name="cartRepository">Repositorio de carritos.</param>
    /// <param name="auditRepository">Repositorio documental de auditoría.</param>
    /// <param name="unitOfWork">Unidad de trabajo.</param>
    /// <param name="passwordHasher">Servicio de hashing de contraseñas.</param>
    /// <param name="auditTrailService">Servicio transversal de auditoría.</param>
    /// <param name="currentUserService">Servicio de usuario actual.</param>
    /// <param name="registerAdminCommandValidator">Validador estructural del comando de registro administrativo.</param>
    /// <param name="resetUserPasswordCommandValidator">Validador estructural del restablecimiento administrativo de contraseña.</param>
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
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _auditRepository = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _auditTrailService = auditTrailService ?? throw new ArgumentNullException(nameof(auditTrailService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _registerAdminCommandValidator = registerAdminCommandValidator ?? throw new ArgumentNullException(nameof(registerAdminCommandValidator));
        _resetUserPasswordCommandValidator = resetUserPasswordCommandValidator ?? new ResetUserPasswordCommandValidator();
    }

    /// <inheritdoc />
    public async Task<Result<AdminDto>> RegisterAdminAsync(
        RegisterAdminCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (validationError is not null)
        {
            return Result.Failure<AdminDto>(validationError);
        }

        Error? authorizationError = await EnsureAdministrativeRegistrationIsAuthorizedAsync(command, cancellationToken).ConfigureAwait(false);
        if (authorizationError is not null)
        {
            return Result.Failure<AdminDto>(authorizationError);
        }

        return await ExecuteAsync(async () =>
        {
            Email email = CreateEmail(command.Email);

            bool emailExists = await _userRepository.ExistsByEmailAsync(email, cancellationToken).ConfigureAwait(false);
            if (emailExists)
            {
                return Result.Failure<AdminDto>(
                    Error.Conflict("Admin.EmailAlreadyExists", $"Ya existe un usuario registrado con el correo '{command.Email}'."));
            }

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

            try
            {
                await _userRepository.AddAsync(admin, cancellationToken).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await AuditAdminEventAsync(admin, command, cancellationToken).ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
                return Result.Failure<AdminDto>(
                    Error.Failure("Admin.Persistence", "No fue posible completar el alta administrativa."));
            }

            return Result.Success(admin.ToAdminDto());
        }, "Admin.Domain").ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<AdminRegistrationDefinitionDto>> GetAdminRegistrationDefinitionAsync(
        GetAdminRegistrationDefinitionQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        Error? authorizationError = await EnsureSuperUserBackofficeAccessAsync(
            query.RequireSuperUserAccess,
            "Admin.AuthenticationRequired",
            "Se requiere una sesión autenticada para consultar la definición funcional de creación de administradores.",
            "Admin.SuperUserRequiredForAdminCreationDefinition",
            "Solo un super usuario puede consultar la definición funcional de creación de administradores.",
            cancellationToken).ConfigureAwait(false);

        if (authorizationError is not null)
        {
            return Result.Failure<AdminRegistrationDefinitionDto>(authorizationError);
        }

        DateTime generatedAtUtc = query.ReferenceDateUtc ?? DateTime.UtcNow;

        return Result.Success(new AdminRegistrationDefinitionDto
        {
            GeneratedAtUtc = generatedAtUtc,
            GeneratedByUserId = GetCurrentActorUserId(),
            GeneratedByUserName = GetCurrentActorName(),
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
    public Task<Result<AdminDashboardDto>> GetDashboardAsync(
        GetAdminDashboardQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return ExecuteAsync(async () =>
        {
            DateTime windowEndUtc = query.ReferenceDateUtc ?? DateTime.UtcNow;
            int windowInDays = query.NormalizedWindowInDays;
            DateTime windowStartUtc = windowEndUtc.AddDays(-windowInDays);

            IReadOnlyCollection<Producto> products = await _productRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            IReadOnlyCollection<Pedido> orders = await _orderRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            IReadOnlyCollection<Usuario> users = await _userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            IReadOnlyCollection<CarritoCompra> carts = await _cartRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

            string currency = orders.FirstOrDefault()?.Total.Currency
                ?? products.FirstOrDefault()?.Precio.Currency
                ?? "COP";

            AuditSearchResult recentAuditWindow = await _auditRepository.SearchAsync(
                new AuditSearchFilter
                {
                    FromUtc = windowEndUtc.AddHours(-24),
                    PageNumber = 1,
                    PageSize = 1,
                    SortDescending = true
                },
                cancellationToken).ConfigureAwait(false);

            AuditSearchResult recentActivity = await _auditRepository.SearchAsync(
                new AuditSearchFilter
                {
                    PageNumber = 1,
                    PageSize = 5,
                    SortDescending = true
                },
                cancellationToken).ConfigureAwait(false);

            int lowStockThreshold = query.NormalizedLowStockThreshold;

            return Result.Success(new AdminDashboardDto
            {
                GeneratedAtUtc = windowEndUtc,
                WindowStartUtc = windowStartUtc,
                WindowEndUtc = windowEndUtc,
                WindowInDays = windowInDays,
                GeneratedByUserId = GetCurrentActorUserId(),
                GeneratedByUserName = GetCurrentActorName(),
                Source = query.Source ?? "Admin.Backoffice",
                ExternalReference = query.ExternalReference,
                TotalProducts = query.IncludeProductMetrics ? products.Count : 0,
                ActiveProducts = query.IncludeProductMetrics ? products.Count(product => product.Activo) : 0,
                InactiveProducts = query.IncludeProductMetrics ? products.Count(product => !product.Activo) : 0,
                FeaturedProducts = query.IncludeProductMetrics ? products.Count(product => product.Destacado) : 0,
                AvailableProducts = query.IncludeProductMetrics ? products.Count(product => product.EstaDisponible()) : 0,
                UnavailableProducts = query.IncludeProductMetrics ? products.Count(product => !product.EstaDisponible()) : 0,
                OutOfStockProducts = query.IncludeProductMetrics ? products.Count(product => product.Stock <= 0) : 0,
                LowStockProducts = query.IncludeProductMetrics ? products.Count(product => product.Stock is > 0 && product.Stock <= lowStockThreshold) : 0,
                NewProductsInWindow = query.IncludeProductMetrics ? products.Count(product => product.FechaCreacionUtc >= windowStartUtc) : 0,
                PhysicalProducts = query.IncludeProductMetrics ? products.Count(product => product.TipoProducto == TipoProducto.Fisico) : 0,
                DigitalProducts = query.IncludeProductMetrics ? products.Count(product => product.TipoProducto == TipoProducto.Digital) : 0,
                TotalOrders = query.IncludeOrderMetrics ? orders.Count : 0,
                NewOrdersInWindow = query.IncludeOrderMetrics ? orders.Count(order => order.FechaCreacionUtc >= windowStartUtc) : 0,
                PendingOrders = query.IncludeOrderMetrics ? orders.Count(order => order.Estado == EstadoPedido.Pendiente) : 0,
                ConfirmedOrders = query.IncludeOrderMetrics ? orders.Count(order => order.Estado == EstadoPedido.Confirmado) : 0,
                PaidOrders = query.IncludeOrderMetrics ? orders.Count(order => order.Estado == EstadoPedido.Pagado) : 0,
                ProcessingOrders = query.IncludeOrderMetrics ? orders.Count(order => order.Estado == EstadoPedido.EnProceso) : 0,
                ShippedOrders = query.IncludeOrderMetrics ? orders.Count(order => order.Estado == EstadoPedido.Enviado) : 0,
                DeliveredOrders = query.IncludeOrderMetrics ? orders.Count(order => order.Estado == EstadoPedido.Entregado) : 0,
                CancelledOrders = query.IncludeOrderMetrics ? orders.Count(order => order.Estado == EstadoPedido.Cancelado) : 0,
                ActiveOrders = query.IncludeOrderMetrics ? orders.Count(order => !order.EstaFinalizado()) : 0,
                FinalizedOrders = query.IncludeOrderMetrics ? orders.Count(order => order.EstaFinalizado()) : 0,
                Currency = currency,
                TotalOrdersAmount = query.IncludeFinancialMetrics ? orders.Sum(order => order.Total.Amount) : 0m,
                OrdersAmountInWindow = query.IncludeFinancialMetrics ? orders.Where(order => order.FechaCreacionUtc >= windowStartUtc).Sum(order => order.Total.Amount) : 0m,
                PaidOrdersAmount = query.IncludeFinancialMetrics ? orders.Where(order => order.Estado == EstadoPedido.Pagado).Sum(order => order.Total.Amount) : 0m,
                DeliveredOrdersAmount = query.IncludeFinancialMetrics ? orders.Where(order => order.Estado == EstadoPedido.Entregado).Sum(order => order.Total.Amount) : 0m,
                CancelledOrdersAmount = query.IncludeFinancialMetrics ? orders.Where(order => order.Estado == EstadoPedido.Cancelado).Sum(order => order.Total.Amount) : 0m,
                TotalUsers = query.IncludeUserMetrics ? users.Count : 0,
                TotalCustomers = query.IncludeUserMetrics ? users.OfType<Cliente>().Count() : 0,
                TotalAdministrators = query.IncludeUserMetrics ? users.OfType<Administrador>().Count() : 0,
                ActiveUsers = query.IncludeUserMetrics ? users.Count(user => user.Activo) : 0,
                InactiveUsers = query.IncludeUserMetrics ? users.Count(user => !user.Activo) : 0,
                EmailConfirmedUsers = query.IncludeUserMetrics ? users.Count(user => user.CorreoConfirmado) : 0,
                NewUsersInWindow = query.IncludeUserMetrics ? users.Count(user => user.FechaCreacionUtc >= windowStartUtc) : 0,
                UsersWithRecentAccess = query.IncludeUserMetrics ? users.Count(user => user.FechaUltimoAccesoUtc >= windowStartUtc) : 0,
                HasOutOfStockAlerts = query.IncludeOperationalAlerts && products.Any(product => product.Stock <= 0),
                HasLowStockAlerts = query.IncludeOperationalAlerts && products.Any(product => product.Stock is > 0 && product.Stock <= lowStockThreshold),
                HasOperationalBacklog = query.IncludeOperationalAlerts && orders.Any(order => !order.EstaFinalizado()),
                ActiveCarts = query.IncludeOperationalAlerts ? carts.Count(cart => cart.Activo) : 0,
                AuditEventsLast24Hours = query.IncludeOperationalAlerts ? recentAuditWindow.TotalCount : 0,
                RecentActivities = query.IncludeOperationalAlerts
                    ? recentActivity.Items
                        .Select(entry => new AdminDashboardRecentActivityDto
                        {
                            OccurredAtUtc = entry.OccurredAtUtc,
                            Module = entry.Module,
                            Action = entry.Action,
                            Detail = entry.Detail,
                            PerformedBy = entry.PerformedBy
                        })
                        .ToArray()
                    : Array.Empty<AdminDashboardRecentActivityDto>()
            });
        }, "Admin.Dashboard");
    }

    /// <inheritdoc />
    public async Task<Result<AdminUsersBackofficeDto>> GetUsersAsync(
        GetAdminUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        Error? authorizationError = await EnsureUsersBackofficeAccessAsync(query, cancellationToken).ConfigureAwait(false);
        if (authorizationError is not null)
        {
            return Result.Failure<AdminUsersBackofficeDto>(authorizationError);
        }

        Task<Result<AdminUsersBackofficeDto>> operation = ExecuteAsync(async () =>
        {
            DateTime generatedAtUtc = query.ReferenceDateUtc ?? DateTime.UtcNow;
            DateTime recentAccessWindowStartUtc = generatedAtUtc.AddDays(-query.NormalizedRecentAccessWindowInDays);

        IReadOnlyCollection<Usuario> users = await GetUsersForBackofficeAsync(query, cancellationToken).ConfigureAwait(false);

            AdminBackofficeUserDto[] projectedUsers = users
                .Where(user => !query.OnlyActiveUsers || user.Activo)
                .Where(user => !query.OnlyAdministrativeUsers || user is Administrador)
                .Select(MapToBackofficeUser)
                .OrderByDescending(user => user.IsAdministrative)
                .ThenByDescending(user => user.IsSuperUser)
                .ThenBy(user => user.Name, StringComparer.Ordinal)
                .ToArray();

            return Result.Success(new AdminUsersBackofficeDto
            {
                GeneratedAtUtc = generatedAtUtc,
                GeneratedByUserId = GetCurrentActorUserId(),
                GeneratedByUserName = GetCurrentActorName(),
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

        return await operation.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<AdminBackofficeUserDto>> ResetUserPasswordAsync(
        ResetUserPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Error? validationError = await ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (validationError is not null)
        {
            return Result.Failure<AdminBackofficeUserDto>(validationError);
        }

        Error? authorizationError = await EnsureAuthenticatedSuperUserActorAsync(
            "Admin.AuthenticationRequired",
            "Se requiere una sesión autenticada para restablecer contraseñas de usuarios.",
            "Admin.SuperUserRequiredForUserPasswordReset",
            "Solo un super usuario puede restablecer contraseñas de usuarios.",
            cancellationToken).ConfigureAwait(false);

        if (authorizationError is not null)
        {
            return Result.Failure<AdminBackofficeUserDto>(authorizationError);
        }

        return await ExecuteAsync(async () =>
        {
            Usuario? user = await _userRepository.GetByIdAsync(command.TargetUserId, cancellationToken).ConfigureAwait(false);
            if (user is null)
            {
                return Result.Failure<AdminBackofficeUserDto>(
                    Error.NotFound("Admin.UserNotFound", $"No se encontró el usuario con identificador '{command.TargetUserId}'."));
            }

            string passwordHash = _passwordHasher.HashPassword(command.NewPassword);
            user.CambiarContrasenaHash(passwordHash);

            try
            {
                await _userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await AuditUserPasswordResetEventAsync(user, command, cancellationToken).ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
                return Result.Failure<AdminBackofficeUserDto>(
                    Error.Failure("Admin.UserPasswordResetPersistence", "No fue posible completar el restablecimiento administrativo de la contraseña."));
            }

            return Result.Success(MapToBackofficeUser(user));
        }, "Admin.UserPasswordReset").ConfigureAwait(false);
    }

    private async Task<IReadOnlyCollection<Usuario>> GetUsersForBackofficeAsync(
        GetAdminUsersQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.OnlyAdministrativeUsers)
        {
            IReadOnlyCollection<Administrador> administrators = await _userRepository
                .GetAdministratorsAsync(cancellationToken)
                .ConfigureAwait(false);

            return administrators.Cast<Usuario>().ToArray();
        }

        return await _userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }

    private static Email CreateEmail(string value)
    {
        return new Email(value);
    }

    private static AdminBackofficeUserDto MapToBackofficeUser(Usuario user)
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

    private Task<Error?> ValidateAsync(RegisterAdminCommand command, CancellationToken cancellationToken)
    {
        return ApplicationExecution.ValidateAsync(
            command,
            _registerAdminCommandValidator,
            "Admin.Validation",
            "La solicitud contiene errores de validación.",
            cancellationToken);
    }

    private Task<Error?> ValidateAsync(ResetUserPasswordCommand command, CancellationToken cancellationToken)
    {
        return ApplicationExecution.ValidateAsync(
            command,
            _resetUserPasswordCommandValidator,
            "Admin.ResetUserPasswordValidation",
            "La solicitud contiene errores de validación.",
            cancellationToken);
    }

    private static Task<Result<TResponse>> ExecuteAsync<TResponse>(
        Func<Task<Result<TResponse>>> operation,
        string errorCode)
    {
        return ApplicationExecution.ExecuteAsync(operation, errorCode);
    }

    private Task AuditAdminEventAsync(
        Administrador admin,
        RegisterAdminCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(admin);
        ArgumentNullException.ThrowIfNull(command);

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
                ["createdByUserId"] = GetCurrentActorUserId()?.ToString() ?? "bootstrap",
                ["createdByRole"] = GetCurrentActorRole() ?? RolUsuario.SuperUsuario.ToString(),
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
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(command);

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
                ["resetByUserId"] = GetCurrentActorUserId()?.ToString() ?? string.Empty,
                ["resetByRole"] = GetCurrentActorRole() ?? string.Empty,
                ["source"] = command.Source ?? "Admin.Backoffice.Users",
                ["externalReference"] = command.ExternalReference ?? string.Empty,
                ["reason"] = command.Reason ?? string.Empty
            },
            cancellationToken);
    }

    private Guid? GetCurrentActorUserId()
    {
        return _currentUserService.IsAuthenticated
            ? _currentUserService.UserId
            : null;
    }

    private string? GetCurrentActorName()
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return null;
        }

        return _currentUserService.UserName ?? _currentUserService.Email;
    }

    private string? GetCurrentActorRole()
    {
        return _currentUserService.IsAuthenticated
            ? _currentUserService.Role
            : null;
    }

    private async Task<Error?> EnsureAdministrativeRegistrationIsAuthorizedAsync(
        RegisterAdminCommand command,
        CancellationToken cancellationToken)
    {
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

    private Task<Error?> EnsureUsersBackofficeAccessAsync(GetAdminUsersQuery query, CancellationToken cancellationToken)
    {
        return EnsureSuperUserBackofficeAccessAsync(
            query.RequireSuperUserAccess,
            "Admin.AuthenticationRequired",
            "Se requiere una sesión autenticada para consultar el backoffice de usuarios.",
            "Admin.SuperUserRequiredForUsersBackoffice",
            "Solo un super usuario puede consultar el backoffice de usuarios.",
            cancellationToken);
    }

    private async Task<Error?> EnsureSuperUserBackofficeAccessAsync(
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

    private async Task<Error?> EnsureAuthenticatedSuperUserActorAsync(
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

        Administrador? actor = await _userRepository
            .GetAdministratorByIdAsync(actorUserId.Value, cancellationToken)
            .ConfigureAwait(false);

        return actor is { EsSuperUsuario: true } && actor.EstaHabilitado()
            ? null
            : Error.Unauthorized(authorizationErrorCode, authorizationErrorMessage);
    }

    private Task<bool> SuperUserExistsAsync(CancellationToken cancellationToken)
    {
        return _userRepository.ExistsByRoleAsync(RolUsuario.SuperUsuario, cancellationToken);
    }
}
