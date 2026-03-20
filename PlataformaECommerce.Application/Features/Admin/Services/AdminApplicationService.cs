using FluentValidation;
using PlataformaECommerce.Application.Common.Execution;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Mappings;
using PlataformaECommerce.Application.Features.Admin.Queries;
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
        IValidator<RegisterAdminCommand> registerAdminCommandValidator)
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

            await _userRepository.AddAsync(admin, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await AuditAdminEventAsync(admin, cancellationToken).ConfigureAwait(false);

            return Result.Success(admin.ToAdminDto());
        }, "Admin.Domain").ConfigureAwait(false);
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
                GeneratedByUserId = _currentUserService.UserId ?? query.RequestedByUserId,
                GeneratedByUserName = _currentUserService.UserName ?? _currentUserService.Email ?? query.RequestedByUserName,
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

    private static Email CreateEmail(string value)
    {
        return new Email(value);
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

    private static Task<Result<TResponse>> ExecuteAsync<TResponse>(
        Func<Task<Result<TResponse>>> operation,
        string errorCode)
    {
        return ApplicationExecution.ExecuteAsync(operation, errorCode);
    }

    private Task AuditAdminEventAsync(Administrador admin, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(admin);

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
                ["isEmailConfirmed"] = admin.CorreoConfirmado.ToString()
            },
            cancellationToken);
    }
}
