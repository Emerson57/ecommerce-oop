using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Domain.Entities.Products;

/// <summary>
/// Representa un producto digital dentro del dominio del e-commerce.
/// </summary>
/// <remarks>
/// Un producto digital corresponde a un bien intangible cuya entrega se realiza por medios
/// electrónicos, como descargas, licencias, acceso a contenido o distribución de archivos.
/// Esta entidad extiende el comportamiento base de <see cref="Producto"/> incorporando
/// información técnica propia del canal digital de entrega.
/// </remarks>
public sealed class ProductoDigital : Producto
{
    #region Constantes de negocio

    /// <summary>
    /// Umbral máximo para considerar un archivo como liviano.
    /// </summary>
    private const decimal TamanoMaximoArchivoLivianoMb = 100m;

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor privado sin parámetros requerido por herramientas de persistencia como EF Core.
    /// </summary>
    private ProductoDigital()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="ProductoDigital"/> con la información
    /// comercial, técnica y de clasificación requerida para su gestión dentro del sistema.
    /// </summary>
    /// <param name="nombre">Nombre comercial del producto digital.</param>
    /// <param name="descripcion">Descripción funcional o comercial del producto digital.</param>
    /// <param name="sku">SKU del producto representado como Value Object.</param>
    /// <param name="precio">Precio unitario actual del producto representado como Value Object.</param>
    /// <param name="stock">Cantidad disponible para operación comercial.</param>
    /// <param name="slug">Identificador amigable para URL.</param>
    /// <param name="imagenPrincipalUrl">Ruta o URL de la imagen principal del producto.</param>
    /// <param name="categoriaId">Identificador de la categoría principal.</param>
    /// <param name="subcategoriaId">Identificador de la subcategoría.</param>
    /// <param name="etiquetas">Etiquetas comerciales o funcionales asociadas al producto.</param>
    /// <param name="formatoArchivo">Formato técnico principal del archivo digital.</param>
    /// <param name="tamanoArchivoMb">Tamaño del archivo expresado en megabytes.</param>
    /// <param name="requiereLicencia">Indica si el producto requiere activación, licencia o autorización adicional para su uso.</param>
    /// <param name="galeriaImagenes">Colección opcional de URLs o rutas de imágenes adicionales para la galería del producto.</param>
    public ProductoDigital(
        string nombre,
        string descripcion,
        Sku sku,
        Money precio,
        int stock,
        string slug,
        string? imagenPrincipalUrl,
        Guid? categoriaId,
        Guid? subcategoriaId,
        IEnumerable<EtiquetaProducto>? etiquetas,
        string formatoArchivo,
        decimal? tamanoArchivoMb,
        bool requiereLicencia,
        IEnumerable<string>? galeriaImagenes = null)
        : base(nombre, descripcion, sku, precio, stock, slug, imagenPrincipalUrl, categoriaId, subcategoriaId, etiquetas, galeriaImagenes)
    {
        Archivo = new ArchivoDigital(formatoArchivo, tamanoArchivoMb);
        RequiereLicencia = requiereLicencia;
        TipoProducto = TipoProducto.Digital;
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Información técnica principal del archivo digital como objeto de valor.
    /// </summary>
    public ArchivoDigital Archivo { get; private set; } = null!;

    /// <summary>
    /// Formato técnico principal del archivo digital.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes incluyen PDF, MP4, ZIP, EPUB, MP3, PNG o formatos equivalentes
    /// según la naturaleza del producto digital.
    /// </remarks>
    public string FormatoArchivo => Archivo.Formato;

    /// <summary>
    /// Tamaño estimado del archivo digital expresado en megabytes.
    /// </summary>
    /// <remarks>
    /// Puede ser nulo en escenarios donde el tamaño no se conoce aún o no aplica directamente
    /// al modelo comercial del producto.
    /// </remarks>
    public decimal? TamanoArchivoMb => Archivo.TamanoMb;

    /// <summary>
    /// Indica si el producto requiere una licencia, activación o mecanismo adicional
    /// para habilitar su consumo por parte del cliente.
    /// </summary>
    public bool RequiereLicencia { get; private set; }

    #endregion

    #region Métodos de negocio

    /// <summary>
    /// Actualiza la información técnica asociada al producto digital.
    /// </summary>
    /// <param name="formatoArchivo">Nuevo formato del archivo digital.</param>
    /// <param name="tamanoArchivoMb">Nuevo tamaño estimado del archivo en megabytes.</param>
    /// <param name="requiereLicencia">Nuevo indicador de requerimiento de licencia.</param>
    public void ActualizarInformacionDigital(
        string formatoArchivo,
        decimal? tamanoArchivoMb,
        bool requiereLicencia)
    {
        Archivo = new ArchivoDigital(formatoArchivo, tamanoArchivoMb);
        RequiereLicencia = requiereLicencia;

        MarcarActualizacion();
    }

    /// <summary>
    /// Indica si el archivo digital puede considerarse liviano según su tamaño.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si el tamaño del archivo es conocido y menor o igual al umbral definido;
    /// de lo contrario, <see langword="false"/>.
    /// </returns>
    public bool EsArchivoLiviano()
    {
        return Archivo.EsLiviano(TamanoMaximoArchivoLivianoMb);
    }

    /// <summary>
    /// Indica si el producto digital requiere algún tipo de activación posterior a la compra.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si el producto requiere licencia o habilitación adicional;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool RequiereActivacionPosterior()
    {
        return RequiereLicencia;
    }

    /// <summary>
    /// Determina si el producto digital está listo para distribución electrónica inmediata.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si no requiere licencia adicional;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool EstaListoParaEntregaInmediata()
    {
        return !RequiereLicencia;
    }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del producto digital para trazabilidad y depuración.
    /// </summary>
    /// <returns>Cadena representativa del producto digital.</returns>
    public override string ToString()
    {
        string tamanoTexto = TamanoArchivoMb.HasValue
            ? $"{TamanoArchivoMb.Value:0.##} MB"
            : "Sin tamaño definido";

        return $"{base.ToString()} | Digital: {FormatoArchivo} ({tamanoTexto}) | Requiere licencia: {RequiereLicencia}";
    }

    #endregion
}