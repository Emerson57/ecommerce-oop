namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Representa las opciones de disponibilidad del módulo de usuarios del backoffice.
/// </summary>
/// <remarks>
/// Esta configuración permite preparar rutas y contratos futuros sin exponer todavía
/// funcionalidades visuales incompletas al usuario final.
/// </remarks>
public sealed class AdminUsersBackofficeOptions
{
    /// <summary>
    /// Nombre de la sección de configuración asociada al módulo de usuarios del backoffice.
    /// </summary>
    public const string SectionName = "Backoffice:Users";

    /// <summary>
    /// Indica si la interfaz interactiva de creación de administradores está disponible.
    /// </summary>
    public bool EnableAdministratorCreationUi { get; set; }
}
