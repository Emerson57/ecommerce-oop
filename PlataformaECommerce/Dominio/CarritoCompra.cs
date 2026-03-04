using System;
using System.Collections.Generic;
using System.Linq;

namespace PlataformaECommerce.Dominio
{
    public class CarritoCompra
    {
        #region Campos privados (estado interno)

        /// Lista interna de productos (se mantiene privada para proteger invariantes).
        private readonly List<Producto> _productos;

        #endregion

        #region Propiedades

        /// Total monetario del carrito.
        public decimal Total { get; private set; }

        /// Productos del carrito en modo solo lectura.
        public IReadOnlyList<Producto> Productos => _productos.AsReadOnly();

        /// Cantidad total de ítems en el carrito (conteo simple de entradas).
        public int CantidadItems => _productos.Count;

        #endregion

        #region Constructor

        /// Inicializa un carrito vacío.
        public CarritoCompra()
        {
            _productos = new List<Producto>();
            Total = 0m;
        }

        #endregion

        #region Operaciones principales

        /// Agrega un producto al carrito.
        public void AgregarProducto(Producto producto)
        {
            if (producto is null)
                throw new ArgumentNullException(nameof(producto), "El producto no puede ser null.");

            // validamos que exista stock al agregar.
            if (producto.Stock <= 0)
                throw new InvalidOperationException("No se puede agregar el producto porque no hay stock disponible.");

            _productos.Add(producto);
            RecalcularTotal();
        }

        /// Remueve la primera ocurrencia de un producto del carrito por Id.
        public bool RemoverProducto(int idProducto)
        {
            if (idProducto <= 0)
                throw new ArgumentException("El Id del producto debe ser mayor que 0.", nameof(idProducto));

            var producto = _productos.FirstOrDefault(p => p.Id == idProducto);

            if (producto is null)
                return false;

            _productos.Remove(producto);
            RecalcularTotal();
            return true;
        }

        /// Vacía el carrito por completo.
        public void VaciarCarrito()
        {
            _productos.Clear();
            Total = 0m;
        }

        #endregion

        #region Cálculo de total (interno)

        /// Recalcula el total del carrito.
        private void RecalcularTotal()
        {
            Total = _productos.Sum(p => p.Precio);
        }

        #endregion
    }
}