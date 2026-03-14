using System;

namespace PlataformaECommerce.Domain.Exceptions
{
    public class InventarioInsuficienteException : ProductException
    {
        /// Identificador del producto afectado.
        public int ProductoId { get; }

        /// Nombre del producto afectado.
        public string NombreProducto { get; }

        /// Cantidad disponible en inventario.
        public int StockDisponible { get; }

        /// Cantidad que se intentó descontar.
        public int CantidadSolicitada { get; }

        /// Inicializa una nueva instancia de la excepción
        /// indicando los detalles del error de inventario.
        public InventarioInsuficienteException(
            int productoId,
            string nombreProducto,
            int stockDisponible,
            int cantidadSolicitada)
            : base($"Inventario insuficiente para el producto '{nombreProducto}'. " +
                   $"Stock disponible: {stockDisponible}, cantidad solicitada: {cantidadSolicitada}.")
        {
            ProductoId = productoId;
            NombreProducto = nombreProducto;
            StockDisponible = stockDisponible;
            CantidadSolicitada = cantidadSolicitada;
        }
    }
}