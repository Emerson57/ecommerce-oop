using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin.DTOs;

namespace PlataformaECommerce.Application.Features.Admin.Queries;

/// <summary>
/// Representa la consulta de aplicación para obtener el tablero administrativo
/// consolidado del e-Commerce.
/// </summary>
/// <remarks>
/// Esta query modela una intención explícita de lectura dentro del módulo
/// administrativo, correspondiente al caso de uso de consultar indicadores
/// operativos, comerciales y de seguimiento general de la plataforma.
///
/// Su responsabilidad es transportar los criterios mínimos necesarios para que
/// la capa Application construya una proyección de alto nivel en forma de
/// <see cref="AdminDashboardDto"/>, desacoplada del dominio y de la infraestructura.
///
/// Esta consulta está preparada para soportar escenarios como:
/// - panel principal de administración,
/// - monitoreo diario de operación,
/// - tableros ejecutivos,
/// - seguimiento de indicadores recientes,
/// - revisiones rápidas por parte de soporte o gerencia,
/// - y control operacional del e-Commerce.
///
/// Esta clase no debe contener lógica de negocio ni acceso a datos.
/// Dichas responsabilidades corresponden al servicio de aplicación
/// especializado que procese la consulta.
/// </remarks>
public sealed class GetAdminDashboardQuery
{
    #region Constantes

    /// <summary>
    /// Ventana temporal por defecto, expresada en días.
    /// </summary>
    private const int DefaultWindowInDays = 30;

    /// <summary>
    /// Ventana temporal mínima permitida.
    /// </summary>
    private const int MinWindowInDays = 1;

    /// <summary>
    /// Ventana temporal máxima permitida.
    /// </summary>
    private const int MaxWindowInDays = 365;

    /// <summary>
    /// Umbral de stock bajo por defecto utilizado por el tablero.
    /// </summary>
    private const int DefaultLowStockThreshold = 5;

    /// <summary>
    /// Umbral mínimo permitido para stock bajo.
    /// </summary>
    private const int MinLowStockThreshold = 0;

    /// <summary>
    /// Umbral máximo permitido para stock bajo.
    /// </summary>
    private const int MaxLowStockThreshold = 1000;

    #endregion

    #region Configuración temporal

    /// <summary>
    /// Cantidad de días que deben considerarse para las métricas recientes.
    /// </summary>
    public int WindowInDays { get; init; } = DefaultWindowInDays;

    /// <summary>
    /// Fecha UTC de referencia para construir el tablero, cuando la capa superior
    /// desee controlarla explícitamente.
    /// </summary>
    /// <remarks>
    /// Si no se informa, el servicio de aplicación puede utilizar una fuente
    /// de tiempo controlada a través de una abstracción temporal.
    /// </remarks>
    public DateTime? ReferenceDateUtc { get; init; }

    #endregion

    #region Configuración operativa del tablero

    /// <summary>
    /// Umbral utilizado para considerar que un producto tiene inventario bajo.
    /// </summary>
    public int LowStockThreshold { get; init; } = DefaultLowStockThreshold;

    /// <summary>
    /// Indica si deben incluirse métricas de usuarios en el tablero.
    /// </summary>
    public bool IncludeUserMetrics { get; init; } = true;

    /// <summary>
    /// Indica si deben incluirse métricas de productos en el tablero.
    /// </summary>
    public bool IncludeProductMetrics { get; init; } = true;

    /// <summary>
    /// Indica si deben incluirse métricas de pedidos en el tablero.
    /// </summary>
    public bool IncludeOrderMetrics { get; init; } = true;

    /// <summary>
    /// Indica si deben incluirse métricas financieras resumidas.
    /// </summary>
    public bool IncludeFinancialMetrics { get; init; } = true;

    /// <summary>
    /// Indica si deben incluirse señales operativas o alertas resumidas.
    /// </summary>
    public bool IncludeOperationalAlerts { get; init; } = true;

    #endregion

    #region Seguridad y contexto

    /// <summary>
    /// Indica si la consulta exige que el usuario actual posea acceso administrativo.
    /// </summary>
    public bool RequireAdministratorAccess { get; init; } = true;

    /// <summary>
    /// Identificador opcional del usuario que origina la consulta.
    /// </summary>
    /// <remarks>
    /// Este valor puede utilizarse con fines de trazabilidad o validación adicional
    /// respecto al contexto autenticado actual.
    /// </remarks>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Nombre visible opcional del usuario que origina la consulta.
    /// </summary>
    public string? RequestedByUserName { get; init; }

    /// <summary>
    /// Canal de origen desde el cual se solicita el tablero.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - AdminPortal
    /// - InternalDashboard
    /// - BackOffice
    /// - MonitoringPanel
    /// </remarks>
    public string? Source { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada a la consulta.
    /// </summary>
    public string? ExternalReference { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Obtiene la ventana temporal normalizada.
    /// </summary>
    public int NormalizedWindowInDays
    {
        get
        {
            if (WindowInDays < MinWindowInDays)
            {
                return DefaultWindowInDays;
            }

            return WindowInDays > MaxWindowInDays
                ? MaxWindowInDays
                : WindowInDays;
        }
    }

    /// <summary>
    /// Obtiene el umbral de stock bajo normalizado.
    /// </summary>
    public int NormalizedLowStockThreshold
    {
        get
        {
            if (LowStockThreshold < MinLowStockThreshold)
            {
                return DefaultLowStockThreshold;
            }

            return LowStockThreshold > MaxLowStockThreshold
                ? MaxLowStockThreshold
                : LowStockThreshold;
        }
    }

    /// <summary>
    /// Indica si la consulta incluye al menos una sección métrica habilitada.
    /// </summary>
    public bool HasAnyMetricEnabled =>
        IncludeUserMetrics ||
        IncludeProductMetrics ||
        IncludeOrderMetrics ||
        IncludeFinancialMetrics ||
        IncludeOperationalAlerts;

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida de la consulta del tablero administrativo.
    /// </summary>
    /// <returns>Cadena representativa de la query.</returns>
    public override string ToString()
    {
        return $"GetAdminDashboardQuery | WindowInDays: {NormalizedWindowInDays} | LowStockThreshold: {NormalizedLowStockThreshold} | RequireAdministratorAccess: {RequireAdministratorAccess} | Source: {Source} | ExternalReference: {ExternalReference}";
    }

    #endregion
}