using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Domain.Rules;

/// <summary>
/// Representa la regla de negocio que determina si un pedido
/// puede ser cancelado dentro del flujo comercial.
/// </summary>
/// <remarks>
/// Esta regla encapsula la validación funcional de cancelación del pedido,
/// evitando que dicha lógica quede dispersa en múltiples capas del sistema.
///
/// Un pedido se considera cancelable cuando:
/// - existe,
/// - no se encuentra ya cancelado,
/// - y no ha sido entregado.
///
/// Esta definición puede servir como base para escenarios futuros
/// en los que se requieran condiciones adicionales, como ventanas
/// temporales de cancelación o estados intermedios restringidos.
/// </remarks>
public sealed class PedidoCancelableRule
{
    /// <summary>
    /// Evalúa si un pedido puede ser cancelado de acuerdo con su estado actual.
    /// </summary>
    /// <param name="pedido">Pedido a evaluar.</param>
    /// <returns>
    /// <see langword="true"/> si el pedido puede cancelarse;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool IsSatisfiedBy(Pedido? pedido)
    {
        if (pedido is null)
        {
            return false;
        }

        return pedido.Estado is not EstadoPedido.Cancelado
            && pedido.Estado is not EstadoPedido.Entregado;
    }

    /// <summary>
    /// Obtiene una descripción funcional de la regla.
    /// </summary>
    /// <returns>Texto descriptivo de la regla.</returns>
    public override string ToString()
    {
        return "Un pedido puede cancelarse siempre que no se encuentre entregado ni previamente cancelado.";
    }
}