using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Catalog.DTOs;

/// <summary>
/// Representa un producto destacado dentro de vitrinas comerciales,
/// carruseles promocionales, secciones principales o campañas del catálogo.
/// </summary>
/// <remarks>
/// Este DTO está orientado a escenarios de alta visibilidad comercial,
/// donde el producto necesita proyectarse con información optimizada para
/// experiencia de usuario, marketing y conversión.
///
/// A diferencia de un DTO de catálogo general, esta representación prioriza
/// atributos clave para vitrinas destacadas, tales como:
/// - visibilidad promocional,
/// - mensajes comerciales,
/// - etiquetas de campaña,
/// - percepción de valor,
/// - y recursos visuales de mayor impacto.
///
/// Esta clase no debe contener lógica de negocio ni comportamiento del dominio.
/// Las decisiones sobre elegibilidad, promoción, segmentación o ranking deben
/// resolverse en servicios de aplicación, dominio o motores especializados.
/// </remarks>
public sealed class FeaturedProductDto
{
    #region Identificación principal

    /// <summary>
    /// Identificador único del producto.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Código SKU del producto.
    /// </summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>
    /// Nombre comercial del producto destacado.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Slug público o identificador amigable para URL.
    /// </summary>
    public string? Slug { get; init; }

    #endregion

    #region Información de presentación comercial

    /// <summary>
    /// Título promocional o encabezado comercial del producto destacado.
    /// </summary>
    /// <remarks>
    /// Este campo puede utilizarse para resaltar un mensaje estratégico
    /// dentro de home, banners, sliders o vitrinas especiales.
    /// </remarks>
    public string? PromotionalTitle { get; init; }

    /// <summary>
    /// Mensaje breve o texto comercial asociado al producto destacado.
    /// </summary>
    public string? PromotionalText { get; init; }

    /// <summary>
    /// Etiqueta promocional principal asociada al producto.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - Oferta
    /// - Nuevo
    /// - Recomendado
    /// - Más vendido
    /// - Edición especial
    /// </remarks>
    public string? BadgeText { get; init; }

    /// <summary>
    /// Categoría principal del producto.
    /// </summary>
    public string? CategoryName { get; init; }

    /// <summary>
    /// Marca comercial del producto, cuando aplique.
    /// </summary>
    public string? Brand { get; init; }

    /// <summary>
    /// Tipo funcional del producto dentro del dominio.
    /// </summary>
    public TipoProducto ProductType { get; init; }

    #endregion

    #region Información comercial y monetaria

    /// <summary>
    /// Precio actual del producto destacado.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Código de moneda asociado al precio.
    /// </summary>
    public string Currency { get; init; } = "COP";

    /// <summary>
    /// Precio anterior o de referencia del producto, cuando exista.
    /// </summary>
    public decimal? PreviousPrice { get; init; }

    /// <summary>
    /// Valor del descuento aplicado al producto, cuando exista.
    /// </summary>
    public decimal? DiscountAmount { get; init; }

    /// <summary>
    /// Porcentaje de descuento aplicado al producto, cuando exista.
    /// </summary>
    public decimal? DiscountPercentage { get; init; }

    /// <summary>
    /// Indica si el producto se encuentra en promoción.
    /// </summary>
    public bool IsOnSale { get; init; }

    #endregion

    #region Disponibilidad y visibilidad

    /// <summary>
    /// Indica si el producto se encuentra disponible para venta.
    /// </summary>
    public bool IsAvailable { get; init; }

    /// <summary>
    /// Indica si el producto posee existencia disponible.
    /// </summary>
    public bool HasStock { get; init; }

    /// <summary>
    /// Indica si el producto está marcado como nuevo.
    /// </summary>
    public bool IsNew { get; init; }

    /// <summary>
    /// Indica si el producto está marcado como recomendado.
    /// </summary>
    public bool IsRecommended { get; init; }

    /// <summary>
    /// Indica si el producto está marcado como más vendido.
    /// </summary>
    public bool IsBestSeller { get; init; }

    /// <summary>
    /// Indica si el producto se encuentra disponible para compra inmediata.
    /// </summary>
    public bool IsReadyToBuy { get; init; }

    #endregion

    #region Recursos visuales

