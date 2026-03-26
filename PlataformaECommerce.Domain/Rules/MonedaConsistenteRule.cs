using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Domain.Rules;

/// <summary>
/// Representa la regla de negocio que valida consistencia monetaria dentro de un mismo agregado comercial.
/// </summary>
/// <remarks>
/// La regla se utiliza para impedir que un carrito o un pedido combine líneas expresadas
/// en monedas distintas, preservando así la coherencia del cálculo de subtotales y totales.
/// </remarks>
internal static class MonedaConsistenteRule
{
    /// <summary>
    /// Evalúa si un valor monetario puede coexistir con una moneda esperada.
    /// </summary>
    /// <param name="monedaEsperada">Moneda ya establecida en el agregado.</param>
    /// <param name="valor">Valor monetario a validar.</param>
    /// <returns>
    /// <see langword="true"/> si la moneda es consistente o si aún no existe una referencia previa;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public static bool IsSatisfiedBy(string? monedaEsperada, Money? valor)
    {
        if (valor is null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(monedaEsperada))
        {
            return true;
        }

        return string.Equals(monedaEsperada.Trim(), valor.Currency, StringComparison.OrdinalIgnoreCase);
    }
}