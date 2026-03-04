using System;

namespace PlataformaECommerce.Dominio
{
    public abstract class Producto
    {
        #region Campos privados (estado interno / encapsulación)

        private int _id;
        private string _nombre = string.Empty;
        private string _descripcion = string.Empty;
        private decimal _precio;
        private int _stock;

        #endregion

        #region Propiedades públicas (API segura del objeto)

        /// Identificador único del producto.
        public int Id
        {
            get => _id;
            private set
            {
                if (value <= 0)
                    throw new ArgumentException("El Id debe ser mayor que 0.", nameof(Id));

                _id = value;
            }
        }

        /// Nombre del producto.
        public string Nombre
        {
            get => _nombre;
            protected set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El nombre no puede estar vacío.", nameof(Nombre));

                _nombre = value.Trim();
            }
        }

        /// Descripción del producto.
        public string Descripcion
        {
            get => _descripcion;
            protected set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("La descripción no puede estar vacía.", nameof(Descripcion));

                _descripcion = value.Trim();
            }
        }

        /// Precio del producto.
        public decimal Precio
        {
            get => _precio;
            protected set
            {
                if (value <= 0)
                    throw new ArgumentException("El precio debe ser mayor que 0.", nameof(Precio));

                _precio = value;
            }
        }

        /// Stock disponible del producto.
        public int Stock
        {
            get => _stock;
            protected set
            {
                if (value < 0)
                    throw new ArgumentException("El stock no puede ser negativo.", nameof(Stock));

                _stock = value;
            }
        }

        #endregion

        #region Constructor
        /// Constructor protegido
        protected Producto(int id, string nombre, string descripcion, decimal precio, int stock)
        {
            // Asignación con validaciones (mediante propiedades)
            Id = id;
            Nombre = nombre;
            Descripcion = descripcion;
            Precio = precio;
            Stock = stock;
        }

        #endregion

        #region Métodos de negocio (operaciones seguras sobre el estado)

        /// Incrementa el stock del producto.
        public void AumentarStock(int cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que 0.", nameof(cantidad));

            checked
            {
                Stock += cantidad;
            }
        }
        
        /// Disminuye el stock del producto.
        public void DisminuirStock(int cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que 0.", nameof(cantidad));

            if (cantidad > Stock)
                throw new InvalidOperationException("No hay stock suficiente para disminuir esa cantidad.");

            Stock -= cantidad;
        }

        /// Actualiza el precio del producto aplicando la validación correspondiente.
        public void ActualizarPrecio(decimal nuevoPrecio)
        {
            Precio = nuevoPrecio; // Validación en la propiedad
        }

        /// Indica si el producto tiene stock disponible.
        public bool TieneStock() => Stock > 0;

        /// Método virtual para que las clases derivadas puedan personalizar una descripción "detallada".
        public virtual string ObtenerDescripcionDetallada()
            => $"{Nombre} - {Descripcion}";

        #endregion

        #region Representación de texto

        /// Representación corta del producto (para logs, debugging o salida en consola).
        public override string ToString()
            => $"Producto: {Id} - {Nombre} | Precio: {Precio:C} | Stock: {Stock}";

        #endregion
    }
}