using System;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Domain.Entities.Products
{
    public sealed class ProductoFisico : Producto
    {
        #region Constantes de negocio

        /// Peso máximo permitido para productos físicos (Kg).
        private const decimal PesoMaximoKg = 1000;

        /// Dimensión máxima permitida para productos físicos (cm).
        private const decimal DimensionMaximaCm = 500;

        #endregion

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
            _pesoKg = ValidarPeso(pesoKg);
            _altoCm = ValidarDimension(altoCm, nameof(altoCm));
            _anchoCm = ValidarDimension(anchoCm, nameof(anchoCm));
            _largoCm = ValidarDimension(largoCm, nameof(largoCm));
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
            _pesoKg = ValidarPeso(pesoKg);
            _altoCm = ValidarDimension(altoCm, nameof(altoCm));
            _anchoCm = ValidarDimension(anchoCm, nameof(anchoCm));
            _largoCm = ValidarDimension(largoCm, nameof(largoCm));

            ActualizarFechaModificacion();
        }

        /// Determina si el producto puede considerarse voluminoso según su volumen.
        public bool EsVoluminoso() => VolumenCm3 > 100000;

        /// Devuelve una descripción detallada del producto físico incluyendo
        /// información logística relevante.
        public override string ObtenerDescripcionDetallada()
        {
            return $"{base.ObtenerDescripcionDetallada()} | Peso: {PesoKg:0.###} Kg | Dimensiones: {AltoCm:0.##} x {AnchoCm:0.##} x {LargoCm:0.##} cm | Volumen: {VolumenCm3:0.##} cm³";
        }

        #endregion

        #region Métodos privados de validación

        /// Valida el peso del producto.
        private static decimal ValidarPeso(decimal pesoKg)
        {
            if (pesoKg <= 0)
                throw new ProductException("El peso del producto debe ser mayor que cero.");

            if (pesoKg > PesoMaximoKg)
                throw new ProductException($"El peso del producto no puede superar los {PesoMaximoKg} Kg.");

            return decimal.Round(pesoKg, 3, MidpointRounding.AwayFromZero);
        }

        /// Valida que una dimensión física sea mayor que cero.
        private static decimal ValidarDimension(decimal valor, string nombreParametro)
        {
            if (valor <= 0)
                throw new ProductException($"La dimensión '{nombreParametro}' debe ser mayor que cero.");

            if (valor > DimensionMaximaCm)
                throw new ProductException($"La dimensión '{nombreParametro}' no puede superar los {DimensionMaximaCm} cm.");

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