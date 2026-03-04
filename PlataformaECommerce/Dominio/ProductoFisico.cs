using System;

namespace PlataformaECommerce.Dominio
{
    public sealed class ProductoFisico : Producto
    {
        #region Campos privados

        private decimal _pesoKg;
        private decimal _altoCm;
        private decimal _anchoCm;
        private decimal _largoCm;

        #endregion

        #region Propiedades

        /// Peso del producto en kilogramos.
        public decimal PesoKg
        {
            get => _pesoKg;
            private set
            {
                if (value <= 0)
                    throw new ArgumentException("El peso (Kg) debe ser mayor que 0.", nameof(PesoKg));

                _pesoKg = value;
            }
        }

        /// Alto del producto en centímetros.
        public decimal AltoCm
        {
            get => _altoCm;
            private set
            {
                if (value <= 0)
                    throw new ArgumentException("El alto (cm) debe ser mayor que 0.", nameof(AltoCm));

                _altoCm = value;
            }
        }

        /// Ancho del producto en centímetros.
        public decimal AnchoCm
        {
            get => _anchoCm;
            private set
            {
                if (value <= 0)
                    throw new ArgumentException("El ancho (cm) debe ser mayor que 0.", nameof(AnchoCm));

                _anchoCm = value;
            }
        }

        /// Largo del producto en centímetros.
        public decimal LargoCm
        {
            get => _largoCm;
            private set
            {
                if (value <= 0)
                    throw new ArgumentException("El largo (cm) debe ser mayor que 0.", nameof(LargoCm));

                _largoCm = value;
            }
        }

        /// Volumen aproximado en cm³ (útil para cálculos logísticos).
        public decimal VolumenCm3 => AltoCm * AnchoCm * LargoCm;

        #endregion

        #region Constructor

        /// Crea un producto físico con sus datos base + datos específicos de logística (peso y dimensiones).
        public ProductoFisico(
            int id,
            string nombre,
            string descripcion,
            decimal precio,
            int stock,
            decimal pesoKg,
            decimal altoCm,
            decimal anchoCm,
            decimal largoCm
        ) : base(id, nombre, descripcion, precio, stock)
        {
            // Validaciones específicas del producto físico
            PesoKg = pesoKg;
            AltoCm = altoCm;
            AnchoCm = anchoCm;
            LargoCm = largoCm;
        }

        #endregion

        #region Métodos (personalización de comportamiento)

        /// Sobrescribe la descripción detallada para incluir información física relevante.
        public override string ObtenerDescripcionDetallada()
            => $"{base.ObtenerDescripcionDetallada()} | Peso: {PesoKg:0.###} Kg | Dimensiones: {AltoCm:0.##}x{AnchoCm:0.##}x{LargoCm:0.##} cm";

        /// Representación corta del producto físico.
        public override string ToString()
            => $"{base.ToString()} | Físico: {PesoKg:0.###} Kg ({AltoCm:0.##}x{AnchoCm:0.##}x{LargoCm:0.##} cm)";

        #endregion
    }
}