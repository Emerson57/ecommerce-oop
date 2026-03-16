using System.Text.RegularExpressions;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Domain.ValueObjects;

/// <summary>
/// Representa el SKU (Stock Keeping Unit) de un producto.
/// </summary>
/// <remarks>
/// El SKU es un identificador comercial único utilizado para
/// gestionar inventario y operaciones logísticas.
/// </remarks>
public sealed class Sku : IEquatable<Sku>
{
    private const int MaxLength = 40;

    private static readonly Regex SkuRegex =
        new(@"^[A-Z0-9\-_]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Valor del SKU.
    /// </summary>
    public string Value { get; }

    private Sku()
    {
        Value = string.Empty;
    }

    public Sku(string value)
    {
        Value = Validate(value);
    }

    private static string Validate(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("El SKU del producto es obligatorio.");

        sku = sku.Trim().ToUpperInvariant();

        if (sku.Length > MaxLength)
            throw new DomainException("El SKU supera la longitud máxima permitida.");

        if (!SkuRegex.IsMatch(sku))
            throw new DomainException("El formato del SKU no es válido.");

        return sku;
    }

    public bool Equals(Sku? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
        => obj is Sku other && Equals(other);

    public override int GetHashCode()
        => Value.GetHashCode();

    public override string ToString()
        => Value;

    public static implicit operator string(Sku sku)
        => sku.Value;
}