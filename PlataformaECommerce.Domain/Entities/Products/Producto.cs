using PlataformaECommerce.Domain.Common;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.Events;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.Rules;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Domain.Entities.Products;

/// <summary>
/// Representa la entidad base de un producto dentro del dominio del e-commerce.
/// </summary>
/// <remarks>
/// Esta clase abstrae el comportamiento común de todos los productos del sistema,
/// independientemente de si son físicos o digitales. Centraliza las reglas de negocio
/// relacionadas con identidad, información comercial, disponibilidad, inventario,
/// estado operativo, clasificación funcional y trazabilidad temporal.
/// 
/// La entidad utiliza Value Objects para representar conceptos críticos del dominio,
/// como el SKU, el valor monetario del precio y las etiquetas del producto,
/// reduciendo el acoplamiento con tipos primitivos y fortaleciendo la consistencia
/// del modelo.
/// 
/// La clasificación del producto se resuelve mediante:
/// - <see cref="CategoriaId"/> para la categoría principal
/// - <see cref="SubcategoriaId"/> para una categoría hija opcional
/// - <see cref="Etiquetas"/> para clasificación transversal y filtrado comercial
/// 
/// La jerarquía real de categorías se modela en la entidad <c>CategoriaProducto</c>.
/// Esta entidad solo conserva los identificadores necesarios para mantener
/// desacoplamiento entre agregados.
/// 
/// Además, se apoya en reglas de negocio reutilizables para expresar decisiones
/// de disponibilidad comercial y suficiencia de stock de manera más limpia,
/// mantenible y alineada con Domain-Driven Design.
/// 
/// Finalmente, la entidad registra eventos de dominio cuando ocurren hechos
/// relevantes del negocio, como la transición del inventario a cero unidades.
/// </remarks>
public abstract class Producto : AggregateRoot
{
    #region Constantes de negocio

    /// <summary>
    /// Longitud mínima permitida para el nombre del producto.
    /// </summary>
    private const int LongitudMinimaNombre = 3;

    /// <summary>
    /// Longitud máxima permitida para el nombre del producto.
    /// </summary>
    private const int LongitudMaximaNombre = 150;

    /// <summary>
    /// Longitud máxima permitida para la descripción del producto.
    /// </summary>
    private const int LongitudMaximaDescripcion = 2000;

    /// <summary>
    /// Longitud máxima permitida para el slug del producto.
    /// </summary>
    private const int LongitudMaximaSlug = 160;

    #endregion

    #region Campos privados

    /// <summary>
    /// Colección interna de etiquetas de negocio asociadas al producto.
    /// </summary>
    private readonly List<EtiquetaProducto> _etiquetas = new();

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor protegido sin parámetros requerido por herramientas de persistencia como EF Core.
    /// </summary>
    protected Producto()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la entidad <see cref="Producto"/> con la información base requerida.
    /// </summary>
    /// <param name="nombre">Nombre comercial del producto.</param>
    /// <param name="descripcion">Descripción funcional o comercial del producto.</param>
    /// <param name="sku">SKU del producto representado como Value Object.</param>
    /// <param name="precio">Precio unitario actual del producto representado como Value Object.</param>
    /// <param name="stock">Cantidad disponible del producto.</param>
    /// <param name="slug">Identificador amigable para URL.</param>
    /// <param name="imagenPrincipalUrl">Ruta o URL de la imagen principal del producto.</param>
    /// <param name="categoriaId">Identificador de la categoría principal.</param>
    /// <param name="subcategoriaId">Identificador de la subcategoría.</param>
    /// <param name="etiquetas">Colección de etiquetas de clasificación comercial.</param>
    protected Producto(
        string nombre,
        string descripcion,
        Sku sku,
        Money precio,
        int stock,
        string slug,
        string? imagenPrincipalUrl,
        Guid? categoriaId,
        Guid? subcategoriaId,
        IEnumerable<EtiquetaProducto>? etiquetas)
    {
        InicializarAggregateRoot();
        AplicarInformacionBasica(nombre, descripcion, sku, precio, slug, imagenPrincipalUrl);
        Stock = ValidarStock(stock);

        Activo = false;
        Destacado = false;
        AplicarClasificacion(categoriaId, subcategoriaId, etiquetas);
    }

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Nombre comercial del producto.
    /// </summary>
    public string Nombre { get; private set; } = string.Empty;

