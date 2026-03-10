using System;

namespace PlataformaECommerce.Domain.Entities
{
    public sealed class ProductoFisico : Producto
    {
        #region Campos privados

        private decimal _pesoKg;
        private decimal _altoCm;
        private decimal _anchoCm;
        private decimal _largoCm;

        #endregion

        #region Constructores

        /// Constructor protegido sin parámetros.
        protected ProductoFisico()
        {
        }

        /// Crea una nueva instancia de un producto físico con sus datos base
        public ProductoFisico(
            int id,
            string nombre,
            string descripcion,
            decimal precio,
            int stock,
            decimal pesoKg,
            decimal altoCm,
            decimal anchoCm,
            decimal largoCm)
            : base(id, nombre, descripcion, precio, stock)
        {
            _pesoKg = ValidarMedidaPositiva(pesoKg, nameof(pesoKg), "El peso del producto debe ser mayor que cero.");
            _altoCm = ValidarMedidaPositiva(altoCm, nameof(altoCm), "El alto del producto debe ser mayor que cero.");
            _anchoCm = ValidarMedidaPositiva(anchoCm, nameof(anchoCm), "El ancho del producto debe ser mayor que cero.");
            _largoCm = ValidarMedidaPositiva(largoCm, nameof(largoCm), "El largo del producto debe ser mayor que cero.");
        }

        #endregion

        #region Propiedades públicas

        /// Peso del producto en kilogramos.
        public decimal PesoKg => _pesoKg;

        /// Alto del producto en centímetros.
        public decimal AltoCm => _altoCm;

        /// Ancho del producto en centímetros.
        public decimal AnchoCm => _anchoCm;

        /// Largo del producto en centímetros.
        public decimal LargoCm => _largoCm;

        /// Volumen aproximado del producto en centímetros cúbicos.
        public decimal VolumenCm3 => _altoCm * _anchoCm * _largoCm;

        #endregion

        #region Métodos de negocio

        /// Actualiza los datos físicos y logísticos del producto.
        public void ActualizarInformacionFisica(decimal pesoKg, decimal altoCm, decimal anchoCm, decimal largoCm)
        {
            _pesoKg = ValidarMedidaPositiva(pesoKg, nameof(pesoKg), "El peso del producto debe ser mayor que cero.");
            _altoCm = ValidarMedidaPositiva(altoCm, nameof(altoCm), "El alto del producto debe ser mayor que cero.");
            _anchoCm = ValidarMedidaPositiva(anchoCm, nameof(anchoCm), "El ancho del producto debe ser mayor que cero.");
            _largoCm = ValidarMedidaPositiva(largoCm, nameof(largoCm), "El largo del producto debe ser mayor que cero.");

            ActualizarFechaModificacion();
        }

        /// Determina si el producto puede considerarse voluminoso según su volumen.
        public bool EsVoluminoso() => VolumenCm3 > 100000;

        /// Devuelve una descripción detallada del producto físico incluyendo
        public override string ObtenerDescripcionDetallada()
        {
            return $"{base.ObtenerDescripcionDetallada()} | Peso: {PesoKg:0.###} Kg | Dimensiones: {AltoCm:0.##} x {AnchoCm:0.##} x {LargoCm:0.##} cm | Volumen: {VolumenCm3:0.##} cm³";
        }

        #endregion

        #region Métodos privados de validación

        /// Valida que una medida física sea mayor que cero y la redondea a dos decimales.
        private static decimal ValidarMedidaPositiva(decimal valor, string nombreParametro, string mensajeError)
        {
            if (valor <= 0)
                throw new ArgumentOutOfRangeException(nombreParametro, mensajeError);

            return decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
        }

        #endregion

        #region Representación textual

        /// Devuelve una representación corta del producto físico.
        public override string ToString()
        {
            return $"{base.ToString()} | Físico: {PesoKg:0.###} Kg ({AltoCm:0.##} x {AnchoCm:0.##} x {LargoCm:0.##} cm)";
        }

        #endregion
    }
}