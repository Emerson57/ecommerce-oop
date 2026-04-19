using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Mappings;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Interfaces.Repositories.Products;

/// <summary>
/// Define el contrato del repositorio responsable de la persistencia
/// y recuperación de entidades <see cref="Producto"/> dentro del sistema.
/// </summary>
/// <remarks>
/// Este repositorio forma parte de la capa Application y actúa como
/// abstracción de acceso a datos para el agregado de productos.
///
/// Su responsabilidad es proporcionar operaciones de consulta
/// y persistencia sin exponer detalles de infraestructura como:
/// - ORM utilizados (Entity Framework, Dapper, etc.)
/// - motores de base de datos
/// - mecanismos de almacenamiento.
///
/// En una arquitectura basada en DDD y Clean Architecture,
/// el repositorio es implementado en la capa Infrastructure
/// y consumido por:
/// - servicios de aplicación,
/// - consultas de aplicación,
/// - y componentes especializados de orquestación.
///
/// La interfaz se mantiene orientada al agregado <see cref="Producto"/>
/// y no expone DTOs ni estructuras de infraestructura.
/// </remarks>
public interface IProductRepository
{
    #region Consultas generales

    /// <summary>
    /// Obtiene todos los productos registrados en el sistema.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una colección de productos registrados.
    /// </returns>
    Task<IReadOnlyCollection<Producto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un producto por su identificador único.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// El producto encontrado o <see langword="null"/> si no existe.
    /// </returns>
    Task<Producto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un producto a partir de su SKU.
    /// </summary>
    /// <param name="sku">SKU del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// El producto encontrado o <see langword="null"/> si no existe.
    /// </returns>
    Task<Producto?> GetBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default);

    #endregion

    #region Consultas de catálogo

    /// <summary>
    /// Obtiene los productos activos disponibles en el catálogo.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una colección de productos activos.
    /// </returns>
    Task<IReadOnlyCollection<Producto>> GetActiveProductsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los productos destacados del catálogo.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una colección de productos destacados.
    /// </returns>
    Task<IReadOnlyCollection<Producto>> GetFeaturedProductsAsync(
        CancellationToken cancellationToken = default);

    #endregion

    #region Validaciones de existencia

    /// <summary>
    /// Verifica si existe un producto con el identificador indicado.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// <see langword="true"/> si el producto existe;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    Task<bool> ExistsByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe un producto con el SKU indicado.
    /// </summary>
    /// <param name="sku">SKU del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// <see langword="true"/> si el producto existe;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    Task<bool> ExistsBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default);

    #endregion

    #region Persistencia

    /// <summary>
    /// Agrega un nuevo producto al repositorio.
    /// </summary>
    /// <param name="producto">Entidad producto a persistir.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona.
    /// </returns>
    Task AddAsync(
        Producto producto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un producto existente en el repositorio.
    /// </summary>
    /// <param name="producto">Entidad producto a actualizar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona.
    /// </returns>
    Task UpdateAsync(
        Producto producto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un producto del repositorio por su identificador.
    /// </summary>
    /// <param name="id">Identificador del producto a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona.
    /// </returns>
    Task RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    #endregion

    #region Consultas avanzadas (proyecciones para listado)

    /// <summary>
    /// Consulta productos aplicando filtros, ordenamiento y paginación
    /// y proyecta directamente a DTOs ligeros para listados.
    /// </summary>
    /// <returns>Tupla con items y total count.</returns>
    async Task<(IReadOnlyCollection<ProductDto> Items, int TotalCount)> QueryProductsAsync(
        GetProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Default fallback implementation for tests and legacy code: materialize all and project in-memory.
        // Implementations in Infrastructure should override for SQL execution.
        ArgumentNullException.ThrowIfNull(query);
        var list = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        var filtered = list.AsEnumerable();

        int total = filtered.Count();

        var items = filtered
            .Skip(query.Offset)
            .Take(query.NormalizedPageSize)
            .Select(p => p.ToProductDto())
            .ToArray();

        return (items, total);
    }

    #endregion
}