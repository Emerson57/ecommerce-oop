using System.ComponentModel.DataAnnotations;

namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Define la configuración centralizada de rate limiting para endpoints sensibles y públicos.
/// </summary>
public sealed class WebRateLimitingOptions
{
    /// <summary>
    /// Nombre de la sección de configuración.
    /// </summary>
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Nombre de la política de autenticación interactiva.
    /// </summary>
    public const string AuthFlowPolicyName = "auth-flow";

    /// <summary>
    /// Nombre de la política aplicada a APIs administrativas sensibles.
    /// </summary>
    public const string SensitiveApiPolicyName = "sensitive-api";

    /// <summary>
    /// Nombre de la política aplicada a APIs públicas consultivas.
    /// </summary>
    public const string PublicApiPolicyName = "public-api";

    /// <summary>
    /// Configuración del rate limiting para autenticación interactiva.
    /// </summary>
    public FixedWindowPolicyOptions AuthFlow { get; set; } = new(permitLimit: 10, windowSeconds: 60, queueLimit: 0);

    /// <summary>
    /// Configuración del rate limiting para endpoints administrativos sensibles.
    /// </summary>
    public FixedWindowPolicyOptions SensitiveApi { get; set; } = new(permitLimit: 30, windowSeconds: 60, queueLimit: 0);

    /// <summary>
    /// Configuración del rate limiting para endpoints públicos.
    /// </summary>
    public FixedWindowPolicyOptions PublicApi { get; set; } = new(permitLimit: 120, windowSeconds: 60, queueLimit: 0);

    /// <summary>
    /// Define una ventana fija de rate limiting.
    /// </summary>
    public sealed class FixedWindowPolicyOptions
    {
        /// <summary>
        /// Inicializa una nueva instancia con valores por defecto razonables.
        /// </summary>
        public FixedWindowPolicyOptions()
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia con valores explícitos.
        /// </summary>
        public FixedWindowPolicyOptions(int permitLimit, int windowSeconds, int queueLimit)
        {
            PermitLimit = permitLimit;
            WindowSeconds = windowSeconds;
            QueueLimit = queueLimit;
        }

        /// <summary>
        /// Cantidad máxima de solicitudes permitidas por ventana.
        /// </summary>
        [Range(1, 10000)]
        public int PermitLimit { get; set; }

        /// <summary>
        /// Duración de la ventana en segundos.
        /// </summary>
        [Range(1, 3600)]
        public int WindowSeconds { get; set; }

        /// <summary>
        /// Cantidad máxima de solicitudes en cola.
        /// </summary>
        [Range(0, 1000)]
        public int QueueLimit { get; set; }
    }
}
