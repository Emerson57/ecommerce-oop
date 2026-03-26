using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Domain.ValueObjects;

/// <summary>
/// Representa las dimensiones físicas de un producto expresadas en centímetros.
/// </summary>
public sealed class DimensionesProducto : IEquatable<DimensionesProducto>
{
    private const decimal DimensionMaximaCm = 500m;

    public DimensionesProducto(decimal altoCm, decimal anchoCm, decimal largoCm)
    {
        AltoCm = ValidarDimension(altoCm, nameof(altoCm));
        AnchoCm = ValidarDimension(anchoCm, nameof(anchoCm));
        LargoCm = ValidarDimension(largoCm, nameof(largoCm));
    }

    public decimal AltoCm { get; }

    public decimal AnchoCm { get; }

    public decimal LargoCm { get; }

    public decimal VolumenCm3 => decimal.Round(AltoCm * AnchoCm * LargoCm, 2, MidpointRounding.AwayFromZero);

    public bool EsVoluminosa(decimal volumenMinimoCm3)
    {
        if (volumenMinimoCm3 <= 0)
        {
            throw new DomainException("El umbral de volumen debe ser mayor que cero.");
        }

        return VolumenCm3 > volumenMinimoCm3;
    }

    public bool Equals(DimensionesProducto? other)
    {
        if (other is null)
        {
            return false;
        }

        return AltoCm == other.AltoCm &&
               AnchoCm == other.AnchoCm &&
               LargoCm == other.LargoCm;
    }

    public override bool Equals(object? obj)
    {
        return obj is DimensionesProducto other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(AltoCm, AnchoCm, LargoCm);
    }

    public override string ToString()
    {
        return $"{AltoCm:0.##} x {AnchoCm:0.##} x {LargoCm:0.##} cm";
    }

    private static decimal ValidarDimension(decimal valor, string nombreParametro)
    {
        if (valor <= 0)
        {
            throw new ProductException($"La dimensión '{nombreParametro}' del producto físico debe ser mayor que cero.");
        }

        if (valor > DimensionMaximaCm)
        {
            throw new ProductException($"La dimensión '{nombreParametro}' del producto físico no puede superar los {DimensionMaximaCm} cm.");
        }

        return decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
    }
}
