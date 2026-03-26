using System.Text.RegularExpressions;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Domain.ValueObjects;

/// <summary>
/// Representa un valor monetario dentro del dominio del sistema.
/// </summary>
/// <remarks>
/// Este Value Object encapsula la representación de dinero evitando errores
/// comunes asociados al uso directo de tipos numéricos para valores financieros.
///
/// Características principales:
/// - Inmutable
/// - Comparación por valor
/// - Control de precisión decimal
/// - Validación estricta de moneda
/// - Operadores aritméticos seguros
/// - Operadores de comparación
///
/// Se utiliza en entidades como:
/// - Producto
/// - Pedido
/// - DetallePedido
/// - Carrito
/// </remarks>
public sealed class Money : IEquatable<Money>, IComparable<Money>
{
    #region Constantes de negocio

    /// <summary>
    /// Valor monetario mínimo permitido.
    /// </summary>
    private const decimal MinAmount = 0m;

    /// <summary>
    /// Valor monetario máximo permitido.
    /// </summary>
    private const decimal MaxAmount = 999999999m;

    /// <summary>
    /// Código ISO de moneda por defecto.
    /// </summary>
    private const string DefaultCurrency = "COP";

    /// <summary>
    /// Patrón estricto para códigos ISO 4217 de moneda.
    /// </summary>
    /// <remarks>
    /// Se exige exactamente tres letras alfabéticas en mayúscula.
    /// Ejemplos válidos: COP, USD, EUR.
    /// </remarks>
    private const string CurrencyPattern = "^[A-Z]{3}$";

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Monto monetario.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Código de moneda ISO 4217.
    /// </summary>
    public string Currency { get; }

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor privado requerido por algunas herramientas de persistencia.
    /// </summary>
    private Money()
    {
        Amount = 0m;
        Currency = DefaultCurrency;
    }

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="Money"/>.
    /// </summary>
    /// <param name="amount">Monto monetario.</param>
    /// <param name="currency">Código ISO 4217 de la moneda.</param>
    public Money(decimal amount, string currency = DefaultCurrency)
    {
        Amount = ValidateAmount(amount);
        Currency = ValidateCurrency(currency);
    }

    #endregion

    #region Métodos de fábrica

    /// <summary>
    /// Representa un valor monetario cero.
    /// </summary>
    /// <param name="currency">Código ISO 4217 de la moneda.</param>
    /// <returns>Instancia monetaria con valor cero.</returns>
    public static Money Zero(string currency = DefaultCurrency)
    {
        return new Money(0m, currency);
    }

    #endregion

    #region Operadores aritméticos

    /// <summary>
    /// Suma dos valores monetarios de la misma moneda.
    /// </summary>
    /// <param name="a">Primer valor monetario.</param>
    /// <param name="b">Segundo valor monetario.</param>
    /// <returns>Resultado de la suma.</returns>
    public static Money operator +(Money a, Money b)
    {
        ValidateNotNull(a, nameof(a));
        ValidateNotNull(b, nameof(b));
        ValidateSameCurrency(a, b);

        return new Money(a.Amount + b.Amount, a.Currency);
    }

    /// <summary>
    /// Resta dos valores monetarios de la misma moneda.
    /// </summary>
    /// <param name="a">Primer valor monetario.</param>
    /// <param name="b">Segundo valor monetario.</param>
    /// <returns>Resultado de la resta.</returns>
    public static Money operator -(Money a, Money b)
    {
        ValidateNotNull(a, nameof(a));
        ValidateNotNull(b, nameof(b));
        ValidateSameCurrency(a, b);

        return new Money(a.Amount - b.Amount, a.Currency);
    }

