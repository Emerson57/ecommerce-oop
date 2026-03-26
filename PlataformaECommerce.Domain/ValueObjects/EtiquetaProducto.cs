using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Domain.ValueObjects;

/// <summary>
/// Representa una etiqueta funcional o comercial asociada a un producto.
/// </summary>
/// <remarks>
/// Este Value Object encapsula la validación, normalización y comparación por valor
/// de las etiquetas utilizadas en productos del e-commerce.
/// 
/// Su propósito es permitir:
/// - clasificación adicional de productos
/// - filtros por etiquetas
/// - navegación comercial
/// - agrupación temática
/// - futuras estrategias de búsqueda y SEO
/// 
/// Ejemplos de etiquetas válidas:
/// - "gaming"
/// - "oferta"
/// - "nuevo-ingreso"
/// - "bestseller"
/// 
/// La clase es inmutable y comparable por valor.
/// </remarks>
public sealed class EtiquetaProducto : IEquatable<EtiquetaProducto>
{
    #region Constantes de negocio

    /// <summary>
    /// Longitud máxima permitida para una etiqueta.
    /// </summary>
    private const int MaxLength = 50;

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor privado requerido por herramientas de persistencia.
    /// </summary>
    private EtiquetaProducto()
    {
        Value = string.Empty;
    }

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="EtiquetaProducto"/>.
    /// </summary>
    /// <param name="value">Texto de la etiqueta.</param>
    public EtiquetaProducto(string value)
    {
        Value = Validar(value);
    }

    #endregion

    #region Propiedades

    /// <summary>
    /// Valor normalizado de la etiqueta.
    /// </summary>
    public string Value { get; }

    #endregion

    #region Métodos de validación

    /// <summary>
    /// Valida y normaliza una etiqueta de producto.
    /// </summary>
    /// <param name="value">Valor recibido.</param>
    /// <returns>Etiqueta normalizada.</returns>
    /// <exception cref="DomainException">Se lanza si la etiqueta no cumple las reglas del dominio.</exception>
    private static string Validar(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("La etiqueta del producto es obligatoria.");
        }

        value = value.Trim().ToLowerInvariant();

        if (value.Length > MaxLength)
        {
            throw new DomainException($"La etiqueta del producto no puede superar los {MaxLength} caracteres.");
        }

        if (value.Contains("  "))
        {
            throw new DomainException("La etiqueta del producto no puede contener espacios dobles consecutivos.");
        }

        return value;
    }

    #endregion

    #region Comparación por valor

    /// <summary>
    /// Compara esta etiqueta con otra instancia por valor.
    /// </summary>
    /// <param name="other">Otra etiqueta.</param>
    /// <returns>True si ambas representan el mismo valor lógico.</returns>
    public bool Equals(EtiquetaProducto? other)
    {
        if (other is null)
        {
            return false;
        }

        return Value == other.Value;
    }

    /// <summary>
    /// Compara esta etiqueta con otro objeto.
    /// </summary>
    /// <param name="obj">Objeto a comparar.</param>
    /// <returns>True si el objeto es una etiqueta equivalente.</returns>
    public override bool Equals(object? obj)
    {
        return obj is EtiquetaProducto other && Equals(other);
    }

    /// <summary>
    /// Obtiene el hash code basado en el valor normalizado de la etiqueta.
    /// </summary>
    /// <returns>Hash code del objeto.</returns>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    #endregion

    #region Conversión y representación

    /// <summary>
    /// Devuelve el valor textual normalizado de la etiqueta.
    /// </summary>
    /// <returns>Valor de la etiqueta.</returns>
    public override string ToString()
    {
        return Value;
    }

    /// <summary>
    /// Permite convertir implícitamente una etiqueta a string.
    /// </summary>
    /// <param name="etiqueta">Etiqueta a convertir.</param>
    public static implicit operator string(EtiquetaProducto etiqueta)
    {
        return etiqueta.Value;
    }

    #endregion
}