    /// <summary>
    /// Descripción general del producto.
    /// </summary>
    public string Descripcion { get; private set; } = string.Empty;

    /// <summary>
    /// Código SKU del producto utilizado para control interno y trazabilidad comercial.
    /// </summary>
    public Sku Sku { get; private set; } = null!;

    /// <summary>
    /// Precio unitario actual del producto.
    /// </summary>
    public Money Precio { get; private set; } = null!;

    /// <summary>
    /// Stock disponible del producto.
    /// </summary>
    public int Stock { get; private set; }

    /// <summary>
    /// Indica si el producto se encuentra habilitado para ser ofrecido en el sistema.
    /// </summary>
    public bool Activo { get; private set; }

    /// <summary>
    /// Indica si el producto ha sido marcado como destacado dentro del catálogo.
    /// </summary>
    public bool Destacado { get; private set; }

    /// <summary>
    /// Identificador amigable para URL y navegación pública.
    /// </summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// URL o ruta de la imagen principal asociada al producto.
    /// </summary>
    public string? ImagenPrincipalUrl { get; private set; }

    /// <summary>
    /// Tipo de producto representado por la entidad.
    /// </summary>
    public TipoProducto TipoProducto { get; protected set; }

    /// <summary>
    /// Identificador de la categoría principal del producto.
    /// </summary>
    public Guid? CategoriaId { get; private set; }

    /// <summary>
    /// Identificador de la subcategoría del producto.
    /// </summary>
    public Guid? SubcategoriaId { get; private set; }

    /// <summary>
    /// Colección de etiquetas comerciales o funcionales asociadas al producto.
    /// </summary>
    public IReadOnlyCollection<EtiquetaProducto> Etiquetas => _etiquetas.AsReadOnly();

    #endregion

    #region Métodos de negocio

    /// <summary>
    /// Actualiza la información comercial básica del producto.
    /// </summary>
    /// <param name="nombre">Nuevo nombre comercial del producto.</param>
    /// <param name="descripcion">Nueva descripción del producto.</param>
    /// <param name="sku">Nuevo SKU del producto.</param>
    /// <param name="precio">Nuevo precio del producto.</param>
    /// <param name="slug">Nuevo slug del producto.</param>
    /// <param name="imagenPrincipalUrl">Nueva imagen principal del producto.</param>
    public void ActualizarInformacionBasica(
        string nombre,
        string descripcion,
        Sku sku,
        Money precio,
        string slug,
        string? imagenPrincipalUrl)
    {
        AplicarInformacionBasica(nombre, descripcion, sku, precio, slug, imagenPrincipalUrl);

        MarcarActualizacion();
    }

    /// <summary>
    /// Actualiza la clasificación del producto.
    /// </summary>
    /// <param name="categoriaId">Nuevo identificador de categoría principal.</param>
    /// <param name="subcategoriaId">Nuevo identificador de subcategoría.</param>
    /// <param name="etiquetas">Nueva colección de etiquetas.</param>
    /// <remarks>
    /// Esta operación centraliza la consistencia de la clasificación comercial del producto.
    /// La validación de existencia de categorías y de relación padre-hijo pertenece a la capa
    /// de aplicación, mientras que aquí se protegen las invariantes propias de la entidad.
    /// </remarks>
    public void ActualizarClasificacion(
        Guid? categoriaId,
        Guid? subcategoriaId,
        IEnumerable<EtiquetaProducto>? etiquetas)
    {
        AplicarClasificacion(categoriaId, subcategoriaId, etiquetas);
        MarcarActualizacion();
    }

