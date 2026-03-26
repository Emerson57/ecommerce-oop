namespace PlataformaECommerce.Domain.Exceptions;

/// <summary>
/// Representa el error generado cuando se intenta operar un carrito que no contiene productos.
/// </summary>
/// <remarks>
/// Esta excepción se utiliza especialmente en procesos de confirmación de pedido,
/// cálculo de totales o validaciones previas a la compra, donde el carrito debe
/// contener al menos un ítem válido.
/// </remarks>
public class CarritoVacioException : CartException
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CarritoVacioException"/>.
    /// </summary>
    public CarritoVacioException()
        : base("No es posible procesar la operación porque el carrito no contiene productos.")
    {
    }
}