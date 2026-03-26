using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Application.Features.Admin;

namespace PlataformaECommerce.Application.Features.Admin.DTOs;

/// <summary>
/// Representa la definición funcional del caso de uso de creación de administradores desde el backoffice.
/// </summary>
/// <remarks>
/// Este DTO expone hacia la capa Web las reglas, restricciones y valores por defecto del flujo
/// de alta administrativa para que la futura UI pueda construirse sin duplicar decisiones críticas
/// de negocio ni de seguridad.
/// </remarks>
public sealed class AdminRegistrationDefinitionDto
{
    /// <summary>
    /// Fecha y hora UTC en que fue generada la definición funcional.
    /// </summary>
    public DateTime GeneratedAtUtc { get; init; }

    /// <summary>
    /// Identificador del usuario que originó la consulta, cuando se conoce.
    /// </summary>
    public Guid? GeneratedByUserId { get; init; }

    /// <summary>
    /// Nombre visible del usuario que originó la consulta, cuando se conoce.
    /// </summary>
    public string? GeneratedByUserName { get; init; }

    /// <summary>
    /// Canal de origen asociado a la consulta.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada a la consulta.
    /// </summary>
    public string? ExternalReference { get; init; }

    /// <summary>
    /// Rol objetivo permitido para el alta administrativa interactiva.
    /// </summary>
    public RolUsuario AllowedRole { get; init; } = AdminRegistrationPolicies.AllowedBackofficeRole;

    /// <summary>
    /// Área organizacional sugerida por defecto para el formulario.
    /// </summary>
    public string DefaultArea { get; init; } = AdminRegistrationPolicies.DefaultArea;

    /// <summary>
    /// Indica si la cuenta debe sugerirse inicialmente activa.
    /// </summary>
    public bool DefaultIsActive { get; init; } = AdminRegistrationPolicies.DefaultIsActive;

    /// <summary>
    /// Indica si la cuenta debe sugerirse inicialmente con correo confirmado.
    /// </summary>
    public bool DefaultIsEmailConfirmed { get; init; } = AdminRegistrationPolicies.DefaultIsEmailConfirmed;

    /// <summary>
    /// Indica si el caso de uso exige un super usuario autenticado.
    /// </summary>
    public bool RequiresAuthenticatedSuperUser { get; init; } = true;

    /// <summary>
    /// Indica si el formulario permite crear cuentas de super usuario.
    /// </summary>
    public bool AllowsSuperUserCreation { get; init; }

    /// <summary>
    /// Indica si el correo electrónico debe ser único en el sistema.
    /// </summary>
    public bool RequiresUniqueEmail { get; init; } = true;

    /// <summary>
    /// Indica si el alta debe registrar auditoría obligatoria.
    /// </summary>
    public bool RequiresAuditTrail { get; init; } = true;

    /// <summary>
    /// Indica si el formulario permite definir el estado activo inicial de la cuenta.
    /// </summary>
    public bool SupportsInitialActivationStatus { get; init; } = true;

    /// <summary>
    /// Indica si el formulario permite definir el estado inicial de confirmación del correo.
    /// </summary>
    public bool SupportsInitialEmailConfirmationStatus { get; init; } = true;

    /// <summary>
    /// Longitud mínima requerida para la contraseña.
    /// </summary>
    public int PasswordMinLength { get; init; } = AdminRegistrationPolicies.MinPasswordLength;

    /// <summary>
    /// Indica si la contraseña debe contener al menos una letra mayúscula.
    /// </summary>
    public bool RequiresUppercase { get; init; } = true;

    /// <summary>
    /// Indica si la contraseña debe contener al menos una letra minúscula.
    /// </summary>
    public bool RequiresLowercase { get; init; } = true;

    /// <summary>
    /// Indica si la contraseña debe contener al menos un número.
    /// </summary>
    public bool RequiresDigit { get; init; } = true;

    /// <summary>
    /// Indica si la contraseña debe contener al menos un carácter especial.
    /// </summary>
    public bool RequiresSpecialCharacter { get; init; } = true;

    /// <summary>
    /// Colección de campos obligatorios del formulario.
    /// </summary>
    public IReadOnlyCollection<string> RequiredFields { get; init; } = AdminRegistrationPolicies.RequiredFields;
}
