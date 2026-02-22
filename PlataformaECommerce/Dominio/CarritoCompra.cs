using System;
using System.Collections.Generic;
using System.Text;

namespace PlataformaECommerce.Dominio
{
    public class CarritoCompra
    {
        // 1) Lista de productos seleccionados
        private List<Producto> _productos;

        // 2) Total del carrito
        public decimal Total { get; private set; }

        // 3) Exponemos los productos en modo solo lectura (buena práctica)
        public IReadOnlyList<Producto> Productos => _productos.AsReadOnly();

        // 4) Constructor
        public CarritoCompra()
        {
            _productos = new List<Producto>();
            Total = 0;
        }

        // 5) Método para agregar un producto
        public void AgregarProducto(Producto producto)
        {
            if (producto == null)
                throw new ArgumentNullException(nameof(producto), "El producto no puede ser null.");

            // Validación mínima: si no hay stock, no se puede agregar
            if (producto.Stock <= 0)
                throw new InvalidOperationException("No se puede agregar el producto porque no hay stock disponible.");

            _productos.Add(producto);
            CalcularTotal();
        }

        // 6) Método para remover producto por id
        public bool RemoverProducto(int idProducto)
        {
            var producto = _productos.FirstOrDefault(p => p.Id == idProducto);

            if (producto == null)
                return false;

            _productos.Remove(producto);
            CalcularTotal();
            return true;
        }

        // 7) Calcular total del carrito
        public void CalcularTotal()
        {
            Total = _productos.Sum(p => p.Precio);
        }

        // 8) Vaciar carrito
        public void VaciarCarrito()
        {
            _productos.Clear();
            Total = 0;
        }
    }
}
