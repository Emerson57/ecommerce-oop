using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Queries;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Admin.Services;

/// <summary>
/// Proporciona los casos de uso de aplicación relacionados con consultas
/// administrativas y tableros de control del e-Commerce.
/// </summary>
/// <remarks>
/// Esta clase actúa como servicio de aplicación para construir vistas
/// consolidadas del contexto administrativo del sistema, coordinando:
///
/// - acceso de solo lectura a repositorios,
/// - validaciones de seguridad del contexto actual,
/// - agregación de métricas,
/// - composición de indicadores,
/// - y proyección hacia DTOs de alto nivel.
///
/// Su objetivo es ofrecer una forma profesional, mantenible y desacoplada
/// de construir tableros ejecutivos y operativos sin exponer directamente
/// entidades del dominio ni detalles de infraestructura.
///
/// Este servicio no reemplaza una futura implementación basada en handlers
/// CQRS, pero constituye una capa de orquestación sólida para el módulo
/// administrativo.
/// </remarks>
public sealed class AdminApplicationService
{
    #region Campos privados

    /// <summary>
    /// Repositorio de usuarios.
    /// </summary>
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Repositorio de productos.
    /// </summary>
    private readonly IProductRepository _productRepository;

    /// <summary>
    /// Repositorio de pedidos.
    /// </summary>
    private readonly IOrderRepository _orderRepository;

