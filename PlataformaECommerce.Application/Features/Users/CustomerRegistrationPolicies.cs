using PlataformaECommerce.Application.Common.Security;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Application.Features.Users;

/// <summary>
/// Centraliza la definición funcional y las constantes compartidas del registro de clientes.
/// </summary>
/// <remarks>
/// Esta clase mantiene una única fuente de verdad para longitudes, reglas de contraseña,
/// consentimientos y límites operativos reutilizados por validadores de Application y por
/// experiencias web de registro público.
/// </remarks>
public static class CustomerRegistrationPolicies
{
    /// <summary>
    /// Longitud mínima permitida para el nombre del cliente.
    /// </summary>
    public const int MinNameLength = Usuario.LongitudMinimaNombre;

    /// <summary>
    /// Longitud máxima permitida para el nombre del cliente.
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
    /// Longitud mínima permitida para una preferencia del cliente.
    /// </summary>
    public const int MinPreferenceLength = 2;

    /// <summary>
    /// Longitud máxima permitida para una preferencia del cliente.
    /// </summary>
    public const int MaxPreferenceLength = 50;

    /// <summary>
    /// Cantidad máxima de preferencias permitidas durante el registro.
    /// </summary>
    public const int MaxPreferencesCount = 20;

    /// <summary>
    /// Longitud máxima permitida para la dirección IP de origen.
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
}
