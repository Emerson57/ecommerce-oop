using System.ComponentModel.DataAnnotations;

namespace PlataformaECommerce.Infrastructure.Configurations;

/// <summary>
/// Representa la configuración requerida para persistir y compartir de forma estable las claves de Data Protection.
/// </summary>
public sealed class DataProtectionKeyManagementSettings
{
    /// <summary>
    /// Nombre de la sección de configuración utilizada para enlazar esta opción.
    /// </summary>
    public const string SectionName = "DataProtection";

    /// <summary>
    /// Obtiene o establece el nombre lógico compartido por todas las instancias que deben reutilizar el mismo anillo de claves.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la vida útil por defecto de las claves emitidas para el anillo compartido.
    /// </summary>
    [Range(7, 365)]
    public int KeyLifetimeDays { get; set; } = 90;
}
