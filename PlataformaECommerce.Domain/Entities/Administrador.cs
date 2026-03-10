using System;

namespace PlataformaECommerce.Domain.Entities
{
    public sealed class Administrador : Usuario
    {
        #region Constantes de negocio

        /// Longitud mínima permitida para el área del administrador.
        private const int LongitudMinimaArea = 3;

        /// Longitud máxima permitida para el área del administrador.
        private const int LongitudMaximaArea = 60;

        /// Porcentaje máximo de descuento permitido para promociones.
        private const decimal PorcentajeMaximoDescuento = 90m;

        #endregion

        #region Campos privados

        /// Área o departamento al que pertenece el administrador.
        private string _area = string.Empty;

        #endregion

        #region Constructores

        /// Constructor protegido sin parámetros.
        protected Administrador()
        {
        }

        /// Crea una nueva instancia de Administrador con sus datos base
        /// y el área a la que pertenece.
        public Administrador(int id, string nombre, string correo, string contrasena, string area = "Operaciones")
            : base(id, nombre, correo, contrasena)
        {
            _area = ValidarArea(area);
        }

        #endregion

        #region Propiedades públicas

        /// Área o departamento del administrador.
        public string Area => _area;

        #endregion

        #region Métodos de negocio

        /// Actualiza el área del administrador.
        public void ActualizarArea(string nuevaArea)
        {
            _area = ValidarArea(nuevaArea);
            ActualizarFechaModificacion();
        }

        /// Gestiona el inventario de un producto estableciendo un nuevo stock absoluto.
        public void GestionarInventario(Producto producto, int nuevoStock)
        {
            if (producto is null)
                throw new ArgumentNullException(nameof(producto), "El producto no puede ser nulo.");

            if (nuevoStock < 0)
                throw new ArgumentOutOfRangeException(nameof(nuevoStock), "El nuevo stock no puede ser negativo.");

            if (nuevoStock > producto.Stock)
            {
                int diferencia = nuevoStock - producto.Stock;
                producto.ReponerStock(diferencia);
            }
            else if (nuevoStock < producto.Stock)
            {
                int diferencia = producto.Stock - nuevoStock;
                producto.ReducirStock(diferencia);
            }

            ActualizarFechaModificacion();
        }

        /// Aplica una promoción a un producto reduciendo su precio según
        /// un porcentaje de descuento permitido.
        public void EstablecerPromocion(Producto producto, decimal porcentajeDescuento)
        {
            if (producto is null)
                throw new ArgumentNullException(nameof(producto), "El producto no puede ser nulo.");

            decimal descuentoValidado = ValidarPorcentajeDescuento(porcentajeDescuento);

            decimal precioOriginal = producto.Precio;
            decimal factorDescuento = 1m - (descuentoValidado / 100m);
            decimal nuevoPrecio = Math.Round(precioOriginal * factorDescuento, 2, MidpointRounding.AwayFromZero);

            if (nuevoPrecio <= 0)
                throw new InvalidOperationException("El precio resultante de la promoción debe ser mayor que cero.");

            producto.ActualizarPrecio(nuevoPrecio);
            ActualizarFechaModificacion();
        }

        /// Activa un producto dentro del catálogo.
        public void ActivarProducto(Producto producto)
        {
            if (producto is null)
                throw new ArgumentNullException(nameof(producto), "El producto no puede ser nulo.");

            producto.Activar();
            ActualizarFechaModificacion();
        }

        /// Desactiva un producto dentro del catálogo.
        public void DesactivarProducto(Producto producto)
        {
            if (producto is null)
                throw new ArgumentNullException(nameof(producto), "El producto no puede ser nulo.");

            producto.Desactivar();
            ActualizarFechaModificacion();
        }

        #endregion

        #region Overrides

        /// Devuelve el rol específico del usuario dentro del sistema.
        public override string ObtenerRol()
        {
            return "Administrador";
        }

        /// Devuelve una representación legible del perfil del administrador
        /// incluyendo su área de trabajo.
        public override string MostrarPerfil()
        {
            return $"{base.MostrarPerfil()} | Área: {Area}";
        }

        /// Devuelve una representación corta del administrador.
        public override string ToString()
        {
            return $"{Nombre} ({Correo}) - Administrador | Área: {Area}";
        }

        #endregion

        #region Métodos privados de validación

        /// Valida y normaliza el área del administrador.
        private static string ValidarArea(string area)
        {
            if (string.IsNullOrWhiteSpace(area))
                throw new ArgumentException("El área del administrador es obligatoria.", nameof(area));

            string areaNormalizada = area.Trim();

            if (areaNormalizada.Length < LongitudMinimaArea)
                throw new ArgumentException($"El área del administrador debe tener al menos {LongitudMinimaArea} caracteres.", nameof(area));

            if (areaNormalizada.Length > LongitudMaximaArea)
                throw new ArgumentException($"El área del administrador no puede superar los {LongitudMaximaArea} caracteres.", nameof(area));

            return areaNormalizada;
        }

        /// Valida que el porcentaje de descuento esté dentro del rango permitido.
        private static decimal ValidarPorcentajeDescuento(decimal porcentajeDescuento)
        {
            if (porcentajeDescuento <= 0 || porcentajeDescuento > PorcentajeMaximoDescuento)
                throw new ArgumentOutOfRangeException(
                    nameof(porcentajeDescuento),
                    $"El porcentaje de descuento debe estar entre 0 y {PorcentajeMaximoDescuento}."
                );

            return decimal.Round(porcentajeDescuento, 2, MidpointRounding.AwayFromZero);
        }

        #endregion
    }
}