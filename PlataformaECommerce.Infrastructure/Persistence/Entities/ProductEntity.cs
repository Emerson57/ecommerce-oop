using System;

namespace PlataformaECommerce.Infrastructure.Persistence.Entities

{
    /// <summary>
    /// Representa la proyección persistente de un producto dentro de la infraestructura.
    /// </summary>
    /// <remarks>
    /// La entidad conserva la información necesaria para rehidratar el agregado de dominio,
    /// incluyendo clasificación comercial, trazabilidad temporal y atributos específicos
    /// según el tipo de producto.
    /// </remarks>
    public sealed class ProductEntity
    {
        /// <summary>
        /// Identificador único del producto.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre comercial del producto.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción comercial o funcional del producto.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// SKU del producto.
        /// </summary>
        public string Sku { get; set; } = string.Empty;

        /// <summary>
        /// Precio unitario del producto.
        /// </summary>
        public decimal Precio { get; set; }

        /// <summary>
        /// Precio base del producto antes de promociones.
        /// </summary>
        public decimal PrecioBase { get; set; }

        /// <summary>
        /// Precio promocional vigente cuando existe una promoción activa.
        /// </summary>
        public decimal? PrecioPromocionalActual { get; set; }

        /// <summary>
        /// Porcentaje de descuento promocional actualmente aplicado.
        /// </summary>
        public decimal? DescuentoPromocionalActual { get; set; }

        /// <summary>
        /// Moneda asociada al precio del producto.
        /// </summary>
        public string Moneda { get; set; } = string.Empty;

        /// <summary>
        /// Stock disponible del producto.
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// Indica si el producto está activo.
        /// </summary>
        public bool Activo { get; set; }

        /// <summary>
        /// Indica si el producto está destacado.
        /// </summary>
        public bool Destacado { get; set; }

        /// <summary>
        /// Tipo lógico del producto.
        /// </summary>
        public string TipoProducto { get; set; } = string.Empty;

        /// <summary>
        /// Slug comercial del producto.
        /// </summary>
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// Imagen principal asociada al producto.
        /// </summary>
        public string? ImagenPrincipalUrl { get; set; }

        /// <summary>
        /// Identificador de la categoría principal.
        /// </summary>
        public Guid? CategoriaId { get; set; }

        /// <summary>
        /// Identificador de la subcategoría.
        /// </summary>
        public Guid? SubcategoriaId { get; set; }

        /// <summary>
        /// Representación serializada de las etiquetas del producto.
        /// </summary>
        public string? EtiquetasSerializadas { get; set; }

        /// <summary>
        /// Fecha de creación del producto en UTC.
        /// </summary>
        public DateTime FechaCreacionUtc { get; set; }

        /// <summary>
        /// Fecha de última actualización del producto en UTC.
        /// </summary>
        public DateTime? FechaActualizacionUtc { get; set; }

        /// <summary>
        /// Formato del archivo del producto digital.
        /// </summary>
        public string? FormatoArchivo { get; set; }

        /// <summary>
        /// Tamaño del archivo en MB para productos digitales.
        /// </summary>
        public decimal? TamanoMB { get; set; }

        /// <summary>
        /// Indica si el producto digital requiere licencia.
        /// </summary>
        public bool? RequiereLicencia { get; set; }

        /// <summary>
        /// Peso del producto en kilogramos.
        /// </summary>
        public decimal? PesoKg { get; set; }

        /// <summary>
        /// Alto del producto en centímetros.
        /// </summary>
        public decimal? AltoCm { get; set; }

        /// <summary>
        /// Ancho del producto en centímetros.
        /// </summary>
        public decimal? AnchoCm { get; set; }

        /// <summary>
        /// Largo del producto en centímetros.
        /// </summary>
        public decimal? LargoCm { get; set; }

        /// <summary>
        /// Indica si el producto físico requiere envío.
        /// </summary>
        public bool? RequiereEnvio { get; set; }
    }
}