    /// <summary>
    /// Multiplica un valor monetario por un entero no negativo.
    /// </summary>
    /// <param name="a">Valor monetario.</param>
    /// <param name="multiplier">Multiplicador entero.</param>
    /// <returns>Resultado de la multiplicación.</returns>
    public static Money operator *(Money a, int multiplier)
    {
        ValidateNotNull(a, nameof(a));

        if (multiplier < 0)
        {
            throw new DomainException("El multiplicador entero no puede ser negativo.");
        }

        return new Money(a.Amount * multiplier, a.Currency);
    }

    /// <summary>
    /// Multiplica un valor monetario por un factor decimal no negativo.
    /// </summary>
    /// <param name="a">Valor monetario.</param>
    /// <param name="multiplier">Multiplicador decimal.</param>
    /// <returns>Resultado de la multiplicación.</returns>
    public static Money operator *(Money a, decimal multiplier)
    {
        ValidateNotNull(a, nameof(a));

        if (multiplier < 0)
        {
            throw new DomainException("El multiplicador decimal no puede ser negativo.");
        }

        return new Money(a.Amount * multiplier, a.Currency);
    }

    /// <summary>
    /// Multiplica un valor monetario por un factor decimal no negativo.
    /// </summary>
    /// <param name="multiplier">Multiplicador decimal.</param>
    /// <param name="a">Valor monetario.</param>
    /// <returns>Resultado de la multiplicación.</returns>
    public static Money operator *(decimal multiplier, Money a)
    {
        return a * multiplier;
    }

    #endregion

    #region Operadores de comparación

    /// <summary>
    /// Determina si un valor monetario es mayor que otro.
    /// </summary>
    public static bool operator >(Money a, Money b)
    {
        return Compare(a, b) > 0;
    }

    /// <summary>
    /// Determina si un valor monetario es menor que otro.
    /// </summary>
    public static bool operator <(Money a, Money b)
    {
        return Compare(a, b) < 0;
    }

    /// <summary>
    /// Determina si un valor monetario es mayor o igual que otro.
    /// </summary>
    public static bool operator >=(Money a, Money b)
    {
        return Compare(a, b) >= 0;
    }

    /// <summary>
    /// Determina si un valor monetario es menor o igual que otro.
    /// </summary>
    public static bool operator <=(Money a, Money b)
    {
        return Compare(a, b) <= 0;
    }

    /// <summary>
    /// Determina si dos valores monetarios son iguales.
    /// </summary>
    public static bool operator ==(Money? a, Money? b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    /// <summary>
    /// Determina si dos valores monetarios son diferentes.
    /// </summary>
    public static bool operator !=(Money? a, Money? b)
    {
        return !(a == b);
    }

    #endregion

    #region Métodos públicos

    /// <summary>
    /// Determina si el valor monetario es cero.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si el monto es cero;
    /// de lo contrario, <see langword="false"/>.
    /// </returns>
    public bool IsZero()
    {
        return Amount == 0m;
    }

    /// <summary>
    /// Determina si el valor monetario es mayor que cero.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si el monto es mayor que cero;
    /// de lo contrario, <see langword="false"/>.
    /// </returns>
    public bool IsPositive()
    {
        return Amount > 0m;
    }

    /// <summary>
    /// Determina si el valor monetario actual comparte la misma moneda con otro valor monetario.
    /// </summary>
    /// <param name="other">Otro valor monetario a evaluar.</param>
    /// <returns>
    /// <see langword="true"/> si ambos valores tienen la misma moneda;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool HasSameCurrency(Money? other)
    {
        return other is not null && Currency == other.Currency;
    }

    /// <summary>
    /// Compara la instancia actual con otro valor monetario.
    /// </summary>
    /// <param name="other">Otro valor monetario.</param>
    /// <returns>
    /// Un valor menor que cero si esta instancia es menor,
    /// cero si ambas son iguales,
    /// o un valor mayor que cero si esta instancia es mayor.
    /// </returns>
    public int CompareTo(Money? other)
    {
        if (other is null)
        {
            return 1;
        }

        ValidateSameCurrency(this, other);
        return Amount.CompareTo(other.Amount);
    }

    /// <summary>
    /// Determina si la instancia actual es igual a otro valor monetario.
    /// </summary>
    /// <param name="other">Otro valor monetario.</param>
    /// <returns>
    /// <see langword="true"/> si ambos valores son equivalentes;
    /// de lo contrario, <see langword="false"/>.
    /// </returns>
    public bool Equals(Money? other)
    {
        if (other is null)
        {
            return false;
        }

        return Amount == other.Amount && Currency == other.Currency;
    }

    /// <summary>
    /// Determina si la instancia actual es igual a otro objeto.
    /// </summary>
    /// <param name="obj">Objeto a comparar.</param>
    /// <returns>
    /// <see langword="true"/> si el objeto representa el mismo valor monetario;
    /// de lo contrario, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return obj is Money other && Equals(other);
    }

