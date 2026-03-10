using System;

namespace PlataformaECommerce.Application.DTOs.Productos
{
    public sealed class ProductResponse
    {
        /// Identificador único del producto.
        public int Id { get; set; }

        /// Nombre del producto.
        public string Nombre { get; set; } = string.Empty;

        /// Descripción del producto.
        public string Descripcion { get; set; } = string.Empty;

        /// Precio actual del producto.
        public decimal Precio { get; set; }

        /// Stock disponible del producto.
        public int Stock { get; set; }

        /// Indica si el producto está activo.
        public bool Activo { get; set; }

        /// Tipo de producto: Fisico o Digital.
        public string TipoProducto { get; set; } = string.Empty;

        /// Fecha de creación del producto.
        public DateTime FechaCreacion { get; set; }

        /// Fecha de última actualización del producto.
        public DateTime FechaActualizacion { get; set; }

        /// Formato del archivo para productos digitales.
        public string? FormatoArchivo { get; set; }

        /// Tamaño del archivo en MB para productos digitales.
        public decimal? TamanoMB { get; set; }

        /// Peso del producto en Kg para productos físicos.
        public decimal? PesoKg { get; set; }

        /// Alto del producto en centímetros para productos físicos.
        public decimal? AltoCm { get; set; }

        /// Ancho del producto en centímetros para productos físicos.
        public decimal? AnchoCm { get; set; }

        /// Largo del producto en centímetros para productos físicos.
        public decimal? LargoCm { get; set; }

        /// Volumen del producto en cm³ para productos físicos.
        public decimal? VolumenCm3 { get; set; }
    }
}