    /// <summary>
    /// URL de la imagen principal del producto.
    /// </summary>
    public string? MainImageUrl { get; init; }

    /// <summary>
    /// URL de una imagen promocional o hero image, cuando aplique.
    /// </summary>
    public string? HeroImageUrl { get; init; }

    /// <summary>
    /// URL opcional de un banner visual asociado al producto destacado.
    /// </summary>
    public string? BannerImageUrl { get; init; }

    /// <summary>
    /// Conjunto de imágenes de apoyo asociadas al producto destacado.
    /// </summary>
    public IReadOnlyCollection<string> ImageUrls { get; init; } = Array.Empty<string>();

    #endregion

    #region Métricas y posicionamiento

    /// <summary>
    /// Calificación promedio del producto, cuando el sistema
    /// soporte reputación o valoraciones.
    /// </summary>
    public decimal? AverageRating { get; init; }

    /// <summary>
    /// Cantidad total de valoraciones recibidas por el producto.
    /// </summary>
    public int ReviewCount { get; init; }

    /// <summary>
    /// Posición relativa del producto dentro de una vitrina o colección destacada.
    /// </summary>
    /// <remarks>
    /// Este valor puede utilizarse para ordenar visualmente los elementos
    /// en sliders, banners o componentes de home.
    /// </remarks>
    public int? DisplayOrder { get; init; }

    #endregion

    #region Navegación y acción

    /// <summary>
    /// URL pública del detalle del producto.
    /// </summary>
    public string? ProductUrl { get; init; }

    /// <summary>
    /// Texto sugerido para la llamada a la acción principal.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes:
    /// - Ver producto
    /// - Comprar ahora
    /// - Aprovechar oferta
    /// - Conocer más
    /// </remarks>
    public string? CallToActionText { get; init; }

    #endregion

    #region Fechas y vigencia

    /// <summary>
    /// Fecha y hora UTC desde la cual el producto está disponible
    /// en la vitrina destacada.
    /// </summary>
    public DateTime? FeaturedFromUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC hasta la cual el producto debe permanecer
    /// en la vitrina destacada, cuando aplique una campaña temporal.
    /// </summary>
    public DateTime? FeaturedToUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC de creación del producto.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Fecha y hora UTC de la última actualización del producto.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; init; }

    #endregion

    #region Propiedades calculadas

    /// <summary>
    /// Indica si el producto cuenta con descuento informado.
    /// </summary>
    public bool HasDiscount =>
        DiscountAmount.HasValue && DiscountAmount.Value > 0 ||
        DiscountPercentage.HasValue && DiscountPercentage.Value > 0;

    /// <summary>
    /// Indica si el producto posee una etiqueta promocional visible.
    /// </summary>
    public bool HasBadge => !string.IsNullOrWhiteSpace(BadgeText);

    /// <summary>
    /// Indica si el producto dispone de recursos visuales suficientes
    /// para una presentación destacada.
    /// </summary>
    public bool HasVisualAssets =>
        !string.IsNullOrWhiteSpace(MainImageUrl) ||
        !string.IsNullOrWhiteSpace(HeroImageUrl) ||
        !string.IsNullOrWhiteSpace(BannerImageUrl) ||
        ImageUrls.Count > 0;

    /// <summary>
    /// Indica si el producto tiene campaña destacada con vigencia temporal definida.
    /// </summary>
    public bool HasFeaturedWindow => FeaturedFromUtc.HasValue || FeaturedToUtc.HasValue;

    /// <summary>
    /// Obtiene el texto de acción más apropiado para la interfaz.
    /// </summary>
    public string EffectiveCallToActionText =>
        !string.IsNullOrWhiteSpace(CallToActionText)
            ? CallToActionText
            : IsOnSale
                ? "Aprovechar oferta"
                : "Ver producto";

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del producto destacado.
    /// </summary>
    /// <returns>Cadena representativa del DTO.</returns>
    public override string ToString()
    {
        return $"FeaturedProductDto | Id: {Id} | Sku: {Sku} | Name: {Name} | Price: {Currency} {Price:N2} | IsAvailable: {IsAvailable} | IsOnSale: {IsOnSale} | BadgeText: {BadgeText}";
    }

    #endregion
}