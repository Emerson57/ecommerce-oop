using System.ComponentModel.DataAnnotations;

namespace PlataformaECommerce.Infrastructure.Configurations;

/// <summary>
/// Representa la configuración tipada empleada para emitir y validar
/// tokens JWT dentro de la solución.
/// </summary>
/// <remarks>
/// Esta configuración abstrae los parámetros criptográficos y temporales
/// necesarios para que la infraestructura implemente autenticación basada
/// en tokens sin acoplar la capa Application a detalles de firma,
/// expiración o metadatos del emisor.
/// </remarks>
public sealed class JwtSettings
{
    /// <summary>
    /// Nombre de la sección de configuración utilizada para enlazar esta opción.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Obtiene o establece el emisor lógico de los tokens generados.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la audiencia esperada para los tokens emitidos.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la clave simétrica utilizada para firmar los tokens.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la vigencia del token de acceso expresada en minutos.
    /// </summary>
    [Range(1, 1440)]
    public int AccessTokenExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Obtiene o establece la vigencia del token de refresco expresada en días.
    /// </summary>
    [Range(1, 90)]
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// Obtiene o establece un valor que indica si el middleware JWT debe exigir HTTPS metadata.
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;
}
