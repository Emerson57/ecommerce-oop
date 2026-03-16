namespace PlataformaECommerce.Domain.Enums;

/// <summary>
/// Define los estados posibles del ciclo de vida de un pedido dentro del e-commerce.
/// </summary>
/// <remarks>
/// Este enumerador representa la evolución operativa y comercial de un pedido,
/// desde su creación inicial hasta su cierre exitoso o cancelación. Su propósito
/// es estandarizar el flujo del pedido dentro del dominio y facilitar validaciones,
/// auditoría, trazabilidad y reglas de negocio.
/// </remarks>
public enum EstadoPedido
{
    /// <summary>
    /// El pedido ha sido creado en el sistema, pero todavía no ha sido confirmado
    /// ni procesado para su ejecución operativa.
    /// </summary>
    Pendiente = 1,

    /// <summary>
    /// El pedido ha sido validado y confirmado por el sistema o por el usuario,
    /// quedando listo para continuar con el flujo comercial correspondiente.
    /// </summary>
    Confirmado = 2,

    /// <summary>
    /// El pago asociado al pedido ha sido aprobado o registrado satisfactoriamente.
    /// </summary>
    Pagado = 3,

    /// <summary>
    /// El pedido se encuentra en preparación, alistamiento, validación interna
    /// o ejecución del proceso previo a su entrega final.
    /// </summary>
    EnProceso = 4,

    /// <summary>
    /// El pedido ha sido despachado o enviado al cliente mediante el canal de entrega definido.
    /// </summary>
    Enviado = 5,

    /// <summary>
    /// El pedido ha sido entregado satisfactoriamente al cliente o completado de forma definitiva.
    /// </summary>
    Entregado = 6,

    /// <summary>
    /// El pedido ha sido cancelado y no continuará dentro del flujo operativo del sistema.
    /// </summary>
    Cancelado = 7
}