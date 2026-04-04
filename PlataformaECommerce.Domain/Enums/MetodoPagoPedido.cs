namespace PlataformaECommerce.Domain.Enums;

/// <summary>
/// Define los métodos de pago seleccionables durante el checkout comercial.
/// </summary>
public enum MetodoPagoPedido
{
    /// <summary>
    /// Pago electrónico con tarjeta de crédito o débito procesado por pasarela.
    /// </summary>
    Tarjeta = 1,

    /// <summary>
    /// Pago por débito bancario inmediato mediante PSE.
    /// </summary>
    Pse = 2,

    /// <summary>
    /// Pago mediante transferencia bancaria manual o conciliada.
    /// </summary>
    TransferenciaBancaria = 3,

    /// <summary>
    /// Pago contra entrega para pedidos físicos elegibles.
    /// </summary>
    ContraEntrega = 4
}
