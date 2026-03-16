namespace PlataformaECommerce.Domain.Rules;

/// <summary>
/// Representa la regla de negocio que valida si existe inventario suficiente
/// para atender una cantidad solicitada.
/// </summary>
/// <remarks>
/// Esta regla centraliza la validación de disponibilidad de stock en escenarios como:
/// - agregado al carrito,
/// - incremento de cantidades,
/// - reserva de inventario,
/// - validaciones previas al checkout,
/// - reducción de existencias,
/// - generación de pedidos.
///
/// La regla exige que:
/// - la cantidad solicitada sea mayor que cero,
/// - el stock disponible no sea negativo,
/// - y el stock disponible sea mayor o igual a la cantidad requerida.
/// </remarks>
public sealed class StockDisponibleRule
{
    /// <summary>
    /// Evalúa si existe stock suficiente para cubrir una cantidad solicitada.
    /// </summary>
    /// <param name="stockDisponible">Stock actualmente disponible.</param>
    /// <param name="cantidadSolicitada">Cantidad requerida por la operación.</param>
    /// <returns>
    /// <see langword="true"/> si el stock disponible es suficiente;
    /// de lo contrario, <see langword="false"/>.
    /// </returns>
    public bool IsSatisfiedBy(int stockDisponible, int cantidadSolicitada)
    {
        if (stockDisponible < 0)
        {
            return false;
        }

        if (cantidadSolicitada <= 0)
        {
            return false;
        }

        return stockDisponible >= cantidadSolicitada;
    }

    /// <summary>
    /// Obtiene una descripción funcional de la regla.
    /// </summary>
    /// <returns>Texto descriptivo de la regla.</returns>
    public override string ToString()
    {
        return "El stock disponible debe ser mayor o igual a la cantidad solicitada, y ambos valores deben ser válidos.";
    }
}