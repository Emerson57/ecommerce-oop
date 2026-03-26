namespace PlataformaECommerce.Application.Common.Security;

/// <summary>
/// Centraliza la política de composición mínima requerida para contraseñas sensibles del sistema.
/// </summary>
/// <remarks>
/// Estas reglas se reutilizan en flujos de alta administrativa y recuperación de credenciales
/// para evitar divergencias funcionales entre validadores y experiencias de usuario.
/// </remarks>
public static class PasswordPolicyRules
{
    /// <summary>
    /// Longitud mínima requerida para la contraseña.
    /// </summary>
    public const int MinLength = 8;

    /// <summary>
    /// Longitud máxima permitida para la contraseña.
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// Determina si la contraseña contiene al menos una letra mayúscula.
    /// </summary>
    public static bool HasUppercase(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && value.Any(char.IsUpper);
    }

    /// <summary>
    /// Determina si la contraseña contiene al menos una letra minúscula.
    /// </summary>
    public static bool HasLowercase(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && value.Any(char.IsLower);
    }

    /// <summary>
    /// Determina si la contraseña contiene al menos un dígito.
    /// </summary>
    public static bool HasDigit(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && value.Any(char.IsDigit);
    }

    /// <summary>
    /// Determina si la contraseña contiene al menos un carácter no alfanumérico.
    /// </summary>
    public static bool HasSpecialCharacter(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && value.Any(character => !char.IsLetterOrDigit(character));
    }
}
