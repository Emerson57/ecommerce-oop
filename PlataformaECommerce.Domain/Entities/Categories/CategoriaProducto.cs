using PlataformaECommerce.Domain.Common;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Domain.Entities.Categories;

/// <summary>
/// Representa una categoría de producto dentro del dominio del e-commerce.
/// </summary>
/// <remarks>
/// Esta entidad modela la clasificación jerárquica de productos.
/// 
/// El diseño permite representar tanto:
/// - categorías raíz, como por ejemplo: "Tecnología"
/// - subcategorías, como por ejemplo: "Laptops"
/// 
/// La jerarquía se resuelve mediante la propiedad <see cref="ParentCategoryId"/>:
/// - Si es null, la categoría es raíz.
/// - Si tiene valor, la categoría depende de una categoría padre.
/// 
/// Esta entidad se utilizará en:
/// - clasificación de productos
/// - navegación del catálogo
/// - filtros de búsqueda
/// - administración del e-commerce
/// - futuras URLs amigables y SEO
/// </remarks>
public class CategoriaProducto : AggregateRoot
{
    #region Constantes de negocio

    /// <summary>
    /// Longitud máxima permitida para el nombre de la categoría.
    /// </summary>
    private const int NombreMaxLength = 120;

    /// <summary>
    /// Longitud máxima permitida para la descripción de la categoría.
    /// </summary>
    private const int DescripcionMaxLength = 500;

    /// <summary>
    /// Longitud máxima permitida para el slug.
    /// </summary>
    private const int SlugMaxLength = 140;

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor protegido requerido por herramientas de persistencia como EF Core.
    /// </summary>
    protected CategoriaProducto()
    {
        Nombre = string.Empty;
        Slug = string.Empty;
        Descripcion = null;
    }

    /// <summary>
    /// Inicializa una nueva categoría raíz o subcategoría.
    /// </summary>
    /// <param name="nombre">Nombre visible de la categoría.</param>
    /// <param name="slug">Identificador amigable para URL y navegación.</param>
    /// <param name="descripcion">Descripción opcional de la categoría.</param>
    /// <param name="parentCategoryId">Identificador de la categoría padre. Null si es categoría raíz.</param>
    public CategoriaProducto(
        string nombre,
        string slug,
        string? descripcion = null,
        Guid? parentCategoryId = null)
    {
        InicializarAggregateRoot();
        Nombre = ValidarNombre(nombre);
        Slug = ValidarSlug(slug);
        Descripcion = ValidarDescripcion(descripcion);
        ParentCategoryId = ValidarPadre(parentCategoryId, Id);
        Activa = false;
    }

    #endregion

    #region Propiedades

    /// <summary>
    /// Nombre visible de la categoría.
    /// </summary>
    public string Nombre { get; private set; }

    /// <summary>
    /// Slug normalizado para URL y filtrado amigable.
    /// </summary>
    public string Slug { get; private set; }

    /// <summary>
    /// Descripción funcional o comercial de la categoría.
    /// </summary>
    public string? Descripcion { get; private set; }

    /// <summary>
    /// Indica si la categoría se encuentra habilitada para uso en el catálogo y administración.
    /// </summary>
    public bool Activa { get; private set; }

    /// <summary>
    /// Identificador de la categoría padre.
    /// Si es null, esta categoría se considera una categoría raíz.
    /// </summary>
    public Guid? ParentCategoryId { get; private set; }

    #endregion

    #region Propiedades derivadas

    /// <summary>
    /// Indica si la categoría es raíz.
    /// </summary>
    public bool EsCategoriaRaiz => ParentCategoryId is null;

    /// <summary>
    /// Indica si la categoría depende de una categoría padre.
    /// </summary>
    public bool EsSubcategoria => ParentCategoryId.HasValue;

    #endregion

    #region Métodos de negocio

    /// <summary>
    /// Actualiza la información básica de la categoría.
    /// </summary>
    /// <param name="nombre">Nuevo nombre de la categoría.</param>
    /// <param name="slug">Nuevo slug de la categoría.</param>
    /// <param name="descripcion">Nueva descripción opcional.</param>
    public void ActualizarInformacionBasica(
        string nombre,
        string slug,
        string? descripcion)
    {
        Nombre = ValidarNombre(nombre);
        Slug = ValidarSlug(slug);
        Descripcion = ValidarDescripcion(descripcion);
        MarcarActualizacion();
    }