    /// <summary>
    /// Asigna o actualiza únicamente la categoría principal y subcategoría del producto,
    /// preservando las etiquetas actuales.
    /// </summary>
    /// <param name="categoriaId">Identificador de la categoría principal.</param>
    /// <param name="subcategoriaId">Identificador de la subcategoría.</param>
    public void AsignarCategoria(Guid? categoriaId, Guid? subcategoriaId)
    {
        AplicarClasificacion(categoriaId, subcategoriaId, _etiquetas);
        MarcarActualizacion();
    }

    /// <summary>
    /// Elimina la clasificación de categoría y subcategoría del producto,
    /// preservando las etiquetas actuales.
    /// </summary>
    public void QuitarClasificacion()
    {
        AplicarClasificacion(null, null, _etiquetas);
        MarcarActualizacion();
    }

    /// <summary>
    /// Reemplaza completamente la colección de etiquetas del producto,
    /// preservando la clasificación actual por categoría.
    /// </summary>
    /// <param name="etiquetas">Nueva colección de etiquetas.</param>
    public void ReemplazarEtiquetas(IEnumerable<EtiquetaProducto>? etiquetas)
    {
        AplicarClasificacion(CategoriaId, SubcategoriaId, etiquetas);
        MarcarActualizacion();
    }

    /// <summary>
    /// Indica si el producto contiene una etiqueta determinada.
    /// </summary>
    /// <param name="etiqueta">Etiqueta a evaluar.</param>
    /// <returns>
    /// <see langword="true"/> si la etiqueta existe dentro del producto;
    /// de lo contrario, <see langword="false"/>.
    /// </returns>
    public bool TieneEtiqueta(EtiquetaProducto etiqueta)
    {
        ArgumentNullException.ThrowIfNull(etiqueta);

        return _etiquetas.Contains(etiqueta);
    }

    /// <summary>
    /// Actualiza únicamente el precio del producto.
    /// </summary>
    /// <param name="nuevoPrecio">Nuevo valor unitario del producto.</param>
    public void ActualizarPrecio(Money nuevoPrecio)
    {
        ActualizarPrecioInterno(nuevoPrecio);
        MarcarActualizacion();
    }

    /// <summary>
    /// Actualiza el SKU del producto.
    /// </summary>
    /// <param name="nuevoSku">Nuevo SKU a asignar al producto.</param>
    public void ActualizarSku(Sku nuevoSku)
    {
        Sku = ValidarSku(nuevoSku);
        MarcarActualizacion();
    }

    /// <summary>
    /// Actualiza la imagen principal del producto.
    /// </summary>
    /// <param name="imagenPrincipalUrl">Nueva URL o ruta de imagen.</param>
    public void ActualizarImagenPrincipal(string? imagenPrincipalUrl)
    {
        ImagenPrincipalUrl = ValidarImagenPrincipalUrl(imagenPrincipalUrl);
        MarcarActualizacion();
    }

    /// <summary>
    /// Establece un valor absoluto para el stock disponible del producto.
    /// </summary>
    /// <param name="nuevoStock">Nuevo stock del producto.</param>
    public void ActualizarStock(int nuevoStock)
    {
        Stock = ValidarStock(nuevoStock);
        MarcarActualizacion();
    }

    /// <summary>
    /// Incrementa el stock disponible del producto.
    /// </summary>
    /// <param name="cantidad">Cantidad a adicionar al inventario actual.</param>
    public void IncrementarStock(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ProductException("La cantidad a incrementar en inventario debe ser mayor que cero.");
        }

        checked
        {
            Stock += cantidad;
        }