    /// <summary>
    /// Devuelve el código hash de la instancia actual.
    /// </summary>
    /// <returns>Código hash basado en monto y moneda.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Currency);
    }

    /// <summary>
    /// Devuelve una representación legible del valor monetario.
    /// </summary>
    /// <returns>Texto con moneda y monto formateado.</returns>
    public override string ToString()
    {
        return $"{Currency} {Amount:N2}";
    }

    #endregion

    #region Métodos privados auxiliares

    /// <summary>
    /// Valida el monto monetario.
    /// </summary>
    /// <param name="amount">Monto a validar.</param>
    /// <returns>Monto validado y redondeado a dos decimales.</returns>
    private static decimal ValidateAmount(decimal amount)
    {
        if (amount < MinAmount)
        {
            throw new DomainException("El valor monetario no puede ser negativo.");
        }

        if (amount > MaxAmount)
        {
            throw new DomainException("El valor monetario supera el límite permitido.");
        }

        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Valida el código de moneda conforme a un formato ISO 4217 estricto.
    /// </summary>
    /// <param name="currency">Código de moneda a validar.</param>
    /// <returns>Código de moneda validado y normalizado.</returns>
    private static string ValidateCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("La moneda es obligatoria.");
        }

        string normalizedCurrency = currency.Trim().ToUpperInvariant();

        if (!Regex.IsMatch(normalizedCurrency, CurrencyPattern))
        {
            throw new DomainException("La moneda debe corresponder a un código ISO 4217 válido de tres letras, por ejemplo COP, USD o EUR.");
        }

        return normalizedCurrency;
    }

    /// <summary>
    /// Valida que dos valores monetarios compartan la misma moneda.
    /// </summary>
    /// <param name="a">Primer valor monetario.</param>
    /// <param name="b">Segundo valor monetario.</param>
    private static void ValidateSameCurrency(Money a, Money b)
    {
        if (a.Currency != b.Currency)
        {
            throw new DomainException("No es posible operar ni comparar valores monetarios con diferentes monedas.");
        }
    }

    /// <summary>
    /// Valida que un valor monetario no sea nulo.
    /// </summary>
    /// <param name="money">Valor monetario a validar.</param>
    /// <param name="paramName">Nombre lógico del parámetro.</param>
    private static void ValidateNotNull(Money? money, string paramName)
    {
        if (money is null)
        {
            throw new DomainException($"El valor monetario '{paramName}' no puede ser nulo.");
        }
    }

    /// <summary>
    /// Compara dos valores monetarios garantizando misma moneda.
    /// </summary>
    /// <param name="a">Primer valor monetario.</param>
    /// <param name="b">Segundo valor monetario.</param>
    /// <returns>
    /// Un valor menor que cero si <paramref name="a"/> es menor,
    /// cero si son iguales,
    /// o un valor mayor que cero si <paramref name="a"/> es mayor.
    /// </returns>
    private static int Compare(Money a, Money b)
    {
        ValidateNotNull(a, nameof(a));
        ValidateNotNull(b, nameof(b));
        ValidateSameCurrency(a, b);

        return a.Amount.CompareTo(b.Amount);
    }

    #endregion
}