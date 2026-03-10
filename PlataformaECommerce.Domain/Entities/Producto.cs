using System;

namespace PlataformaECommerce.Domain.Entities
{
    public abstract class Producto
    {
        #region Constantes de negocio
        /// Longitud mínima permitida para el nombre del producto.
        private const int LongitudMinimaNombre = 3;

        /// Longitud máxima permitida para el nombre del producto.
        private const int LongitudMaximaNombre = 150;

        /// Longitud máxima permitida para la descripción del producto.
        private const int LongitudMaximaDescripcion = 500;

        #endregion

        #region Campos privados (estado interno encapsulado)

        private int _id;
        private string _nombre = string.Empty;
        private string _descripcion = string.Empty;
        private decimal _precio;
        private int _stock;
        private bool _activo;
        private DateTime _fechaCreacion;
        private DateTime _fechaActualizacion;

        #endregion

        #region Constructor

        /// Constructor protegido sin parámetros.
        protected Producto()
        {
        }

        /// Constructor principal de la entidad Producto.
        protected Producto(int id, string nombre, string descripcion, decimal precio, int stock)
        {
            Id = ValidarId(id);
            _nombre = ValidarNombre(nombre);
            _descripcion = ValidarDescripcion(descripcion);
            _precio = ValidarPrecio(precio);
            _stock = ValidarStock(stock);
            _activo = true;

            _fechaCreacion = DateTime.UtcNow;
            _fechaActualizacion = DateTime.UtcNow;
        }

        #endregion

        #region Propiedades públicas

        /// Identificador único del producto.
        public int Id
        {
            get => _id;
            private set => _id = value;
        }

        /// Nombre del producto.
        public string Nombre => _nombre;

        /// Descripción general del producto.
        public string Descripcion => _descripcion;

        /// Precio unitario actual del producto.
        public decimal Precio => _precio;

        /// Stock disponible del producto.
        public int Stock => _stock;

        /// Indica si el producto está activo para su uso o visualización dentro del sistema.
        public bool Activo => _activo;

        /// Fecha de creación de la entidad en el sistema.
        public DateTime FechaCreacion => _fechaCreacion;

        /// Fecha de la última actualización relevante del producto.
        public DateTime FechaActualizacion => _fechaActualizacion;

        #endregion

        #region Métodos de negocio

        /// Actualiza la información básica editable del producto.
        public void ActualizarInformacionBasica(string nombre, string descripcion)
        {
            _nombre = ValidarNombre(nombre);
            _descripcion = ValidarDescripcion(descripcion);
            ActualizarFechaModificacion();
        }

        /// Actualiza el precio del producto.
        public void ActualizarPrecio(decimal nuevoPrecio)
        {
            _precio = ValidarPrecio(nuevoPrecio);
            ActualizarFechaModificacion();
        }

        /// Incrementa el stock del producto.
        public void ReponerStock(int cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad a reponer debe ser mayor que cero.");

            checked
            {
                _stock += cantidad;
            }

            ActualizarFechaModificacion();
        }

        /// Reduce el stock del producto.
        public void ReducirStock(int cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad a reducir debe ser mayor que cero.");

            if (cantidad > _stock)
                throw new InvalidOperationException("No es posible reducir el stock porque la cantidad solicitada excede la disponibilidad actual.");

            _stock -= cantidad;
            ActualizarFechaModificacion();
        }

        /// Establece un valor absoluto para el stock del producto.
        public void ActualizarStock(int nuevoStock)
        {
            _stock = ValidarStock(nuevoStock);
            ActualizarFechaModificacion();
        }

        /// Marca el producto como activo.
        public void Activar()
        {
            if (_activo)
                return;

            _activo = true;
            ActualizarFechaModificacion();
        }

        /// Marca el producto como inactivo.
        public void Desactivar()
        {
            if (!_activo)
                return;

            _activo = false;
            ActualizarFechaModificacion();
        }

        /// Indica si el producto tiene unidades disponibles para venta o uso.
        public bool TieneStock() => _stock > 0;

        /// Indica si el producto se encuentra disponible para operar en el sistema.
        public bool EstaDisponible() => _activo && TieneStock();

        /// Devuelve una descripción ampliada del producto.
        public virtual string ObtenerDescripcionDetallada()
        {
            return $"{Nombre} - {Descripcion} | Precio: {Precio:C} | Stock: {Stock} | Activo: {Activo}";
        }

        #endregion

        #region Métodos privados de validación

        /// Valida que el Id sea mayor que cero.
        private static int ValidarId(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El Id del producto debe ser mayor que cero.");

            return id;
        }

        /// Valida el nombre del producto según reglas de negocio.
        private static string ValidarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del producto es obligatorio.", nameof(nombre));

            string nombreNormalizado = nombre.Trim();

            if (nombreNormalizado.Length < LongitudMinimaNombre)
                throw new ArgumentException($"El nombre del producto debe tener al menos {LongitudMinimaNombre} caracteres.", nameof(nombre));

            if (nombreNormalizado.Length > LongitudMaximaNombre)
                throw new ArgumentException($"El nombre del producto no puede superar los {LongitudMaximaNombre} caracteres.", nameof(nombre));

            return nombreNormalizado;
        }

        /// Valida la descripción del producto.
        private static string ValidarDescripcion(string descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                throw new ArgumentException("La descripción del producto es obligatoria.", nameof(descripcion));

            string descripcionNormalizada = descripcion.Trim();

            if (descripcionNormalizada.Length > LongitudMaximaDescripcion)
                throw new ArgumentException($"La descripción del producto no puede superar los {LongitudMaximaDescripcion} caracteres.", nameof(descripcion));

            return descripcionNormalizada;
        }

        /// Valida el precio del producto.
        private static decimal ValidarPrecio(decimal precio)
        {
            if (precio <= 0)
                throw new ArgumentOutOfRangeException(nameof(precio), "El precio del producto debe ser mayor que cero.");

            return decimal.Round(precio, 2, MidpointRounding.AwayFromZero);
        }

        /// Valida que el stock no sea negativo.
        private static int ValidarStock(int stock)
        {
            if (stock < 0)
                throw new ArgumentOutOfRangeException(nameof(stock), "El stock del producto no puede ser negativo.");

            return stock;
        }

        /// Actualiza la fecha de modificación de la entidad.
        protected void ActualizarFechaModificacion()
        {
            _fechaActualizacion = DateTime.UtcNow;
        }

        #endregion

        #region Representación textual

        /// Devuelve una representación corta y útil del producto.
        public override string ToString()
        {
            return $"Producto: {Id} - {Nombre} | Precio: {Precio:C} | Stock: {Stock} | Activo: {Activo}";
        }

        #endregion
    }
}