    /// <summary>
    /// Servicio que expone el usuario actualmente autenticado.
    /// </summary>
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Servicio de tiempo controlado para la capa Application.
    /// </summary>
    private readonly IDateTimeProvider _dateTimeProvider;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="AdminApplicationService"/>.
    /// </summary>
    /// <param name="userRepository">Repositorio de usuarios.</param>
    /// <param name="productRepository">Repositorio de productos.</param>
    /// <param name="orderRepository">Repositorio de pedidos.</param>
    /// <param name="currentUserService">Servicio del usuario actual.</param>
    /// <param name="dateTimeProvider">Servicio de tiempo.</param>
    public AdminApplicationService(
        IUserRepository userRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    #endregion

    #region Casos de uso públicos

    /// <summary>
    /// Obtiene el tablero administrativo consolidado del sistema.
    /// </summary>
    /// <param name="query">Consulta del tablero administrativo.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con el tablero consolidado cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<AdminDashboardDto>> GetAdminDashboardAsync(
        GetAdminDashboardQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        Error? validationError = ValidateDashboardQuery(query);
        if (validationError is not null)
        {
            return Result.Failure<AdminDashboardDto>(validationError);
        }

        Error? authorizationError = ValidateDashboardAccess(query);
        if (authorizationError is not null)
        {
            return Result.Failure<AdminDashboardDto>(authorizationError);
        }

        DateTime referenceDateUtc = query.ReferenceDateUtc ?? _dateTimeProvider.UtcNow;
        DateTime windowStartUtc = referenceDateUtc.AddDays(-query.NormalizedWindowInDays);

        IReadOnlyCollection<Usuario> users = query.IncludeUserMetrics || query.IncludeOperationalAlerts
            ? await _userRepository.GetAllAsync(cancellationToken)
            : Array.Empty<Usuario>();

        IReadOnlyCollection<Cliente> customers = query.IncludeUserMetrics
            ? await _userRepository.GetCustomersAsync(cancellationToken)
            : Array.Empty<Cliente>();

        IReadOnlyCollection<Administrador> administrators = query.IncludeUserMetrics
            ? await _userRepository.GetAdministratorsAsync(cancellationToken)
            : Array.Empty<Administrador>();

        IReadOnlyCollection<Producto> products = query.IncludeProductMetrics || query.IncludeOrderMetrics || query.IncludeFinancialMetrics || query.IncludeOperationalAlerts
            ? await _productRepository.GetAllAsync(cancellationToken)
            : Array.Empty<Producto>();

        IReadOnlyCollection<Pedido> orders = query.IncludeOrderMetrics || query.IncludeFinancialMetrics || query.IncludeOperationalAlerts
            ? await _orderRepository.GetAllAsync(cancellationToken)
            : Array.Empty<Pedido>();

        AdminDashboardDto dashboard = new()
        {
            GeneratedAtUtc = referenceDateUtc,
            WindowStartUtc = windowStartUtc,
            WindowEndUtc = referenceDateUtc,
            WindowInDays = query.NormalizedWindowInDays,
            GeneratedByUserId = query.RequestedByUserId ?? _currentUserService.UserId,
            GeneratedByUserName = query.RequestedByUserName ?? _currentUserService.UserName,
            Source = query.Source,
            ExternalReference = query.ExternalReference,

            TotalUsers = query.IncludeUserMetrics ? users.Count : 0,
            TotalCustomers = query.IncludeUserMetrics ? customers.Count : 0,
            TotalAdministrators = query.IncludeUserMetrics ? administrators.Count : 0,
            ActiveUsers = query.IncludeUserMetrics ? users.Count(user => user.Activo) : 0,
            InactiveUsers = query.IncludeUserMetrics ? users.Count(user => !user.Activo) : 0,
            EmailConfirmedUsers = query.IncludeUserMetrics ? users.Count(user => user.CorreoConfirmado) : 0,
            NewUsersInWindow = query.IncludeUserMetrics ? users.Count(user => user.FechaCreacionUtc >= windowStartUtc && user.FechaCreacionUtc <= referenceDateUtc) : 0,
            UsersWithRecentAccess = query.IncludeUserMetrics ? users.Count(user => user.FechaUltimoAccesoUtc.HasValue && user.FechaUltimoAccesoUtc.Value >= windowStartUtc && user.FechaUltimoAccesoUtc.Value <= referenceDateUtc) : 0,

            TotalProducts = query.IncludeProductMetrics ? products.Count : 0,
            ActiveProducts = query.IncludeProductMetrics ? products.Count(product => product.Activo) : 0,
            InactiveProducts = query.IncludeProductMetrics ? products.Count(product => !product.Activo) : 0,
            FeaturedProducts = query.IncludeProductMetrics ? products.Count(product => product.Destacado) : 0,
            AvailableProducts = query.IncludeProductMetrics ? products.Count(product => product.EstaDisponible()) : 0,
            UnavailableProducts = query.IncludeProductMetrics ? products.Count(product => !product.EstaDisponible()) : 0,
            OutOfStockProducts = query.IncludeProductMetrics ? products.Count(product => !product.TieneStock()) : 0,
            LowStockProducts = query.IncludeProductMetrics ? products.Count(product => product.Stock > 0 && product.Stock <= query.NormalizedLowStockThreshold) : 0,
            NewProductsInWindow = query.IncludeProductMetrics ? products.Count(product => product.FechaCreacionUtc >= windowStartUtc && product.FechaCreacionUtc <= referenceDateUtc) : 0,
            PhysicalProducts = query.IncludeProductMetrics ? products.Count(product => product.TipoProducto == TipoProducto.Fisico) : 0,
            DigitalProducts = query.IncludeProductMetrics ? products.Count(product => product.TipoProducto == TipoProducto.Digital) : 0,

            TotalOrders = query.IncludeOrderMetrics ? orders.Count : 0,
            NewOrdersInWindow = query.IncludeOrderMetrics ? orders.Count(order => order.FechaCreacionUtc >= windowStartUtc && order.FechaCreacionUtc <= referenceDateUtc) : 0,
            PendingOrders = query.IncludeOrderMetrics ? orders.Count(order => order.Estado == EstadoPedido.Pendiente) : 0,
            ConfirmedOrders = query.IncludeOrderMetrics ? orders.Count(order => order.Estado == EstadoPedido.Confirmado) : 0,
            PaidOrders = query.IncludeOrderMetrics ? orders.Count(order => order.Estado == EstadoPedido.Pagado) : 0,
            ProcessingOrders = query.IncludeOrderMetrics ? orders.Count(order => order.Estado == EstadoPedido.EnProceso) : 0,
            ShippedOrders = query.IncludeOrderMetrics ? orders.Count(order => order.Estado == EstadoPedido.Enviado) : 0,
            DeliveredOrders = query.IncludeOrderMetrics ? orders.Count(order => order.Estado == EstadoPedido.Entregado) : 0,
            CancelledOrders = query.IncludeOrderMetrics ? orders.Count(order => order.Estado == EstadoPedido.Cancelado) : 0,
            ActiveOrders = query.IncludeOrderMetrics ? orders.Count(order => !order.EstaFinalizado()) : 0,
            FinalizedOrders = query.IncludeOrderMetrics ? orders.Count(order => order.EstaFinalizado()) : 0,

            Currency = ResolveDashboardCurrency(orders, products),
            TotalOrdersAmount = query.IncludeFinancialMetrics ? CalculateOrdersAmount(orders) : 0m,
            OrdersAmountInWindow = query.IncludeFinancialMetrics ? CalculateOrdersAmount(orders.Where(order => order.FechaCreacionUtc >= windowStartUtc && order.FechaCreacionUtc <= referenceDateUtc)) : 0m,
            PaidOrdersAmount = query.IncludeFinancialMetrics ? CalculateOrdersAmount(orders.Where(order => order.Estado == EstadoPedido.Pagado || order.Estado == EstadoPedido.EnProceso || order.Estado == EstadoPedido.Enviado || order.Estado == EstadoPedido.Entregado)) : 0m,
            DeliveredOrdersAmount = query.IncludeFinancialMetrics ? CalculateOrdersAmount(orders.Where(order => order.Estado == EstadoPedido.Entregado)) : 0m,
            CancelledOrdersAmount = query.IncludeFinancialMetrics ? CalculateOrdersAmount(orders.Where(order => order.Estado == EstadoPedido.Cancelado)) : 0m,

            HasOutOfStockAlerts = query.IncludeOperationalAlerts && products.Any(product => !product.TieneStock()),
            HasLowStockAlerts = query.IncludeOperationalAlerts && products.Any(product => product.Stock > 0 && product.Stock <= query.NormalizedLowStockThreshold),
            HasOperationalBacklog = query.IncludeOperationalAlerts && orders.Any(order =>
                order.Estado == EstadoPedido.Pendiente ||
                order.Estado == EstadoPedido.Confirmado ||
                order.Estado == EstadoPedido.Pagado ||
                order.Estado == EstadoPedido.EnProceso ||
                order.Estado == EstadoPedido.Enviado)
        };

        return Result.Success(dashboard);
    }

