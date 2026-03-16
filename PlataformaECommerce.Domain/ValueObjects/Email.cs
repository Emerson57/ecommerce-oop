using System.Text.RegularExpressions;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Domain.ValueObjects;

/// <summary>
/// Representa un correo electrónico dentro del dominio.
/// </summary>
/// <remarks>
/// Encapsula la validación y normalización de direcciones de correo
/// utilizadas por el sistema para autenticación, contacto y notificaciones.
/// </remarks>
public sealed class Email : IEquatable<Email>
{
    private const int MaxLength = 320;

    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Valor normalizado del correo electrónico.
    /// </summary>
    public string Value { get; }

    private Email()
    {
        Value = string.Empty;
    }

    /// <summary>
    /// Crea una nueva instancia de <see cref="Email"/>.
    /// </summary>
    public Email(string value)
    {
        Value = Validate(value);
    }

    private static string Validate(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("El correo electrónico es obligatorio.");

        email = email.Trim().ToLowerInvariant();

        if (email.Length > MaxLength)
            throw new DomainException("El correo electrónico supera la longitud máxima permitida.");

        if (!EmailRegex.IsMatch(email))
            throw new DomainException("El formato del correo electrónico no es válido.");

        return email;
    }

    public bool Equals(Email? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
        => obj is Email other && Equals(other);

    public override int GetHashCode()
        => Value.GetHashCode();

    public override string ToString()
        => Value;

    public static implicit operator string(Email email)
        => email.Value;
}