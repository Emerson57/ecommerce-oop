using System;

namespace PlataformaECommerce.Dominio
{
    public sealed class ProductoDigital : Producto
    {
        #region Campos privados

        private string _formatoArchivo = string.Empty;
        private decimal _tamanoMB;

        #endregion

        #region Propiedades

        /// Formato del archivo digital (ej: "PDF", "EPUB", "MP4", "ZIP").
        public string FormatoArchivo
        {
            get => _formatoArchivo;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El formato de archivo no puede estar vacío.", nameof(FormatoArchivo));

                _formatoArchivo = value.Trim().ToUpperInvariant();
            }
        }

        /// Tamaño del archivo en MB.
        public decimal TamanoMB
        {
            get => _tamanoMB;
            private set
            {
                if (value <= 0)
                    throw new ArgumentException("El tamaño (MB) debe ser mayor que 0.", nameof(TamanoMB));

                _tamanoMB = value;
            }
        }

        #endregion

        #region Constructor

        /// Crea un producto digital con sus datos base + datos específicos del formato digital.
        public ProductoDigital(
            int id,
            string nombre,
            string descripcion,
            decimal precio,
            int stock,
            string formatoArchivo,
            decimal tamanoMB
        ) : base(id, nombre, descripcion, precio, stock)
        {
            // Validaciones específicas del producto digital
            FormatoArchivo = formatoArchivo;
            TamanoMB = tamanoMB;
        }

        #endregion

        #region Métodos (personalización de comportamiento)

        /// Sobrescribe la descripción detallada para incluir información digital relevante.
        public override string ObtenerDescripcionDetallada()
            => $"{base.ObtenerDescripcionDetallada()} | Formato: {FormatoArchivo} | Tamaño: {TamanoMB:0.##} MB";

        /// Representación corta del producto digital.
        public override string ToString()
            => $"{base.ToString()} | Digital: {FormatoArchivo} ({TamanoMB:0.##} MB)";

        #endregion
    }
}