    #endregion

    #region Validaciones privadas

    /// <summary>
    /// Valida estructuralmente la consulta del tablero administrativo.
    /// </summary>
    /// <param name="query">Consulta a validar.</param>
    /// <returns>
    /// Un error de validación cuando la consulta es inválida;
    /// en caso contrario, <see langword="null"/>.
    /// </returns>
    private static Error? ValidateDashboardQuery(GetAdminDashboardQuery query)
    {
        if (!query.HasAnyMetricEnabled)
        {
            return Error.Validation(
                "AdminDashboard.NoMetricsEnabled",
                "La consulta del tablero debe habilitar al menos una sección métrica.");
        }

        if (query.RequestedByUserId.HasValue && query.RequestedByUserId.Value == Guid.Empty)
        {
            return Error.Validation(
                "AdminDashboard.InvalidRequestedByUserId",
                "El identificador del usuario solicitante no es válido.");
        }

        if (query.ReferenceDateUtc.HasValue && query.ReferenceDateUtc.Value.Kind != DateTimeKind.Utc)
        {
            return Error.Validation(
                "AdminDashboard.InvalidReferenceDateUtc",
                "La fecha de referencia del tablero debe estar expresada en UTC.");
        }

        return null;
    }

    /// <summary>
    /// Valida si el contexto actual tiene permisos suficientes para consultar el tablero.
    /// </summary>
    /// <param name="query">Consulta en proceso.</param>
    /// <returns>
    /// Un error de autorización cuando el acceso no está permitido;
    /// en caso contrario, <see langword="null"/>.
    /// </returns>
    private Error? ValidateDashboardAccess(GetAdminDashboardQuery query)
    {
        if (!query.RequireAdministratorAccess)
        {
            return null;
        }

        if (!_currentUserService.IsAuthenticated)
        {
            return Error.Unauthorized(
                "AdminDashboard.AuthenticationRequired",
                "Se requiere un usuario autenticado para consultar el tablero administrativo.");
        }

        bool isAdministrator = _currentUserService.IsInRole(RolUsuario.Administrador.ToString());
        if (!isAdministrator)
        {
            return Error.Unauthorized(
                "AdminDashboard.AdministratorRoleRequired",
                "La consulta del tablero administrativo requiere privilegios de administrador.");
        }

        if (query.RequestedByUserId.HasValue &&
            _currentUserService.UserId.HasValue &&
            query.RequestedByUserId.Value != _currentUserService.UserId.Value)
        {
            return Error.Unauthorized(
                "AdminDashboard.UserContextMismatch",
                "El usuario solicitante informado no coincide con el contexto autenticado actual.");
        }

        return null;
    }

    #endregion

    #region Métodos auxiliares de agregación

    /// <summary>
    /// Calcula el monto total de una colección de pedidos.
    /// </summary>
    /// <param name="orders">Pedidos a agregar.</param>
    /// <returns>Monto total acumulado.</returns>
    private static decimal CalculateOrdersAmount(IEnumerable<Pedido> orders)
    {
        ArgumentNullException.ThrowIfNull(orders);

        decimal total = 0m;

        foreach (Pedido order in orders)
        {
            total += order.Total.Amount;
        }

        return decimal.Round(total, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Determina la moneda base más apropiada para el tablero.
    /// </summary>
    /// <param name="orders">Colección de pedidos disponibles.</param>
    /// <param name="products">Colección de productos disponibles.</param>
    /// <returns>Código de moneda del tablero.</returns>
    private static string ResolveDashboardCurrency(
        IEnumerable<Pedido> orders,
        IEnumerable<Producto> products)
    {
        ArgumentNullException.ThrowIfNull(orders);
        ArgumentNullException.ThrowIfNull(products);

        string? orderCurrency = orders
            .Select(order => order.Total.Currency)
            .FirstOrDefault(currency => !string.IsNullOrWhiteSpace(currency));

        if (!string.IsNullOrWhiteSpace(orderCurrency))
        {
            return orderCurrency;
        }

        string? productCurrency = products
            .Select(product => product.Precio.Currency)
            .FirstOrDefault(currency => !string.IsNullOrWhiteSpace(currency));

        return string.IsNullOrWhiteSpace(productCurrency)
            ? "COP"
            : productCurrency;
    }

    #endregion
}