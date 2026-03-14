using System;

namespace PlataformaECommerce.Domain.Exceptions
{
    public class ProductoNoDisponibleException : ProductException
    {
        /// Identificador del producto afectado.
        public int ProductoId { get; }

        /// Nombre del producto afectado.
        public string NombreProducto { get; }

        /// Indica si el producto se encuentra activo en el sistema.
        public bool Activo { get; }

        /// Stock actual del producto.
        public int Stock { get; }

        /// Inicializa una nueva instancia de la excepción
        /// con información contextual del producto no disponible.
        public ProductoNoDisponibleException(
            int productoId,
            string nombreProducto,
            bool activo,
            int stock)
            : base($"El producto '{nombreProducto}' no está disponible para operar. " +
                   $"Activo: {activo}, stock actual: {stock}.")
        {
            ProductoId = productoId;
            NombreProducto = nombreProducto;
            Activo = activo;
            Stock = stock;
        }
    }
}