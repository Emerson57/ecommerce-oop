using System.ComponentModel.DataAnnotations;

namespace PlataformaECommerce.Application.Features.Products.DTOs
{
    public sealed class UpdateProductRequest
    {
        /// Nombre actualizado del producto.
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        /// Descripción actualizada del producto.
        [Required(ErrorMessage = "La descripción del producto es obligatoria.")]
        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        /// Precio actualizado.
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal Precio { get; set; }

        /// Stock actualizado.
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        /// Tipo de producto.
        [Required(ErrorMessage = "El tipo de producto es obligatorio.")]
        [StringLength(20, ErrorMessage = "El tipo de producto no puede superar los 20 caracteres.")]
        public string TipoProducto { get; set; } = string.Empty;

        /// Formato del archivo para productos digitales.
        [StringLength(20, ErrorMessage = "El formato del archivo no puede superar los 20 caracteres.")]
        public string? FormatoArchivo { get; set; }

        /// Tamaño del archivo en MB para productos digitales.
        public decimal? TamanoMB { get; set; }

        /// Peso del producto en Kg para productos físicos.
        public decimal? PesoKg { get; set; }

        /// Alto del producto en cm para productos físicos.
        public decimal? AltoCm { get; set; }

        /// Ancho del producto en cm para productos físicos.
        public decimal? AnchoCm { get; set; }

        /// Largo del producto en cm para productos físicos.
        public decimal? LargoCm { get; set; }
    }
}