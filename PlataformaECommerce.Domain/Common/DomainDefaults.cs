namespace PlataformaECommerce.Domain.Common;

/// <summary>
/// Centraliza valores por defecto reutilizados por el dominio.
/// </summary>
/// <remarks>
/// Esta clase agrupa constantes transversales del modelo para evitar duplicación
/// de literales entre agregados, reglas y objetos de valor.
/// </remarks>
internal static class DomainDefaults
{
    /// <summary>
    /// Código ISO de moneda utilizado por defecto en agregados comerciales vacíos.
    /// </summary>
    internal const string DefaultCurrency = "COP";
}