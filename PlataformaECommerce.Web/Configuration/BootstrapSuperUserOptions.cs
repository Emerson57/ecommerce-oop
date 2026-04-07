using System.ComponentModel.DataAnnotations;

namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Representa la configuración utilizada para bootstrappear el primer super usuario del sistema.
/// </summary>
/// <remarks>
/// Esta opción se consume únicamente desde la composición raíz para habilitar una creación
/// controlada, auditable y de una sola vez del primer usuario con privilegios máximos.
/// </remarks>
public sealed class BootstrapSuperUserOptions : IValidatableObject
{
    /// <summary>
    /// Nombre de la sección de configuración asociada al bootstrap del super usuario.
    /// </summary>
    public const string SectionName = "Bootstrap:SuperUser";

    /// <summary>
    /// Indica si el bootstrap inicial se encuentra habilitado.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Indica si el bootstrap del super usuario puede ejecutarse explícitamente en producción.
    /// </summary>
    public bool AllowInProduction { get; set; }

    /// <summary>
    /// Nombre completo del super usuario inicial.
    /// </summary>
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tenant objetivo sobre el cual debe ejecutarse el bootstrap inicial.
    /// </summary>
    [StringLength(200)]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico del super usuario inicial.
    /// </summary>
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña temporal del super usuario inicial.
    /// </summary>
    [StringLength(256, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Área organizacional asociada al super usuario inicial.
    /// </summary>
    [StringLength(100)]
    public string Area { get; set; } = "Plataforma";

    /// <summary>
    /// Ejecuta validaciones condicionales para el bootstrap del super usuario cuando la característica está habilitada.
    /// </summary>
    /// <param name="validationContext">Contexto de validación actual.</param>
    /// <returns>Errores de validación detectados para la configuración cargada.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        ArgumentNullException.ThrowIfNull(validationContext);

        if (!Enabled)
        {
            return [];
        }

        List<ValidationResult> validationResults = [];

        AddRequiredWhenEnabled(validationResults, Name, nameof(Name), "El bootstrap del super usuario requiere un nombre válido cuando está habilitado.");
        AddRequiredWhenEnabled(validationResults, TenantId, nameof(TenantId), "El bootstrap del super usuario requiere un tenant objetivo válido cuando está habilitado.");
        AddRequiredWhenEnabled(validationResults, Email, nameof(Email), "El bootstrap del super usuario requiere un correo electrónico válido cuando está habilitado.");
        AddRequiredWhenEnabled(validationResults, Password, nameof(Password), "El bootstrap del super usuario requiere una contraseña válida cuando está habilitado.");
        AddRequiredWhenEnabled(validationResults, Area, nameof(Area), "El bootstrap del super usuario requiere un área válida cuando está habilitado.");

        return validationResults;
    }

    private static void AddRequiredWhenEnabled(
        ICollection<ValidationResult> validationResults,
        string? value,
        string memberName,
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(validationResults);

        if (!string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        validationResults.Add(new ValidationResult(errorMessage, [memberName]));
    }
}
