using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Admin.DTOs;

/// <summary>
/// Representa la vista consolidada del tablero administrativo del e-Commerce.
/// </summary>
/// <remarks>
/// Este DTO se utiliza para transportar, desde la capa Application hacia capas
/// superiores, un resumen ejecutivo y operativo del estado actual de la plataforma.
///
/// Su propósito es centralizar en una sola respuesta la información más relevante
/// para escenarios como:
/// - paneles de administración,
/// - vistas de operación diaria,
/// - monitoreo gerencial,
/// - tableros de control,
/// - seguimiento comercial,
/// - y revisión rápida del comportamiento general del sistema.
///
/// La estructura está pensada para exponer indicadores agregados y métricas
/// derivadas, sin filtrar directamente entidades del dominio ni detalles internos
/// de persistencia. Por este motivo, la clase se organiza en secciones lógicas:
/// - contexto de generación,
/// - métricas de usuarios,
/// - métricas de productos,
/// - métricas de pedidos,
/// - métricas financieras resumidas,
/// - y señales operativas de atención.
///
/// Esta clase no debe contener lógica de negocio compleja. Los cálculos y reglas
/// de composición deben realizarse en servicios de aplicación o
/// componentes especializados de agregación.
/// </remarks>
public sealed class AdminDashboardDto
{
    #region Contexto del tablero

    /// <summary>
    /// Fecha y hora UTC en la que fue generado el tablero.
    /// </summary>
    public DateTime GeneratedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC inicial de la ventana temporal utilizada
    /// para calcular métricas recientes.
    /// </summary>
    public DateTime WindowStartUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC final de la ventana temporal utilizada
    /// para calcular métricas recientes.
    /// </summary>
    public DateTime WindowEndUtc { get; init; }

    /// <summary>
    /// Cantidad de días considerados para la ventana analítica del tablero.
    /// </summary>
    public int WindowInDays { get; init; }

    /// <summary>
    /// Identificador del usuario que generó la consulta, cuando esté disponible.
    /// </summary>
    public Guid? GeneratedByUserId { get; init; }

    /// <summary>
    /// Nombre visible del usuario que generó el tablero, cuando esté disponible.
    /// </summary>
    public string? GeneratedByUserName { get; init; }

    /// <summary>
    /// Canal de origen desde el cual se solicitó el tablero.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada a la consulta del tablero.
    /// </summary>
    public string? ExternalReference { get; init; }

    #endregion

    #region Métricas de usuarios

    /// <summary>
    /// Cantidad total de usuarios registrados en el sistema.
    /// </summary>
    public int TotalUsers { get; init; }

    /// <summary>
    /// Cantidad total de clientes registrados.
    /// </summary>
    public int TotalCustomers { get; init; }

    /// <summary>
    /// Cantidad total de administradores registrados.
    /// </summary>
    public int TotalAdministrators { get; init; }

    /// <summary>
    /// Cantidad total de usuarios activos.
    /// </summary>
    public int ActiveUsers { get; init; }

    /// <summary>
    /// Cantidad total de usuarios inactivos.
    /// </summary>
    public int InactiveUsers { get; init; }

    /// <summary>
    /// Cantidad total de usuarios con correo confirmado.
    /// </summary>
    public int EmailConfirmedUsers { get; init; }

    /// <summary>
    /// Cantidad de usuarios creados dentro de la ventana analítica.
    /// </summary>
    public int NewUsersInWindow { get; init; }

    /// <summary>
    /// Cantidad de usuarios con acceso registrado dentro de la ventana analítica.
    /// </summary>
    public int UsersWithRecentAccess { get; init; }

    #endregion

    #region Métricas de productos

    /// <summary>
    /// Cantidad total de productos registrados en el sistema.
    /// </summary>
    public int TotalProducts { get; init; }

    /// <summary>
    /// Cantidad total de productos activos.
    /// </summary>
    public int ActiveProducts { get; init; }

    /// <summary>
    /// Cantidad total de productos inactivos.
    /// </summary>
    public int InactiveProducts { get; init; }

    /// <summary>
    /// Cantidad total de productos destacados.
    /// </summary>
    public int FeaturedProducts { get; init; }

    /// <summary>
    /// Cantidad total de productos disponibles comercialmente.
    /// </summary>
    public int AvailableProducts { get; init; }

    /// <summary>
    /// Cantidad total de productos no disponibles comercialmente.
    /// </summary>
    public int UnavailableProducts { get; init; }

    /// <summary>
    /// Cantidad total de productos sin existencias.
    /// </summary>
    public int OutOfStockProducts { get; init; }

    /// <summary>
    /// Cantidad total de productos con inventario bajo.
    /// </summary>
    /// <remarks>
    /// El umbral de inventario bajo es definido por la capa Application
    /// al momento de construir el tablero.
    /// </remarks>
    public int LowStockProducts { get; init; }

    /// <summary>
    /// Cantidad de productos creados dentro de la ventana analítica.
    /// </summary>
    public int NewProductsInWindow { get; init; }

    /// <summary>
    /// Cantidad de productos físicos.
    /// </summary>
    public int PhysicalProducts { get; init; }

    /// <summary>
    /// Cantidad de productos digitales.
    /// </summary>
    public int DigitalProducts { get; init; }

    #endregion

    #region Métricas de pedidos

    /// <summary>
    /// Cantidad total de pedidos registrados en el sistema.
    /// </summary>
    public int TotalOrders { get; init; }

    /// <summary>
    /// Cantidad de pedidos creados dentro de la ventana analítica.
    /// </summary>
    public int NewOrdersInWindow { get; init; }

