namespace PlataformaECommerce.Domain.Exceptions;

/// <summary>
/// Representa el error generado cuando un producto no dispone de inventario suficiente
/// para atender una operación solicitada.
/// </summary>
/// <remarks>
/// Esta excepción se utiliza típicamente en procesos de carrito, pedido, reserva de inventario
/// o actualización de stock cuando la cantidad requerida supera la disponibilidad real.
/// </remarks>
public class InventarioInsuficienteException : ProductException
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="InventarioInsuficienteException"/>.
    /// </summary>
    /// <param name="sku">SKU del producto afectado.</param>
    /// <param name="stockDisponible">Cantidad actualmente disponible en inventario.</param>
    /// <param name="cantidadSolicitada">Cantidad requerida por la operación.</param>
    public InventarioInsuficienteException(
        string sku,
        int stockDisponible,
        int cantidadSolicitada)
        : base($"Inventario insuficiente para el producto con SKU '{sku}'. Stock disponible: {stockDisponible}. Cantidad solicitada: {cantidadSolicitada}.")
    {
        Sku = sku;
        StockDisponible = stockDisponible;
        CantidadSolicitada = cantidadSolicitada;
    }

    /// <summary>
    /// Obtiene el SKU del producto afectado.
    /// </summary>
    public string Sku { get; }

    /// <summary>
    /// Obtiene el stock disponible al momento de generarse la excepción.
    /// </summary>
    public int StockDisponible { get; }

    /// <summary>
    /// Obtiene la cantidad solicitada que no pudo ser atendida.
    /// </summary>
    public int CantidadSolicitada { get; }
}