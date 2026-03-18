using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Domain.ValueObjects;

/// <summary>
/// Representa la información técnica principal de un archivo digital asociado a un producto.
/// </summary>
public sealed class ArchivoDigital : IEquatable<ArchivoDigital>
{
    private const int LongitudMaximaFormatoArchivo = 20;
    private const decimal TamanoMaximoArchivoMb = 10240m;

    public ArchivoDigital(string formato, decimal? tamanoMb)
    {
        Formato = ValidarFormato(formato);
        TamanoMb = ValidarTamano(tamanoMb);
    }

    public string Formato { get; }

    public decimal? TamanoMb { get; }

    public bool EsLiviano(decimal tamanoMaximoLivianoMb)
    {
        if (tamanoMaximoLivianoMb <= 0)
        {
            throw new DomainException("El umbral de tamaño liviano debe ser mayor que cero.");
        }

        return TamanoMb.HasValue && TamanoMb.Value <= tamanoMaximoLivianoMb;
    }

    public bool Equals(ArchivoDigital? other)
    {
        if (other is null)
        {
            return false;
        }

        return Formato == other.Formato && TamanoMb == other.TamanoMb;
    }

    public override bool Equals(object? obj)
    {
        return obj is ArchivoDigital other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Formato, TamanoMb);
    }

    public override string ToString()
    {
        return TamanoMb.HasValue
            ? $"{Formato} ({TamanoMb.Value:0.##} MB)"
            : Formato;
    }

    private static string ValidarFormato(string formato)
    {
        if (string.IsNullOrWhiteSpace(formato))
        {
            throw new ProductException("El formato del archivo digital es obligatorio.");
        }

        string formatoNormalizado = formato.Trim().ToUpperInvariant();

        if (formatoNormalizado.Length > LongitudMaximaFormatoArchivo)
        {
            throw new ProductException($"El formato del archivo digital no puede superar los {LongitudMaximaFormatoArchivo} caracteres.");
        }

        return formatoNormalizado;
    }

    private static decimal? ValidarTamano(decimal? tamanoMb)
    {
        if (!tamanoMb.HasValue)
        {
            return null;
        }

        if (tamanoMb.Value <= 0)
        {
            throw new ProductException("El tamaño del archivo digital debe ser mayor que cero.");
        }

        if (tamanoMb.Value > TamanoMaximoArchivoMb)
        {
            throw new ProductException($"El tamaño del archivo digital no puede superar los {TamanoMaximoArchivoMb} MB.");
        }

        return decimal.Round(tamanoMb.Value, 2, MidpointRounding.AwayFromZero);
    }
}