    /// <summary>
    /// Cantidad total de pedidos pendientes.
    /// </summary>
    public int PendingOrders { get; init; }

    /// <summary>
    /// Cantidad total de pedidos confirmados.
    /// </summary>
    public int ConfirmedOrders { get; init; }

    /// <summary>
    /// Cantidad total de pedidos pagados.
    /// </summary>
    public int PaidOrders { get; init; }

    /// <summary>
    /// Cantidad total de pedidos en proceso.
    /// </summary>
    public int ProcessingOrders { get; init; }

    /// <summary>
    /// Cantidad total de pedidos enviados.
    /// </summary>
    public int ShippedOrders { get; init; }

    /// <summary>
    /// Cantidad total de pedidos entregados.
    /// </summary>
    public int DeliveredOrders { get; init; }

    /// <summary>
    /// Cantidad total de pedidos cancelados.
    /// </summary>
    public int CancelledOrders { get; init; }

    /// <summary>
    /// Cantidad total de pedidos activos, es decir,
    /// aquellos que aún no han finalizado.
    /// </summary>
    public int ActiveOrders { get; init; }

    /// <summary>
    /// Cantidad total de pedidos finalizados.
    /// </summary>
    public int FinalizedOrders { get; init; }

    #endregion

    #region Métricas financieras resumidas

    /// <summary>
    /// Código de moneda base utilizado para las métricas monetarias del tablero.
    /// </summary>
    public string Currency { get; init; } = "COP";

    /// <summary>
    /// Monto total acumulado de todos los pedidos registrados.
    /// </summary>
    public decimal TotalOrdersAmount { get; init; }

    /// <summary>
    /// Monto total de los pedidos creados dentro de la ventana analítica.
    /// </summary>
    public decimal OrdersAmountInWindow { get; init; }

    /// <summary>
    /// Monto total asociado a pedidos pagados.
    /// </summary>
    public decimal PaidOrdersAmount { get; init; }

    /// <summary>
    /// Monto total asociado a pedidos entregados.
    /// </summary>
    public decimal DeliveredOrdersAmount { get; init; }

    /// <summary>
    /// Monto total asociado a pedidos cancelados.
    /// </summary>
    public decimal CancelledOrdersAmount { get; init; }

    #endregion

    #region Señales operativas

    /// <summary>
    /// Indica si actualmente existen productos agotados que requieren atención.
    /// </summary>
    public bool HasOutOfStockAlerts { get; init; }

    /// <summary>
    /// Indica si actualmente existen productos con inventario bajo.
    /// </summary>
    public bool HasLowStockAlerts { get; init; }

    /// <summary>
    /// Indica si actualmente existen pedidos activos pendientes de atención.
    /// </summary>
    public bool HasOperationalBacklog { get; init; }

    /// <summary>
    /// Cantidad total de carritos activos registrados actualmente.
    /// </summary>
    public int ActiveCarts { get; init; }

    /// <summary>
    /// Cantidad de eventos de auditoría registrados durante las últimas 24 horas.
    /// </summary>
    public int AuditEventsLast24Hours { get; init; }

    /// <summary>
    /// Colección de actividades recientes provenientes del rastro transversal de auditoría.
    /// </summary>
    public IReadOnlyCollection<AdminDashboardRecentActivityDto> RecentActivities { get; init; } = Array.Empty<AdminDashboardRecentActivityDto>();

    #endregion

    #region Indicadores calculados

    /// <summary>
    /// Obtiene la tasa de usuarios activos respecto del total de usuarios.
    /// </summary>
    public decimal ActiveUsersRate =>
        TotalUsers == 0
            ? 0m
            : decimal.Round((decimal)ActiveUsers / TotalUsers * 100m, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Obtiene la tasa de productos disponibles respecto del total de productos.
    /// </summary>
    public decimal ProductAvailabilityRate =>
        TotalProducts == 0
            ? 0m
            : decimal.Round((decimal)AvailableProducts / TotalProducts * 100m, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Obtiene la tasa de cancelación respecto del total de pedidos.
    /// </summary>
    public decimal OrderCancellationRate =>
        TotalOrders == 0
            ? 0m
            : decimal.Round((decimal)CancelledOrders / TotalOrders * 100m, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Obtiene la tasa de entrega respecto del total de pedidos.
    /// </summary>
    public decimal OrderDeliveryRate =>
        TotalOrders == 0
            ? 0m
            : decimal.Round((decimal)DeliveredOrders / TotalOrders * 100m, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Obtiene el ticket promedio general del sistema.
    /// </summary>
    public decimal AverageOrderAmount =>
        TotalOrders == 0
            ? 0m
            : decimal.Round(TotalOrdersAmount / TotalOrders, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Obtiene el ticket promedio de la ventana analítica.
    /// </summary>
    public decimal AverageOrderAmountInWindow =>
        NewOrdersInWindow == 0
            ? 0m
            : decimal.Round(OrdersAmountInWindow / NewOrdersInWindow, 2, MidpointRounding.AwayFromZero);

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del tablero administrativo.
    /// </summary>
    /// <returns>Cadena representativa del DTO.</returns>
    public override string ToString()
    {
        return $"AdminDashboardDto | GeneratedAtUtc: {GeneratedAtUtc:O} | TotalUsers: {TotalUsers} | TotalProducts: {TotalProducts} | TotalOrders: {TotalOrders} | DeliveredOrders: {DeliveredOrders} | CancelledOrders: {CancelledOrders} | Currency: {Currency}";
    }

    #endregion
}