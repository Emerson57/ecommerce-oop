using System.ComponentModel.DataAnnotations;

namespace PlataformaECommerce.Infrastructure.Configurations;

/// <summary>
/// Representa la configuración requerida para integrar la pasarela de pagos Wompi.
/// </summary>
public sealed class WompiPaymentGatewaySettings
{
    /// <summary>
    /// Nombre de la sección de configuración asociada.
    /// </summary>
    public const string SectionName = "Payments:Wompi";

    /// <summary>
    /// Indica si la integración de pagos externos se encuentra habilitada.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Nombre lógico del proveedor configurado.
    /// </summary>
    public string ProviderName { get; set; } = "Wompi";

    /// <summary>
    /// URL base del checkout hospedado por el proveedor.
    /// </summary>
    public string CheckoutBaseUrl { get; set; } = "https://checkout.wompi.co/p/";

    /// <summary>
    /// URL base del endpoint de consulta de transacciones del proveedor.
    /// </summary>
    public string TransactionsApiBaseUrl { get; set; } = "https://production.wompi.co/v1/transactions/";

    /// <summary>
    /// Llave pública del comercio registrada en la pasarela.
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// Secreto de integridad utilizado para firmar la sesión del checkout.
    /// </summary>
    public string IntegritySecret { get; set; } = string.Empty;

    /// <summary>
    /// Indica si se debe usar el entorno sandbox del proveedor.
    /// </summary>
    public bool UseSandbox { get; set; }
}
