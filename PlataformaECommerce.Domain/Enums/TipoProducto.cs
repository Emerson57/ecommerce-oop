namespace PlataformaECommerce.Domain.Enums;

/// <summary>
/// Define la clasificación funcional de los productos manejados por el e-commerce.
/// </summary>
/// <remarks>
/// Este enumerador permite distinguir el comportamiento de negocio de un producto
/// dentro del dominio, especialmente en procesos de catálogo, validación, logística,
/// entrega y administración.
/// </remarks>
public enum TipoProducto
{
    /// <summary>
    /// Representa un producto físico que requiere control de inventario material
    /// y puede involucrar procesos de almacenamiento, alistamiento y envío.
    /// </summary>
    Fisico = 1,

    /// <summary>
    /// Representa un producto digital cuya entrega se realiza por medios electrónicos,
    /// como descarga, licencia, acceso virtual o distribución de contenido digital.
    /// </summary>
    Digital = 2
}