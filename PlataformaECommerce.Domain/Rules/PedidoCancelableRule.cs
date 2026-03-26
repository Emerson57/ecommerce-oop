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
/// Un pedido se considera cancelable cuando se encuentra en una etapa previa
/// al despacho físico o cierre definitivo del flujo comercial.
///
/// Esta definición puede servir como base para escenarios futuros
/// en los que se requieran condiciones adicionales, como ventanas
/// temporales de cancelación o estados intermedios restringidos.
/// </remarks>
public static class PedidoCancelableRule
{
    /// <summary>
    /// Evalúa si un pedido puede ser cancelado de acuerdo con su estado actual.
    /// </summary>
    /// <param name="pedido">Pedido a evaluar.</param>
    /// <returns>
    /// <see langword="true"/> si el pedido puede cancelarse;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public static bool IsSatisfiedBy(Pedido? pedido)
    {
        if (pedido is null)
        {
            return false;
        }

        return pedido.Estado is EstadoPedido.Pendiente
            or EstadoPedido.Confirmado
            or EstadoPedido.Pagado
            or EstadoPedido.EnProceso;
    }
}