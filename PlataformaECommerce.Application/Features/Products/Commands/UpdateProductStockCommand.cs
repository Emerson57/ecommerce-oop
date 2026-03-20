using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.DTOs;

namespace PlataformaECommerce.Application.Features.Products.Commands;

/// <summary>
/// Representa el comando de aplicación para actualizar el inventario
/// de un producto existente dentro del sistema.
/// </summary>
/// <remarks>
/// Este comando modela una intención explícita de escritura sobre el sistema,
/// correspondiente al caso de uso de ajuste de stock de un producto.
///
/// Su propósito es desacoplar la operación de inventario respecto de la
/// actualización general del producto, permitiendo tratar el stock como
/// una responsabilidad funcional independiente.
///
/// El comando admite diferentes tipos de ajuste:
/// - asignación absoluta,
/// - incremento,
/// - decremento.
///
/// La lógica que interpreta el tipo de ajuste y aplica las reglas del dominio
/// debe residir en el servicio de aplicación correspondiente y en la entidad de dominio.
///
/// El resultado esperado de la operación es un <see cref="Result{TValue}"/>
/// que contiene la representación actualizada del producto cuando la ejecución es exitosa.
/// </remarks>
public sealed class UpdateProductStockCommand
{
    #region Identificación

    /// <summary>
    /// Identificador único del producto cuyo inventario será actualizado.
    /// </summary>
    public Guid ProductId { get; init; }

    #endregion

    #region Información de ajuste

    /// <summary>
    /// Tipo de ajuste de inventario que debe aplicarse.
    /// </summary>
    public StockUpdateType UpdateType { get; init; }

    /// <summary>
    /// Valor del ajuste de inventario.
    /// </summary>
    /// <remarks>
    /// Su interpretación depende del valor de <see cref="UpdateType"/>:
    /// - <see cref="StockUpdateType.Set"/>: representa el nuevo stock absoluto,
    /// - <see cref="StockUpdateType.Increase"/>: representa la cantidad a incrementar,
    /// - <see cref="StockUpdateType.Decrease"/>: representa la cantidad a disminuir.
    /// </remarks>
    public int Quantity { get; init; }

    /// <summary>
    /// Motivo funcional del ajuste de inventario.
    /// </summary>
    /// <remarks>
    /// Este campo es útil para trazabilidad, auditoría y comprensión operativa
    /// del cambio realizado sobre el inventario.
    /// </remarks>
    public string Reason { get; init; } = string.Empty;

    #endregion

    #region Contexto opcional

    /// <summary>
    /// Identificador del usuario que solicita el ajuste de inventario.
    /// </summary>
    /// <remarks>
    /// Este valor puede utilizarse para fines de trazabilidad o auditoría
    /// cuando la capa superior desea enviarlo explícitamente.
    /// </remarks>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>
    /// Referencia externa opcional asociada al ajuste de inventario.
    /// </summary>
    /// <remarks>
    /// Puede representar un número de documento, ticket, orden interna
    /// o cualquier correlación funcional con otros sistemas o procesos.
    /// </remarks>
    public string? ExternalReference { get; init; }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del comando.
    /// </summary>
    /// <returns>Cadena representativa del comando de actualización de stock.</returns>
    public override string ToString()
    {
        return $"UpdateProductStockCommand | ProductId: {ProductId} | UpdateType: {UpdateType} | Quantity: {Quantity} | Reason: {Reason}";
    }

    #endregion
}

/// <summary>
/// Define los tipos de ajuste de inventario soportados por la capa de aplicación.
/// </summary>
/// <remarks>
/// Este enum permite expresar de forma clara la intención del cambio sobre el stock,
/// evitando ambigüedad en el procesamiento del comando.
/// </remarks>
public enum StockUpdateType
{
    /// <summary>
    /// Establece un valor absoluto de stock.
    /// </summary>
    Set = 1,

    /// <summary>
    /// Incrementa el stock actual en la cantidad indicada.
    /// </summary>
    Increase = 2,

    /// <summary>
    /// Disminuye el stock actual en la cantidad indicada.
    /// </summary>
    Decrease = 3
}