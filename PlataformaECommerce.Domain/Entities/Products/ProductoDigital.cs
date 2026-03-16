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
    /// Longitud máxima permitida para el formato del archivo digital.
    /// </summary>
    private const int LongitudMaximaFormatoArchivo = 20;

    /// <summary>
    /// Tamaño máximo permitido para archivos digitales expresado en megabytes.
    /// </summary>
    /// <remarks>
    /// El valor corresponde a 10 GB expresados en MB.
    /// </remarks>
    private const decimal TamanoMaximoArchivoMb = 10240m;

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
    /// comercial y técnica requerida para su gestión dentro del sistema.
    /// </summary>
    /// <param name="nombre">Nombre comercial del producto digital.</param>
    /// <param name="descripcion">Descripción funcional o comercial del producto digital.</param>
    /// <param name="sku">SKU del producto representado como Value Object.</param>
    /// <param name="precio">Precio unitario actual del producto representado como Value Object.</param>
    /// <param name="stock">Cantidad disponible para operación comercial.</param>
    /// <param name="slug">Identificador amigable para URL.</param>
    /// <param name="imagenPrincipalUrl">Ruta o URL de la imagen principal del producto.</param>
    /// <param name="formatoArchivo">Formato técnico principal del archivo digital.</param>
    /// <param name="tamanoArchivoMb">Tamaño del archivo expresado en megabytes.</param>
    /// <param name="requiereLicencia">Indica si el producto requiere activación, licencia o autorización adicional para su uso.</param>
    public ProductoDigital(
        string nombre,
        string descripcion,
        Sku sku,
        Money precio,
        int stock,
        string slug,
        string? imagenPrincipalUrl,
        string formatoArchivo,
        decimal? tamanoArchivoMb,
        bool requiereLicencia)
        : base(nombre, descripcion, sku, precio, stock, slug, imagenPrincipalUrl)
    {
        FormatoArchivo = ValidarFormatoArchivo(formatoArchivo);
        TamanoArchivoMb = ValidarTamanoArchivoMb(tamanoArchivoMb);
        RequiereLicencia = requiereLicencia;
        TipoProducto = TipoProducto.Digital;
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Formato técnico principal del archivo digital.
    /// </summary>
    /// <remarks>
    /// Ejemplos comunes incluyen PDF, MP4, ZIP, EPUB, MP3, PNG o formatos equivalentes
    /// según la naturaleza del producto digital.
    /// </remarks>
    public string FormatoArchivo { get; private set; } = string.Empty;

    /// <summary>
    /// Tamaño estimado del archivo digital expresado en megabytes.
    /// </summary>
    /// <remarks>
    /// Puede ser nulo en escenarios donde el tamaño no se conoce aún o no aplica directamente
    /// al modelo comercial del producto.
    /// </remarks>
    public decimal? TamanoArchivoMb { get; private set; }

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
        FormatoArchivo = ValidarFormatoArchivo(formatoArchivo);
        TamanoArchivoMb = ValidarTamanoArchivoMb(tamanoArchivoMb);
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
        return TamanoArchivoMb.HasValue && TamanoArchivoMb.Value <= TamanoMaximoArchivoLivianoMb;
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

    /// <summary>
    /// Devuelve una descripción detallada del producto digital incluyendo
    /// información técnica relevante del archivo.
    /// </summary>
    /// <returns>Cadena descriptiva con información comercial y técnica del producto.</returns>
    public override string ObtenerDescripcionDetallada()
    {
        string tamanoTexto = TamanoArchivoMb.HasValue
            ? $"{TamanoArchivoMb.Value:0.##} MB"
            : "No especificado";

        return $"{base.ObtenerDescripcionDetallada()} | Formato: {FormatoArchivo} | Tamaño: {tamanoTexto} | Requiere licencia: {RequiereLicencia}";
    }

    #endregion

    #region Métodos privados de validación

    /// <summary>
    /// Valida el formato técnico del archivo digital.
    /// </summary>
    /// <param name="formatoArchivo">Formato a validar.</param>
    /// <returns>Formato normalizado y válido.</returns>
    private static string ValidarFormatoArchivo(string formatoArchivo)
    {
        if (string.IsNullOrWhiteSpace(formatoArchivo))
        {
            throw new ProductException("El formato del archivo digital es obligatorio.");
        }

        string formatoNormalizado = formatoArchivo.Trim().ToUpperInvariant();

        if (formatoNormalizado.Length > LongitudMaximaFormatoArchivo)
        {
            throw new ProductException($"El formato del archivo digital no puede superar los {LongitudMaximaFormatoArchivo} caracteres.");
        }

        return formatoNormalizado;
    }

    /// <summary>
    /// Valida que el tamaño del archivo digital sea consistente con las reglas del dominio.
    /// </summary>
    /// <param name="tamanoArchivoMb">Tamaño del archivo expresado en megabytes.</param>
    /// <returns>
    /// Valor normalizado con dos decimales cuando se suministra tamaño;
    /// en caso contrario, <see langword="null"/>.
    /// </returns>
    private static decimal? ValidarTamanoArchivoMb(decimal? tamanoArchivoMb)
    {
        if (!tamanoArchivoMb.HasValue)
        {
            return null;
        }

        if (tamanoArchivoMb.Value <= 0)
        {
            throw new ProductException("El tamaño del archivo digital debe ser mayor que cero.");
        }

        if (tamanoArchivoMb.Value > TamanoMaximoArchivoMb)
        {
            throw new ProductException($"El tamaño del archivo digital no puede superar los {TamanoMaximoArchivoMb} MB.");
        }

        return decimal.Round(tamanoArchivoMb.Value, 2, MidpointRounding.AwayFromZero);
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