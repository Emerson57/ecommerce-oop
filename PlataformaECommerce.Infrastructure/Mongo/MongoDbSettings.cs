using System.ComponentModel.DataAnnotations;

namespace PlataformaECommerce.Infrastructure.Mongo;

/// <summary>
/// Representa la configuración tipada necesaria para operar con MongoDB
/// como almacén de auditoría y trazabilidad dentro de la solución.
/// </summary>
/// <remarks>
/// Esta configuración se enlaza desde la sección <c>MongoDb</c> del sistema
/// y centraliza los valores mínimos requeridos para construir clientes,
/// seleccionar la base de datos de auditoría y resolver el nombre de la colección
/// asociada a la auditoría transversal del sistema.
/// </remarks>
public sealed class MongoDbSettings
{
    /// <summary>
    /// Nombre de la sección de configuración utilizada para enlazar esta opción.
    /// </summary>
    public const string SectionName = "MongoDb";

    /// <summary>
    /// Obtiene o establece un valor que indica si la auditoría transversal sobre MongoDB se encuentra habilitada.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Obtiene o establece la cadena de conexión hacia la instancia de MongoDB.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre de la base de datos utilizada para auditoría.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el nombre de la colección donde se almacenan
    /// los eventos de auditoría transversales del sistema.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string AuditCollectionName { get; set; } = "audit_trail";

    /// <summary>
    /// Obtiene o establece un valor que indica si la infraestructura debe garantizar
    /// la creación de índices recomendados para auditoría al iniciar el repositorio.
    /// </summary>
    public bool EnsureIndexesOnStartup { get; set; } = true;
}