    /// <summary>
    /// Reasigna la categoría padre.
    /// </summary>
    /// <param name="parentCategoryId">
    /// Identificador de la categoría padre.
    /// Null si se desea convertir en categoría raíz.
    /// </param>
    /// <remarks>
    /// Esta operación no valida si la categoría padre realmente existe.
    /// Esa responsabilidad corresponde a la capa de aplicación o persistencia.
    /// 
    /// Aquí solo se protegen las invariantes propias de la entidad.
    /// </remarks>
    public void ReasignarPadre(Guid? parentCategoryId)
    {
        ParentCategoryId = ValidarPadre(parentCategoryId, Id);
        MarcarActualizacion();
    }

    /// <summary>
    /// Convierte la categoría en una categoría raíz.
    /// </summary>
    public void ConvertirEnCategoriaRaiz()
    {
        if (EsCategoriaRaiz)
        {
            return;
        }

        ParentCategoryId = null;
        MarcarActualizacion();
    }

    /// <summary>
    /// Activa la categoría para su uso en el sistema.
    /// </summary>
    public void Activar()
    {
        if (Activa)
        {
            return;
        }

        Activa = true;
        MarcarActualizacion();
    }

    /// <summary>
    /// Desactiva la categoría para impedir su uso en nuevas operaciones del sistema.
    /// </summary>
    public void Desactivar()
    {
        if (!Activa)
        {
            return;
        }

        Activa = false;
        MarcarActualizacion();
    }

    /// <summary>
    /// Determina si la categoría tiene una categoría padre específica.
    /// </summary>
    /// <param name="parentCategoryId">Identificador de la categoría padre a validar.</param>
    /// <returns>
    /// <see langword="true"/> si la categoría depende del identificador indicado;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    public bool TienePadre(Guid parentCategoryId)
    {
        return parentCategoryId != Guid.Empty && ParentCategoryId == parentCategoryId;
    }

    #endregion

    #region Validaciones internas

    /// <summary>
    /// Valida el nombre de la categoría.
    /// </summary>
    /// <param name="nombre">Nombre a validar.</param>
    /// <returns>Nombre normalizado.</returns>
    /// <exception cref="DomainException">Se lanza si el nombre no cumple las reglas del dominio.</exception>
    private static string ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DomainException("El nombre de la categoría es obligatorio.");
        }

        nombre = nombre.Trim();

        if (nombre.Length > NombreMaxLength)
        {
            throw new DomainException($"El nombre de la categoría no puede superar los {NombreMaxLength} caracteres.");
        }

        return nombre;
    }

    /// <summary>
    /// Valida el slug de la categoría.
    /// </summary>
    /// <param name="slug">Slug a validar.</param>
    /// <returns>Slug normalizado.</returns>
    /// <exception cref="DomainException">Se lanza si el slug no cumple las reglas del dominio.</exception>
    private static string ValidarSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainException("El slug de la categoría es obligatorio.");
        }

        slug = slug.Trim().ToLowerInvariant();

        if (slug.Length > SlugMaxLength)
        {
            throw new DomainException($"El slug de la categoría no puede superar los {SlugMaxLength} caracteres.");
        }

        if (slug.Contains(' '))
        {
            throw new DomainException("El slug de la categoría no puede contener espacios.");
        }

        return slug;
    }

    /// <summary>
    /// Valida la descripción de la categoría.
    /// </summary>
    /// <param name="descripcion">Descripción a validar.</param>
    /// <returns>Descripción normalizada o null.</returns>
    /// <exception cref="DomainException">Se lanza si la descripción supera la longitud permitida.</exception>
    private static string? ValidarDescripcion(string? descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            return null;
        }

        descripcion = descripcion.Trim();

        if (descripcion.Length > DescripcionMaxLength)
        {
            throw new DomainException($"La descripción de la categoría no puede superar los {DescripcionMaxLength} caracteres.");
        }

        return descripcion;
    }

    /// <summary>
    /// Valida la referencia a la categoría padre.
    /// </summary>
    /// <param name="parentCategoryId">Identificador de categoría padre.</param>
    /// <returns>Valor validado.</returns>
    private static Guid? ValidarPadre(Guid? parentCategoryId, Guid currentCategoryId)
    {
        if (parentCategoryId == Guid.Empty)
        {
            throw new DomainException("El identificador de la categoría padre no puede ser vacío.");
        }

        if (parentCategoryId.HasValue && parentCategoryId.Value == currentCategoryId)
        {
            throw new DomainException("Una categoría no puede referenciarse a sí misma como categoría padre.");
        }

        return parentCategoryId;
    }

    #endregion
}