        MarcarActualizacion();
    }

    /// <summary>
    /// Reduce el stock del producto validando disponibilidad suficiente.
    /// </summary>
    /// <param name="cantidad">Cantidad a descontar del inventario actual.</param>
    public void DisminuirStock(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ProductException("La cantidad a disminuir del inventario debe ser mayor que cero.");
        }

        if (!StockDisponibleRule.IsSatisfiedBy(Stock, cantidad))
        {
            throw new InventarioInsuficienteException(Sku.Value, Stock, cantidad);
        }

        Stock -= cantidad;
        MarcarActualizacion();

        if (Stock == 0)
        {
            AddDomainEvent(new ProductoSinStockEvent(this));
        }
    }

    /// <summary>
    /// Indica si el producto posee stock mayor a cero.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si existe al menos una unidad disponible;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool TieneStock()
    {
        return Stock > 0;
    }

    /// <summary>
    /// Determina si el producto tiene stock suficiente para una cantidad solicitada.
    /// </summary>
    /// <param name="cantidad">Cantidad requerida.</param>
    /// <returns>
    /// <see langword="true"/> si el stock disponible cubre la cantidad solicitada;
    /// de lo contrario, <see langword="false"/>.
    /// </returns>
    public bool TieneStockDisponible(int cantidad)
    {
        return StockDisponibleRule.IsSatisfiedBy(Stock, cantidad);
    }

    /// <summary>
    /// Indica si el producto está disponible operativamente para el catálogo.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si el producto está activo y tiene stock;
    /// de lo contrario, <see langword="false"/>.
    /// </returns>
    public bool EstaDisponible()
    {
        return ProductoDisponibleRule.IsSatisfiedBy(this);
    }

    /// <summary>
    /// Valida que el producto se encuentre disponible para una operación comercial.
    /// </summary>
    /// <remarks>
    /// Este método centraliza la validación de disponibilidad para escenarios como
    /// agregado al carrito, reserva de stock o confirmación de compra.
    /// </remarks>
    public void ValidarDisponibilidad()
    {
        if (!ProductoDisponibleRule.IsSatisfiedBy(this))
        {
            throw new ProductoNoDisponibleException(Id, Nombre);
        }
    }

    /// <summary>
    /// Activa el producto para que pueda ser publicado o comercializado.
    /// </summary>
    public void Activar()
    {
        if (Activo)
        {
            return;
        }

        Activo = true;
        MarcarActualizacion();
    }

    /// <summary>
    /// Desactiva el producto para impedir su operación comercial dentro del sistema.
    /// </summary>
    public void Desactivar()
    {
        if (!Activo)
        {
            return;
        }

        Activo = false;
        MarcarActualizacion();
    }

    /// <summary>
    /// Marca el producto como destacado dentro del catálogo.
    /// </summary>
    public void MarcarComoDestacado()
    {
        if (Destacado)
        {
            return;
        }

        Destacado = true;
        MarcarActualizacion();
    }

    /// <summary>
    /// Retira la marca de destacado del producto.
    /// </summary>
    public void QuitarDestacado()
    {
        if (!Destacado)
        {
            return;
        }

        Destacado = false;
        MarcarActualizacion();
    }

    #endregion

    #region Métodos privados de negocio y validación

    /// <summary>
    /// Aplica de forma consistente la clasificación del producto.
    /// </summary>
    /// <param name="categoriaId">Identificador de la categoría principal.</param>
    /// <param name="subcategoriaId">Identificador de la subcategoría.</param>
    /// <param name="etiquetas">Colección de etiquetas.</param>
    private void AplicarClasificacion(
        Guid? categoriaId,
        Guid? subcategoriaId,
        IEnumerable<EtiquetaProducto>? etiquetas)
    {
        ValidarClasificacion(categoriaId, subcategoriaId);

        CategoriaId = categoriaId;
        SubcategoriaId = subcategoriaId;

        _etiquetas.Clear();
        _etiquetas.AddRange(ValidarEtiquetas(etiquetas));
    }

    /// <summary>
    /// Aplica de forma consistente la información comercial base del producto.
    /// </summary>
    /// <param name="nombre">Nombre comercial.</param>
    /// <param name="descripcion">Descripción comercial o funcional.</param>
    /// <param name="sku">SKU del producto.</param>
    /// <param name="precio">Precio vigente del producto.</param>
    /// <param name="slug">Slug del producto.</param>
    /// <param name="imagenPrincipalUrl">Imagen principal asociada.</param>
    private void AplicarInformacionBasica(
        string nombre,
        string descripcion,
        Sku sku,
        Money precio,
        string slug,
        string? imagenPrincipalUrl)
    {
        Nombre = ValidarNombre(nombre);
        Descripcion = ValidarDescripcion(descripcion);
        Sku = ValidarSku(sku);
        ActualizarPrecioInterno(precio);
        Slug = ValidarSlug(slug);
        ImagenPrincipalUrl = ValidarImagenPrincipalUrl(imagenPrincipalUrl);
    }

    /// <summary>
    /// Actualiza el precio del producto preservando la consistencia monetaria del agregado.
    /// </summary>
    /// <param name="precio">Precio a establecer.</param>
    private void ActualizarPrecioInterno(Money precio)
    {
        Money precioValidado = ValidarPrecio(precio);

        if (Precio is not null && !MonedaConsistenteRule.IsSatisfiedBy(Precio.Currency, precioValidado))
        {
            throw new ProductException(
                $"No es posible cambiar la moneda del producto una vez establecida. Moneda esperada: '{Precio.Currency}', moneda recibida: '{precioValidado.Currency}'.");
        }

        Precio = precioValidado;
    }

    /// <summary>
    /// Valida el nombre del producto conforme a las reglas del dominio.
    /// </summary>
    /// <param name="nombre">Nombre a validar.</param>
    /// <returns>Nombre normalizado y válido.</returns>
    private static string ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ProductException("El nombre del producto es obligatorio.");
        }

        string nombreNormalizado = nombre.Trim();

        if (nombreNormalizado.Length < LongitudMinimaNombre)
        {
            throw new ProductException($"El nombre del producto debe tener al menos {LongitudMinimaNombre} caracteres.");
        }

        if (nombreNormalizado.Length > LongitudMaximaNombre)
        {
            throw new ProductException($"El nombre del producto no puede superar los {LongitudMaximaNombre} caracteres.");
        }

        return nombreNormalizado;
    }

    /// <summary>
    /// Valida la descripción del producto.
    /// </summary>
    /// <param name="descripcion">Descripción a validar.</param>
    /// <returns>Descripción normalizada y válida.</returns>
    private static string ValidarDescripcion(string descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            throw new ProductException("La descripción del producto es obligatoria.");
        }

        string descripcionNormalizada = descripcion.Trim();

        if (descripcionNormalizada.Length > LongitudMaximaDescripcion)
        {
            throw new ProductException($"La descripción del producto no puede superar los {LongitudMaximaDescripcion} caracteres.");
        }

        return descripcionNormalizada;
    }

    /// <summary>
    /// Valida el SKU del producto.
    /// </summary>
    /// <param name="sku">SKU a validar.</param>
    /// <returns>SKU válido.</returns>
    private static Sku ValidarSku(Sku sku)
    {
        if (sku is null)
        {
            throw new ProductException("El SKU del producto es obligatorio.");
        }

        return sku;
    }

    /// <summary>
    /// Valida el precio del producto.
    /// </summary>
    /// <param name="precio">Precio a validar.</param>
    /// <returns>Precio válido.</returns>
    private static Money ValidarPrecio(Money precio)
    {
        if (precio is null)
        {
            throw new ProductException("El precio del producto es obligatorio.");
        }

        return precio;
    }

    /// <summary>
    /// Valida que el stock no sea negativo.
    /// </summary>
    /// <param name="stock">Stock a validar.</param>
    /// <returns>Stock válido.</returns>
    private static int ValidarStock(int stock)
    {
        if (stock < 0)
        {
            throw new ProductException("El stock del producto no puede ser negativo.");
        }

        return stock;
    }

    /// <summary>
    /// Valida el slug del producto para su uso en URLs.
    /// </summary>
    /// <param name="slug">Slug a validar.</param>
    /// <returns>Slug normalizado y válido.</returns>
    private static string ValidarSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ProductException("El slug del producto es obligatorio.");
        }

        string slugNormalizado = slug.Trim().ToLowerInvariant();

        if (slugNormalizado.Length > LongitudMaximaSlug)
        {
            throw new ProductException($"El slug del producto no puede superar los {LongitudMaximaSlug} caracteres.");
        }

        if (slugNormalizado.Contains(' '))
        {
            throw new ProductException("El slug del producto no puede contener espacios.");
        }

        return slugNormalizado;
    }

    /// <summary>
    /// Valida y normaliza la URL o ruta de la imagen principal del producto.
    /// </summary>
    /// <param name="imagenPrincipalUrl">Valor de imagen a validar.</param>
    /// <returns>Ruta normalizada o <see langword="null"/> cuando no se suministra valor.</returns>
    private static string? ValidarImagenPrincipalUrl(string? imagenPrincipalUrl)
    {
        if (string.IsNullOrWhiteSpace(imagenPrincipalUrl))
        {
            return null;
        }

        return imagenPrincipalUrl.Trim();
    }

    /// <summary>
    /// Valida la consistencia básica entre categoría y subcategoría.
    /// </summary>
    /// <param name="categoriaId">Identificador de la categoría principal.</param>
    /// <param name="subcategoriaId">Identificador de la subcategoría.</param>
    private static void ValidarClasificacion(Guid? categoriaId, Guid? subcategoriaId)
    {
        if (categoriaId == Guid.Empty)
        {
            throw new ProductException("El identificador de categoría no puede ser vacío.");
        }

        if (subcategoriaId == Guid.Empty)
        {
            throw new ProductException("El identificador de subcategoría no puede ser vacío.");
        }

        if (!categoriaId.HasValue && subcategoriaId.HasValue)
        {
            throw new ProductException("No es válido asignar una subcategoría sin una categoría principal.");
        }

        if (categoriaId.HasValue && subcategoriaId.HasValue && categoriaId.Value == subcategoriaId.Value)
        {
            throw new ProductException("La categoría y la subcategoría del producto no pueden ser iguales.");
        }
    }

    /// <summary>
    /// Valida la colección de etiquetas del producto.
    /// </summary>
    /// <param name="etiquetas">Etiquetas a evaluar.</param>
    /// <returns>Colección validada y sin duplicados.</returns>
    private static List<EtiquetaProducto> ValidarEtiquetas(IEnumerable<EtiquetaProducto>? etiquetas)
    {
        if (etiquetas is null)
        {
            return new List<EtiquetaProducto>();
        }

        List<EtiquetaProducto> etiquetasNormalizadas = etiquetas
            .Where(etiqueta => etiqueta is not null)
            .Distinct()
            .ToList();

        if (etiquetasNormalizadas.Count > DomainLimits.MaximoEtiquetasPorProducto)
        {
            throw new ProductException($"El producto no puede tener más de {DomainLimits.MaximoEtiquetasPorProducto} etiquetas.");
        }

        return etiquetasNormalizadas;
    }

    #endregion

    #region Representación textual

    /// <summary>
    /// Devuelve una representación resumida y útil del producto para trazabilidad y depuración.
    /// </summary>
    /// <returns>Cadena representativa del estado principal del producto.</returns>
    public override string ToString()
    {
        string etiquetasTexto = _etiquetas.Count > 0
            ? string.Join(", ", _etiquetas.Select(x => x.Value))
            : "Sin etiquetas";

        return $"Producto: {Id} | Nombre: {Nombre} | SKU: {Sku} | Precio: {Precio} | Stock: {Stock} | Activo: {Activo} | Destacado: {Destacado} | Tipo: {TipoProducto} | Categoría: {CategoriaId} | Subcategoría: {SubcategoriaId} | Etiquetas: {etiquetasTexto}";
    }

    #endregion
}