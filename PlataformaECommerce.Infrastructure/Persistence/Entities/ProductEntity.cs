using System;

namespace PlataformaECommerce.Infrastructure.Persistence.Entities
{
    public sealed class ProductEntity
    {
        #region Propiedades comunes

        /// Identificador único del producto en la base de datos.
        public int Id { get; set; }

        /// Nombre del producto.
        public string Nombre { get; set; } = string.Empty;

        /// Descripción del producto.
        public string Descripcion { get; set; } = string.Empty;

        /// Precio unitario del producto.
        public decimal Precio { get; set; }

        /// Stock disponible del producto.
        public int Stock { get; set; }

        /// Indica si el producto está activo.
        public bool Activo { get; set; }

        /// Tipo lógico del producto.
        public string TipoProducto { get; set; } = string.Empty;

        /// Fecha de creación del producto.
        public DateTime FechaCreacion { get; set; }

        /// Fecha de última actualización del producto.
        public DateTime FechaActualizacion { get; set; }

        #endregion

        #region Propiedades específicas para productos digitales

        /// Formato del archivo del producto digital.
        public string? FormatoArchivo { get; set; }

        /// Tamaño del archivo en MB para productos digitales.
        public decimal? TamanoMB { get; set; }

        #endregion

        #region Propiedades específicas para productos físicos

        /// Peso del producto en kilogramos.
        public decimal? PesoKg { get; set; }

        /// Alto del producto en centímetros.
        public decimal? AltoCm { get; set; }

        /// Ancho del producto en centímetros.
        public decimal? AnchoCm { get; set; }

        /// Largo del producto en centímetros.
        public decimal? LargoCm { get; set; }

        #endregion
    }
}