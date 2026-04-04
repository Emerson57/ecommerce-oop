using System.ComponentModel.DataAnnotations;

namespace PlataformaECommerce.Infrastructure.Configurations;

/// <summary>
/// Representa la configuración requerida para entregar correos electrónicos por SMTP.
/// </summary>
public sealed class SmtpEmailSettings
{
    /// <summary>
    /// Nombre de la sección de configuración asociada.
    /// </summary>
    public const string SectionName = "Notifications:Smtp";

    /// <summary>
    /// Indica si la entrega de correos está habilitada en el entorno actual.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Host SMTP del proveedor de correo.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Puerto SMTP del proveedor.
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    /// <summary>
    /// Indica si la conexión SMTP debe usar TLS/SSL.
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// Usuario autenticado del servidor SMTP.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Contraseña del usuario SMTP.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Dirección remitente de los correos.
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Nombre visible del remitente.
    /// </summary>
    public string FromDisplayName { get; set; } = "Plataforma ECommerce";
}
