using System;
using System.Collections.Generic;
using System.Linq;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Domain.Entities.Cart
{
    public class CarritoCompra
    {
        #region Constantes de negocio

        /// Cantidad máxima de ítems permitidos en el carrito.
        private const int MaximoItemsPermitidos = 100;

        #endregion

        #region Campos privados

        /// Lista interna de productos agregados al carrito.
        private readonly List<Producto> _productos = new();

        /// Total acumulado del carrito.
        private decimal _total;

        /// Indica si el carrito está activo para recibir operaciones.
        private bool _activo;

        /// Fecha de creación del carrito.
        private DateTime _fechaCreacion;

        /// Fecha de la última actualización del carrito.
        private DateTime _fechaActualizacion;

        #endregion

        #region Constructores

        /// Constructor protegido sin parámetros.
        protected CarritoCompra()
        {
        }

        /// Inicializa un carrito vacío y activo.
        public CarritoCompra(bool activo = true)
        {
            _activo = activo;
            _fechaCreacion = DateTime.UtcNow;
            _fechaActualizacion = DateTime.UtcNow;
            _total = 0m;
        }

        #endregion

        #region Propiedades públicas

        /// Total monetario actual del carrito.
        public decimal Total => _total;

        /// Productos del carrito en modo solo lectura.
        public IReadOnlyList<Producto> Productos => _productos.AsReadOnly();

        /// Cantidad total de ítems en el carrito.
        public int CantidadItems => _productos.Count;

        /// Indica si el carrito está activo.
        public bool Activo => _activo;

        /// Fecha de creación del carrito.
        public DateTime FechaCreacion => _fechaCreacion;

        /// Fecha de la última actualización del carrito.
        public DateTime FechaActualizacion => _fechaActualizacion;

        #endregion

        #region Métodos de negocio

        /// Agrega un producto al carrito.
        public void AgregarProducto(Producto producto)
        {
            ValidarCarritoActivo();

            if (producto is null)
                throw new ProductException("El producto no puede ser nulo.");

            if (!producto.EstaDisponible())
                throw new ProductoNoDisponibleException(
                    producto.Id,
                    producto.Nombre,
                    producto.Activo,
                    producto.Stock
                );

            if (_productos.Count >= MaximoItemsPermitidos)
                throw new CartException($"No es posible agregar más de {MaximoItemsPermitidos} ítems al carrito.");

            _productos.Add(producto);
            RecalcularTotal();
            ActualizarFechaModificacion();
        }

        /// Remueve la primera ocurrencia de un producto del carrito según su identificador.
        public bool RemoverProducto(int idProducto)
        {
            ValidarCarritoActivo();

            if (idProducto <= 0)
                throw new CartException("El Id del producto debe ser mayor que cero.");

            Producto? producto = _productos.FirstOrDefault(p => p.Id == idProducto);

            if (producto is null)
                return false;

            _productos.Remove(producto);
            RecalcularTotal();
            ActualizarFechaModificacion();
            return true;
        }

        /// Elimina todas las entradas del carrito.
        public void VaciarCarrito()
        {
            ValidarCarritoActivo();

            if (_productos.Count == 0)
                throw new CarritoVacioException("No se puede vaciar el carrito porque ya está vacío.");

            _productos.Clear();
            _total = 0m;
            ActualizarFechaModificacion();
        }

        /// Verifica si el carrito contiene al menos una ocurrencia del producto indicado.
        public bool ContieneProducto(int idProducto)
        {
            if (idProducto <= 0)
                return false;

            return _productos.Any(p => p.Id == idProducto);
        }

        /// Obtiene la cantidad de veces que un producto aparece en el carrito.
        public int ObtenerCantidadDeProducto(int idProducto)
        {
            if (idProducto <= 0)
                throw new CartException("El Id del producto debe ser mayor que cero.");

            return _productos.Count(p => p.Id == idProducto);
        }

        /// Obtiene la primera ocurrencia de un producto dentro del carrito.
        public Producto? ObtenerProductoPorId(int idProducto)
        {
            if (idProducto <= 0)
                return null;

            return _productos.FirstOrDefault(p => p.Id == idProducto);
        }

        /// Calcula y devuelve el total actual del carrito.
        public decimal CalcularTotal()
        {
            RecalcularTotal();
            return _total;
        }

        /// Activa el carrito para permitir operaciones nuevamente.
        public void Activar()
        {
            if (_activo)
                return;

            _activo = true;
            ActualizarFechaModificacion();
        }

        /// Desactiva el carrito de forma lógica.
        public void Desactivar()
        {
            if (!_activo)
                return;

            _activo = false;
            ActualizarFechaModificacion();
        }

        #endregion

        #region Métodos privados auxiliares

        /// Recalcula el total sumando el precio de todos los productos del carrito.
        private void RecalcularTotal()
        {
            _total = _productos.Sum(p => p.Precio);
        }

        /// Valida que el carrito esté activo antes de ejecutar operaciones que modifiquen su estado.
        private void ValidarCarritoActivo()
        {
            if (!_activo)
                throw new CartException("No se puede realizar la operación porque el carrito está inactivo.");
        }

        /// Actualiza la fecha de modificación del carrito.
        private void ActualizarFechaModificacion()
        {
            _fechaActualizacion = DateTime.UtcNow;
        }

        #endregion

        #region Representación textual

        /// Devuelve una representación corta y útil del carrito.
        public override string ToString()
        {
            return $"Carrito | Ítems: {CantidadItems} | Total: {Total:C} | Activo: {Activo}";
        }

        #endregion
    }
}