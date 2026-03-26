using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Domain.Entities.Products;

/// <summary>
/// Representa un producto físico dentro del dominio del e-commerce.
/// </summary>
/// <remarks>
/// Un producto físico corresponde a un bien tangible que requiere manejo logístico,
/// almacenamiento, control de inventario material y, en la mayoría de los casos,
/// procesos de alistamiento, embalaje y envío. Esta entidad extiende el comportamiento
/// base de <see cref="Producto"/> incorporando atributos físicos y operativos
/// necesarios para su comercialización y distribución.
/// </remarks>
public sealed class ProductoFisico : Producto
{
    #region Constantes de negocio

    /// <summary>
    /// Peso máximo permitido para productos físicos expresado en kilogramos.
    /// </summary>
    private const decimal PesoMaximoKg = 1000m;

    /// <summary>
    /// Umbral de volumen a partir del cual el producto se considera voluminoso.
    /// </summary>
    private const decimal VolumenMinimoVoluminosoCm3 = 100000m;

    /// <summary>
    /// Umbral de peso a partir del cual el producto se considera pesado para efectos logísticos.
    /// </summary>
    private const decimal PesoMinimoPesadoKg = 25m;

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor privado sin parámetros requerido por herramientas de persistencia como EF Core.
    /// </summary>
    private ProductoFisico()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="ProductoFisico"/> con la información
    /// comercial, logística y de clasificación requerida para su gestión dentro del sistema.
    /// </summary>
    /// <param name="nombre">Nombre comercial del producto físico.</param>
    /// <param name="descripcion">Descripción funcional o comercial del producto físico.</param>
    /// <param name="sku">SKU del producto representado como Value Object.</param>
    /// <param name="precio">Precio unitario actual del producto representado como Value Object.</param>
    /// <param name="stock">Cantidad disponible del producto.</param>
    /// <param name="slug">Identificador amigable para URL.</param>
    /// <param name="imagenPrincipalUrl">Ruta o URL de la imagen principal del producto.</param>
    /// <param name="categoriaId">Identificador de la categoría principal.</param>
    /// <param name="subcategoriaId">Identificador de la subcategoría.</param>
    /// <param name="etiquetas">Etiquetas comerciales o funcionales asociadas al producto.</param>
    /// <param name="pesoKg">Peso del producto expresado en kilogramos.</param>
    /// <param name="altoCm">Alto del producto expresado en centímetros.</param>
    /// <param name="anchoCm">Ancho del producto expresado en centímetros.</param>
    /// <param name="largoCm">Largo del producto expresado en centímetros.</param>
    /// <param name="requiereEnvio">Indica si el producto requiere un proceso formal de envío.</param>
    public ProductoFisico(
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
        decimal pesoKg,
        decimal altoCm,
        decimal anchoCm,
        decimal largoCm,
        bool requiereEnvio = true,
        IEnumerable<string>? galeriaImagenes = null)
        : base(nombre, descripcion, sku, precio, stock, slug, imagenPrincipalUrl, categoriaId, subcategoriaId, etiquetas, galeriaImagenes)
    {
        PesoKg = ValidarPeso(pesoKg);
        Dimensiones = new DimensionesProducto(altoCm, anchoCm, largoCm);
        RequiereEnvio = requiereEnvio;
        TipoProducto = TipoProducto.Fisico;
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Peso del producto expresado en kilogramos.
    /// </summary>
    public decimal PesoKg { get; private set; }

    /// <summary>
    /// Dimensiones físicas del producto expresadas como objeto de valor.
    /// </summary>
    public DimensionesProducto Dimensiones { get; private set; } = null!;

    /// <summary>
    /// Alto del producto expresado en centímetros.
    /// </summary>
    public decimal AltoCm => Dimensiones.AltoCm;

    /// <summary>
    /// Ancho del producto expresado en centímetros.
    /// </summary>
    public decimal AnchoCm => Dimensiones.AnchoCm;

    /// <summary>
    /// Largo del producto expresado en centímetros.
    /// </summary>
    public decimal LargoCm => Dimensiones.LargoCm;

    /// <summary>
    /// Indica si el producto requiere envío físico para su entrega al cliente.
    /// </summary>
    public bool RequiereEnvio { get; private set; }

    /// <summary>
    /// Volumen aproximado del producto expresado en centímetros cúbicos.
    /// </summary>
    /// <remarks>
    /// El volumen se calcula a partir de las dimensiones físicas almacenadas en la entidad.
    /// </remarks>
    public decimal VolumenCm3 => Dimensiones.VolumenCm3;

    #endregion

    #region Métodos de negocio

    /// <summary>
    /// Actualiza la información física y logística del producto.
    /// </summary>
    /// <param name="pesoKg">Nuevo peso del producto en kilogramos.</param>
    /// <param name="altoCm">Nuevo alto del producto en centímetros.</param>
    /// <param name="anchoCm">Nuevo ancho del producto en centímetros.</param>
    /// <param name="largoCm">Nuevo largo del producto en centímetros.</param>
    /// <param name="requiereEnvio">Nuevo indicador de requerimiento de envío.</param>
    public void ActualizarInformacionFisica(
        decimal pesoKg,
        decimal altoCm,
        decimal anchoCm,
        decimal largoCm,
        bool requiereEnvio)
    {
        PesoKg = ValidarPeso(pesoKg);
        Dimensiones = new DimensionesProducto(altoCm, anchoCm, largoCm);
        RequiereEnvio = requiereEnvio;

        MarcarActualizacion();
    }

    /// <summary>
    /// Determina si el producto puede considerarse voluminoso de acuerdo con su volumen aproximado.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si el producto supera el umbral definido para considerarse voluminoso;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool EsVoluminoso()
    {
        return Dimensiones.EsVoluminosa(VolumenMinimoVoluminosoCm3);
    }

    /// <summary>
    /// Determina si el producto puede considerarse pesado para efectos operativos o logísticos.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si el peso del producto supera el umbral definido para carga pesada;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool EsPesado()
    {
        return PesoKg >= PesoMinimoPesadoKg;
    }

    /// <summary>
    /// Determina si el producto requiere tratamiento logístico especial.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si el producto es pesado, voluminoso o requiere envío;
    /// de lo contrario, <see langword="false"/>.
    /// </returns>
    public bool RequiereManejoEspecial()
    {
        return RequiereEnvio && (EsPesado() || EsVoluminoso());
    }

    #endregion

    #region Métodos privados de validación

    /// <summary>
    /// Valida el peso del producto físico.
    /// </summary>
    /// <param name="pesoKg">Peso a validar.</param>
    /// <returns>Peso válido y normalizado.</returns>
    private static decimal ValidarPeso(decimal pesoKg)
    {
        if (pesoKg <= 0)
        {
            throw new ProductException("El peso del producto físico debe ser mayor que cero.");
        }

        if (pesoKg > PesoMaximoKg)
        {
            throw new ProductException($"El peso del producto físico no puede superar los {PesoMaximoKg} Kg.");
        }

        return decimal.Round(pesoKg, 3, MidpointRounding.AwayFromZero);
    }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida del producto físico para trazabilidad y depuración.
    /// </summary>
    /// <returns>Cadena representativa del producto físico.</returns>
    public override string ToString()
    {
        return $"{base.ToString()} | Físico: {PesoKg:0.###} Kg ({AltoCm:0.##} x {AnchoCm:0.##} x {LargoCm:0.##} cm) | Requiere envío: {RequiereEnvio}";
    }

    #endregion
}