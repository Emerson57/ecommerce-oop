using System;

namespace PlataformaECommerce.Dominio
{
    public sealed class Administrador : Usuario
    {
        #region Campos privados

        /// Nombre del área o departamento del administrador (opcional).
        private readonly string _area;

        #endregion

        #region Propiedades públicas

        /// Área del administrador (solo lectura).
        public string Area => _area;

        #endregion

        #region Constructor

        /// Crea un administrador inicializando los datos base del usuario.
        public Administrador(int id, string nombre, string correo, string contrasena, string area = "Operaciones")
            : base(id, nombre, correo, contrasena)
        {
            if (string.IsNullOrWhiteSpace(area))
                throw new ArgumentException("El área del administrador no puede estar vacía.", nameof(area));

            _area = area.Trim();
        }

        #endregion

        #region Métodos requeridos (enunciado)

        /// Gestiona el inventario de un producto actualizando su stock.
        public void GestionarInventario(Producto producto, int nuevoStock)
        {
            if (producto is null)
                throw new ArgumentNullException(nameof(producto), "El producto no puede ser null.");

            if (nuevoStock < 0)
                throw new ArgumentException("El nuevo stock no puede ser negativo.", nameof(nuevoStock));

            // Ajustar stock mediante Aumentar/Disminuir.
            if (nuevoStock > producto.Stock)
            {
                var diferencia = nuevoStock - producto.Stock;
                producto.AumentarStock(diferencia);
            }
            else if (nuevoStock < producto.Stock)
            {
                var diferencia = producto.Stock - nuevoStock;
                producto.DisminuirStock(diferencia);
            }
            // Si es igual, no se hace nada.
        }

        /// Establece una promoción aplicando un descuento porcentual al precio del producto.
        public void EstablecerPromocion(Producto producto, decimal porcentajeDescuento)
        {
            if (producto is null)
                throw new ArgumentNullException(nameof(producto), "El producto no puede ser null.");

            if (porcentajeDescuento <= 0 || porcentajeDescuento > 90)
                throw new ArgumentException("El porcentaje de descuento debe estar entre 1 y 90.", nameof(porcentajeDescuento));

            var precioOriginal = producto.Precio;

            // Calcular nuevo precio: precio * (1 - descuento/100)
            var factor = 1m - (porcentajeDescuento / 100m);
            var nuevoPrecio = Math.Round(precioOriginal * factor, 2, MidpointRounding.AwayFromZero);

            if (nuevoPrecio <= 0)
                throw new InvalidOperationException("El precio con descuento no puede ser menor o igual a 0.");

            producto.ActualizarPrecio(nuevoPrecio);
        }

        #endregion

        #region Overrides (polimorfismo)

        /// Rol específico del usuario en el sistema.
        public override string ObtenerRol() => "Administrador";

        /// Presenta el perfil del administrador con información adicional.
        public override string MostrarPerfil()
            => $"{base.MostrarPerfil()} | Área: {Area}";

        #endregion
    }
}