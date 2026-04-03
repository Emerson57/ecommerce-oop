using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Queries;
using PlataformaECommerce.Application.Interfaces.Repositories.Audit;
using PlataformaECommerce.Application.Interfaces.Repositories.Cart;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Admin;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Admin.Services;

/// <summary>
/// Orquesta el dashboard analítico y operativo del backoffice.
/// </summary>
public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="AdminDashboardService"/>.
    /// </summary>
    public AdminDashboardService(
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        ICartRepository cartRepository,
        IAuditRepository auditRepository,
        ICurrentUserService currentUserService)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _auditRepository = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    /// <inheritdoc />
    public Task<Result<AdminDashboardDto>> GetDashboardAsync(GetAdminDashboardQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return AdminServiceSupport.ExecuteAsync(async () =>
        {
            DateTime windowEndUtc = query.ReferenceDateUtc ?? DateTime.UtcNow;
            int windowInDays = query.NormalizedWindowInDays;
            DateTime windowStartUtc = windowEndUtc.AddDays(-windowInDays);

            IReadOnlyCollection<Producto> products = await _productRepository.GetAllAsync(cancellationToken);
            IReadOnlyCollection<Pedido> orders = await _orderRepository.GetAllAsync(cancellationToken);
            IReadOnlyCollection<Usuario> users = await _userRepository.GetAllAsync(cancellationToken);
            IReadOnlyCollection<CarritoCompra> carts = await _cartRepository.GetAllAsync(cancellationToken);

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
                cancellationToken);

            AuditSearchResult recentActivity = await _auditRepository.SearchAsync(
                new AuditSearchFilter
                {
                    PageNumber = 1,
                    PageSize = 5,
                    SortDescending = true
                },
                cancellationToken);

            int lowStockThreshold = query.NormalizedLowStockThreshold;

            return Result.Success(new AdminDashboardDto
            {
                GeneratedAtUtc = windowEndUtc,
                WindowStartUtc = windowStartUtc,
                WindowEndUtc = windowEndUtc,
                WindowInDays = windowInDays,
                GeneratedByUserId = _currentUserService.IsAuthenticated ? _currentUserService.UserId : null,
                GeneratedByUserName = _currentUserService.IsAuthenticated ? _currentUserService.UserName ?? _currentUserService.Email : null,
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
}
