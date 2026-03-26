using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Domain.ValueObjects;

/// <summary>
/// Representa una dirección de envío dentro del dominio.
/// </summary>
/// <remarks>
/// Este Value Object encapsula los datos necesarios para realizar
/// el despacho físico de un pedido.
/// 
/// Se utiliza típicamente en:
/// - Pedido
/// - Facturación
/// - Perfil del cliente
/// </remarks>
public sealed class DireccionEnvio : IEquatable<DireccionEnvio>
{
    private const int MaxLength = 150;

    public string Calle { get; }
    public string Ciudad { get; }
    public string Departamento { get; }
    public string Pais { get; }
    public string CodigoPostal { get; }

    private DireccionEnvio()
    {
        Calle = string.Empty;
        Ciudad = string.Empty;
        Departamento = string.Empty;
        Pais = string.Empty;
        CodigoPostal = string.Empty;
    }

    public DireccionEnvio(
        string calle,
        string ciudad,
        string departamento,
        string pais,
        string codigoPostal)
    {
        Calle = Validate(calle, nameof(calle));
        Ciudad = Validate(ciudad, nameof(ciudad));
        Departamento = Validate(departamento, nameof(departamento));
        Pais = Validate(pais, nameof(pais));
        CodigoPostal = Validate(codigoPostal, nameof(codigoPostal));
    }

    private static string Validate(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"El campo '{field}' es obligatorio.");

        value = value.Trim();

        if (value.Length > MaxLength)
            throw new DomainException($"El campo '{field}' supera la longitud máxima permitida.");

        return value;
    }

    public bool Equals(DireccionEnvio? other)
    {
        if (other is null) return false;

        return Calle == other.Calle &&
               Ciudad == other.Ciudad &&
               Departamento == other.Departamento &&
               Pais == other.Pais &&
               CodigoPostal == other.CodigoPostal;
    }

    public override bool Equals(object? obj)
        => obj is DireccionEnvio other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Calle, Ciudad, Departamento, Pais, CodigoPostal);

    public override string ToString()
        => $"{Calle}, {Ciudad}, {Departamento}, {Pais}, {CodigoPostal}";
}