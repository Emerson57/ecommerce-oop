using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.ValueObjects;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Application.Common.Security;

namespace PlataformaECommerce.Application.Features.Admin;

/// <summary>
/// Centraliza la definición funcional y las constantes del caso de uso de creación de administradores.
/// </summary>
/// <remarks>
/// Esta clase concentra los valores compartidos entre validadores, servicios de aplicación y
/// futuras interfaces del backoffice para mantener una única fuente de verdad respecto a:
/// - campos obligatorios,
/// - política mínima de contraseña,
/// - rol permitido,
/// - y valores por defecto del flujo de alta administrativa.
/// </remarks>
public static class AdminRegistrationPolicies
{
    /// <summary>
    /// Longitud mínima permitida para el nombre del administrador.
    /// </summary>
    public const int MinNameLength = Usuario.LongitudMinimaNombre;

    /// <summary>
    /// Longitud máxima permitida para el nombre del administrador.
    /// </summary>
    public const int MaxNameLength = Usuario.LongitudMaximaNombre;

    /// <summary>
    /// Longitud máxima permitida para el correo electrónico.
    /// </summary>
    public const int MaxEmailLength = Email.MaxLength;

    /// <summary>
    /// Longitud mínima permitida para la contraseña.
    /// </summary>
    public const int MinPasswordLength = PasswordPolicyRules.MinLength;

    /// <summary>
    /// Longitud máxima permitida para la contraseña.
    /// </summary>
    public const int MaxPasswordLength = PasswordPolicyRules.MaxLength;

    /// <summary>
    /// Longitud mínima permitida para el área organizacional.
    /// </summary>
    public const int MinAreaLength = Administrador.LongitudMinimaArea;

    /// <summary>
    /// Longitud máxima permitida para el área organizacional.
    /// </summary>
    public const int MaxAreaLength = Administrador.LongitudMaximaArea;

    /// <summary>
    /// Longitud máxima permitida para la dirección IP.
    /// </summary>
    public const int MaxIpAddressLength = 64;

    /// <summary>
    /// Longitud máxima permitida para el canal de origen.
    /// </summary>
    public const int MaxSourceLength = 50;

    /// <summary>
    /// Longitud máxima permitida para la referencia externa.
    /// </summary>
    public const int MaxExternalReferenceLength = 100;

    /// <summary>
    /// Longitud máxima permitida para el motivo funcional del registro.
    /// </summary>
    public const int MaxReasonLength = 300;

    /// <summary>
    /// Área organizacional sugerida por defecto para la creación interactiva de administradores.
    /// </summary>
    public const string DefaultArea = "Operaciones";

    /// <summary>
    /// Indica el estado activo sugerido por defecto para nuevas cuentas administrativas.
    /// </summary>
    public const bool DefaultIsActive = true;

    /// <summary>
    /// Indica el estado de confirmación de correo sugerido por defecto para nuevas cuentas administrativas.
    /// </summary>
    public const bool DefaultIsEmailConfirmed = false;

    /// <summary>
    /// Rol permitido para la creación de usuarios desde el formulario del backoffice.
    /// </summary>
    public const RolUsuario AllowedBackofficeRole = RolUsuario.Administrador;

    /// <summary>
    /// Obtiene los campos obligatorios del formulario de alta administrativa.
    /// </summary>
    public static IReadOnlyCollection<string> RequiredFields { get; } =
    [
        "Name",
        "Email",
        "Password",
        "ConfirmPassword",
        "Area"
    ];
}
