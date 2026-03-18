using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Catalog.DTOs;
using PlataformaECommerce.Application.Features.Catalog.Queries;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Enums;

namespace PlataformaECommerce.Application.Features.Catalog.Services;

/// <summary>
/// Proporciona los casos de uso de aplicación relacionados con la consulta
/// del catálogo comercial y de los productos destacados del e-Commerce.
/// </summary>
/// <remarks>
/// Esta clase actúa como servicio de aplicación especializado en escenarios
/// de lectura del catálogo, encapsulando la orquestación necesaria para:
///
/// - recuperar productos desde el repositorio,
/// - aplicar filtros funcionales y comerciales,
/// - ordenar resultados,
/// - paginar colecciones,
/// - proyectar entidades de dominio hacia DTOs de catálogo,
/// - y entregar respuestas consistentes a la capa superior.
///
/// Su propósito es desacoplar la capa Web, API o UI respecto de la lógica
/// de composición de consultas del catálogo, manteniendo una arquitectura
/// clara, profesional y mantenible.
///
/// Este servicio no introduce reglas de negocio del dominio; únicamente
/// coordina operaciones de consulta y proyección sobre el agregado
/// <see cref="Producto"/> y sus especializaciones.
/// </remarks>
public sealed class CatalogApplicationService
{
    #region Constantes internas

    /// <summary>
    /// Longitud máxima utilizada para construir descripciones resumidas.
    /// </summary>
    private const int ShortDescriptionMaxLength = 140;

    #endregion

    #region Campos privados

    /// <summary>
    /// Repositorio de productos.
    /// </summary>
    private readonly IProductRepository _productRepository;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="CatalogApplicationService"/>.
    /// </summary>
    /// <param name="productRepository">Repositorio de productos.</param>
    public CatalogApplicationService(IProductRepository productRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    #endregion

    #region Casos de uso públicos

    /// <summary>
    /// Obtiene la colección de productos del catálogo aplicando búsqueda,
    /// filtrado, ordenamiento y paginación.
    /// </summary>
    /// <param name="query">Consulta de catálogo a ejecutar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la colección de productos del catálogo cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<IReadOnlyCollection<CatalogProductDto>>> GetCatalogProductsAsync(
        GetCatalogProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        Error? validationError = ValidateCatalogQuery(query);
        if (validationError is not null)
        {
            return Result.Failure<IReadOnlyCollection<CatalogProductDto>>(validationError);
        }

        IReadOnlyCollection<Producto> sourceProducts = await GetCatalogSourceProductsAsync(query, cancellationToken);

        IEnumerable<Producto> filteredProducts = ApplyCatalogFilters(sourceProducts, query);
        IEnumerable<Producto> orderedProducts = ApplyCatalogSorting(filteredProducts, query);

        IReadOnlyCollection<CatalogProductDto> result = orderedProducts
            .Skip(query.Offset)
            .Take(query.NormalizedPageSize)
            .Select(product => MapToCatalogProductDto(product, query.IncludeImageGallery, query.IncludeCommercialMetrics))
            .ToArray();

        return Result.Success(result);
    }

    /// <summary>
    /// Obtiene la colección de productos destacados del catálogo
    /// aplicando filtros de vitrina, campaña y priorización comercial.
    /// </summary>
    /// <param name="query">Consulta de productos destacados.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>
    /// Un resultado con la colección de productos destacados cuando la operación es exitosa.
    /// </returns>
    public async Task<Result<IReadOnlyCollection<FeaturedProductDto>>> GetFeaturedProductsAsync(
        GetFeaturedProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        Error? validationError = ValidateFeaturedQuery(query);
        if (validationError is not null)
        {
            return Result.Failure<IReadOnlyCollection<FeaturedProductDto>>(validationError);
        }

        IReadOnlyCollection<Producto> sourceProducts = await _productRepository.GetFeaturedProductsAsync(cancellationToken);

        IEnumerable<Producto> filteredProducts = ApplyFeaturedFilters(sourceProducts, query);
        IEnumerable<Producto> orderedProducts = ApplyFeaturedSorting(filteredProducts, query);

        IReadOnlyCollection<FeaturedProductDto> result = orderedProducts
            .Take(query.NormalizedTake)
            .Select(product => MapToFeaturedProductDto(product, query))
            .ToArray();

        return Result.Success(result);
    }

    #endregion

    #region Validaciones privadas

    /// <summary>
    /// Valida estructuralmente la consulta de catálogo.
    /// </summary>
    /// <param name="query">Consulta a validar.</param>
    /// <returns>
    /// Un error de validación cuando la consulta es inválida;
    /// en caso contrario, <see langword="null"/>.
    /// </returns>
    private static Error? ValidateCatalogQuery(GetCatalogProductsQuery query)
    {
        if (query.MinPrice.HasValue && query.MinPrice.Value < 0)
        {
            return Error.Validation("Catalog.InvalidMinPrice", "El precio mínimo no puede ser menor que cero.");
        }

        if (query.MaxPrice.HasValue && query.MaxPrice.Value < 0)
        {
            return Error.Validation("Catalog.InvalidMaxPrice", "El precio máximo no puede ser menor que cero.");
        }

        if (query.MinPrice.HasValue && query.MaxPrice.HasValue && query.MinPrice.Value > query.MaxPrice.Value)
        {
            return Error.Validation("Catalog.InvalidPriceRange", "El precio mínimo no puede ser mayor que el precio máximo.");
        }

        if (query.CategoryId.HasValue && query.CategoryId.Value == Guid.Empty)
        {
            return Error.Validation("Catalog.InvalidCategoryId", "El identificador de la categoría no es válido.");
        }

        if (query.SubcategoryId.HasValue && query.SubcategoryId.Value == Guid.Empty)
        {
            return Error.Validation("Catalog.InvalidSubcategoryId", "El identificador de la subcategoría no es válido.");
        }

        if (query.RequestedByUserId.HasValue && query.RequestedByUserId.Value == Guid.Empty)
        {
            return Error.Validation("Catalog.InvalidRequestedByUserId", "El identificador del usuario solicitante no es válido.");
        }

        return null;
    }

    /// <summary>
    /// Valida estructuralmente la consulta de productos destacados.
    /// </summary>
    /// <param name="query">Consulta a validar.</param>
    /// <returns>
    /// Un error de validación cuando la consulta es inválida;
    /// en caso contrario, <see langword="null"/>.
    /// </returns>
    private static Error? ValidateFeaturedQuery(GetFeaturedProductsQuery query)
    {
        if (query.CategoryId.HasValue && query.CategoryId.Value == Guid.Empty)
        {
            return Error.Validation("Catalog.InvalidCategoryId", "El identificador de la categoría no es válido.");
        }

        if (query.RequestedByUserId.HasValue && query.RequestedByUserId.Value == Guid.Empty)
        {
            return Error.Validation("Catalog.InvalidRequestedByUserId", "El identificador del usuario solicitante no es válido.");
        }

        if (query.ReferenceDateUtc.HasValue && query.ReferenceDateUtc.Value.Kind != DateTimeKind.Utc)
        {
            return Error.Validation("Catalog.InvalidReferenceDateUtc", "La fecha de referencia debe estar expresada en UTC.");
        }

        return null;
    }

    #endregion

    #region Fuentes de consulta

    /// <summary>
    /// Obtiene la fuente base de productos para la consulta de catálogo,
    /// priorizando la opción más eficiente según los filtros solicitados.
    /// </summary>
    /// <param name="query">Consulta de catálogo.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Colección base de productos.</returns>
    private async Task<IReadOnlyCollection<Producto>> GetCatalogSourceProductsAsync(
        GetCatalogProductsQuery query,
        CancellationToken cancellationToken)
    {
        bool mustLoadAllProducts =
            query.IncludeInactive ||
            query.IsActive == false;

        if (mustLoadAllProducts)
        {
            return await _productRepository.GetAllAsync(cancellationToken);
        }

        return await _productRepository.GetActiveProductsAsync(cancellationToken);
    }

    #endregion

    #region Filtros de catálogo

    /// <summary>
    /// Aplica los filtros funcionales y comerciales a la colección de productos del catálogo.
    /// </summary>
    /// <param name="products">Colección base de productos.</param>
    /// <param name="query">Consulta con criterios de filtrado.</param>
    /// <returns>Colección filtrada.</returns>
    private static IEnumerable<Producto> ApplyCatalogFilters(
        IEnumerable<Producto> products,
        GetCatalogProductsQuery query)
    {
        IEnumerable<Producto> filtered = products;

        if (query.IsActive.HasValue)
        {
            filtered = filtered.Where(product => product.Activo == query.IsActive.Value);
        }
        else if (!query.IncludeInactive)
        {
            filtered = filtered.Where(product => product.Activo);
        }

        if (query.IsAvailable.HasValue)
        {
            filtered = filtered.Where(product => product.EstaDisponible() == query.IsAvailable.Value);
        }

        if (query.HasStock.HasValue)
        {
            filtered = filtered.Where(product => product.TieneStock() == query.HasStock.Value);
        }

        if (query.IsFeatured.HasValue)
        {
            filtered = filtered.Where(product => product.Destacado == query.IsFeatured.Value);
        }

        if (query.IsOnSale == true)
        {
            filtered = filtered.Where(_ => false);
        }

        if (query.IsNew == true)
        {
            filtered = filtered.Where(product => IsNewProduct(product));
        }

        if (query.IsRecommended == true)
        {
            filtered = filtered.Where(product => product.Destacado);
        }

        if (query.ProductType.HasValue)
        {
            filtered = filtered.Where(product => product.TipoProducto == query.ProductType.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Sku))
        {
            string normalizedSku = query.Sku.Trim();
            filtered = filtered.Where(product =>
                string.Equals(product.Sku.Value, normalizedSku, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            string normalizedSearchTerm = query.SearchTerm.Trim();
            filtered = filtered.Where(product => MatchesCatalogSearch(product, normalizedSearchTerm));
        }

        if (!string.IsNullOrWhiteSpace(query.Slug))
        {
            string normalizedSlug = query.Slug.Trim();
            filtered = filtered.Where(product =>
                string.Equals(product.Slug, normalizedSlug, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Brand))
        {
            string normalizedBrand = query.Brand.Trim();
            filtered = filtered.Where(product => MatchesBrand(product, normalizedBrand));
        }

        if (!string.IsNullOrWhiteSpace(query.CategoryName))
        {
            string normalizedCategoryName = query.CategoryName.Trim();
            filtered = filtered.Where(product => MatchesCategory(product, normalizedCategoryName));
        }

        if (!string.IsNullOrWhiteSpace(query.SubcategoryName))
        {
            string normalizedSubcategoryName = query.SubcategoryName.Trim();
            filtered = filtered.Where(product => MatchesSubcategory(product, normalizedSubcategoryName));
        }

        if (!string.IsNullOrWhiteSpace(query.Tag))
        {
            string normalizedTag = query.Tag.Trim();
            filtered = filtered.Where(product => MatchesTag(product, normalizedTag));
        }

        if (query.MinPrice.HasValue)
        {
            filtered = filtered.Where(product => product.Precio.Amount >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            filtered = filtered.Where(product => product.Precio.Amount <= query.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Currency))
        {
            string normalizedCurrency = query.Currency.Trim();
            filtered = filtered.Where(product =>
                string.Equals(product.Precio.Currency, normalizedCurrency, StringComparison.OrdinalIgnoreCase));
        }

        return filtered;
    }

    #endregion

    #region Ordenamiento de catálogo

    /// <summary>
    /// Aplica el ordenamiento solicitado sobre la colección filtrada del catálogo.
    /// </summary>
    /// <param name="products">Colección filtrada.</param>
    /// <param name="query">Consulta que define el ordenamiento.</param>
    /// <returns>Colección ordenada.</returns>
    private static IEnumerable<Producto> ApplyCatalogSorting(
        IEnumerable<Producto> products,
        GetCatalogProductsQuery query)
    {
        string sortBy = query.SortBy?.Trim().ToLowerInvariant() ?? "relevance";

        return sortBy switch
        {
            "name" => query.SortDescending
                ? products.OrderByDescending(product => product.Nombre).ThenByDescending(product => product.FechaCreacionUtc)
                : products.OrderBy(product => product.Nombre).ThenBy(product => product.FechaCreacionUtc),

            "price" => query.SortDescending
                ? products.OrderByDescending(product => product.Precio.Amount).ThenBy(product => product.Nombre)
                : products.OrderBy(product => product.Precio.Amount).ThenBy(product => product.Nombre),

            "createdat" => query.SortDescending
                ? products.OrderByDescending(product => product.FechaCreacionUtc)
                : products.OrderBy(product => product.FechaCreacionUtc),

            "featured" => query.SortDescending
                ? products.OrderByDescending(product => product.Destacado)
                    .ThenByDescending(product => product.EstaDisponible())
                    .ThenBy(product => product.Nombre)
                : products.OrderBy(product => product.Destacado)
                    .ThenBy(product => product.EstaDisponible())
                    .ThenBy(product => product.Nombre),

            "rating" => query.SortDescending
                ? products.OrderByDescending(_ => 0m).ThenBy(product => product.Nombre)
                : products.OrderBy(_ => 0m).ThenBy(product => product.Nombre),

            "sales" => query.SortDescending
                ? products.OrderByDescending(_ => 0).ThenBy(product => product.Nombre)
                : products.OrderBy(_ => 0).ThenBy(product => product.Nombre),

            "relevance" => query.SortDescending
                ? products.OrderByDescending(product => CalculateCatalogRelevanceScore(product, query))
                    .ThenByDescending(product => product.Destacado)
                    .ThenByDescending(product => product.EstaDisponible())
                    .ThenBy(product => product.Nombre)
                : products.OrderBy(product => CalculateCatalogRelevanceScore(product, query))
                    .ThenBy(product => product.Nombre),

            _ => query.SortDescending
                ? products.OrderByDescending(product => CalculateCatalogRelevanceScore(product, query))
                    .ThenByDescending(product => product.FechaCreacionUtc)
                : products.OrderBy(product => product.Nombre)
                    .ThenBy(product => product.FechaCreacionUtc)
        };
    }

    #endregion

    #region Filtros de destacados

    /// <summary>
    /// Aplica los filtros funcionales y comerciales a la colección de productos destacados.
    /// </summary>
    /// <param name="products">Colección base de productos destacados.</param>
    /// <param name="query">Consulta de destacados.</param>
    /// <returns>Colección filtrada de destacados.</returns>
    private static IEnumerable<Producto> ApplyFeaturedFilters(
        IEnumerable<Producto> products,
        GetFeaturedProductsQuery query)
    {
        IEnumerable<Producto> filtered = products.Where(product => product.Destacado);

        if (query.OnlyAvailable.HasValue)
        {
            filtered = filtered.Where(product => product.EstaDisponible() == query.OnlyAvailable.Value);
        }

        if (query.OnlyWithStock.HasValue)
        {
            filtered = filtered.Where(product => product.TieneStock() == query.OnlyWithStock.Value);
        }

        if (query.OnlyOnSale == true)
        {
            filtered = filtered.Where(_ => false);
        }

        if (query.OnlyNew == true)
        {
            filtered = filtered.Where(product => IsNewProduct(product));
        }

        if (query.OnlyRecommended == true)
        {
            filtered = filtered.Where(product => product.Destacado);
        }

        if (query.OnlyBestSellers == true)
        {
            filtered = filtered.Where(_ => false);
        }

        if (query.ProductType.HasValue)
        {
            filtered = filtered.Where(product => product.TipoProducto == query.ProductType.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Brand))
        {
            string normalizedBrand = query.Brand.Trim();
            filtered = filtered.Where(product => MatchesBrand(product, normalizedBrand));
        }

        if (!string.IsNullOrWhiteSpace(query.CategoryName))
        {
            string normalizedCategoryName = query.CategoryName.Trim();
            filtered = filtered.Where(product => MatchesCategory(product, normalizedCategoryName));
        }

        if (!string.IsNullOrWhiteSpace(query.BadgeText))
        {
            string normalizedBadgeText = query.BadgeText.Trim();
            filtered = filtered.Where(product => MatchesFeaturedBadge(product, normalizedBadgeText));
        }

        if (!string.IsNullOrWhiteSpace(query.CampaignKey))
        {
            string normalizedCampaignKey = query.CampaignKey.Trim();
            filtered = filtered.Where(product => MatchesFeaturedCampaign(product, normalizedCampaignKey));
        }

        if (!string.IsNullOrWhiteSpace(query.Placement))
        {
            string normalizedPlacement = query.Placement.Trim();
            filtered = filtered.Where(product => MatchesFeaturedPlacement(product, normalizedPlacement));
        }

        return filtered;
    }

    #endregion

    #region Ordenamiento de destacados

    /// <summary>
    /// Aplica el ordenamiento solicitado a los productos destacados.
    /// </summary>
    /// <param name="products">Colección filtrada.</param>
    /// <param name="query">Consulta que define el ordenamiento.</param>
    /// <returns>Colección ordenada.</returns>
    private static IEnumerable<Producto> ApplyFeaturedSorting(
        IEnumerable<Producto> products,
        GetFeaturedProductsQuery query)
    {
        string sortBy = query.SortBy?.Trim().ToLowerInvariant() ?? "displayorder";

        return sortBy switch
        {
            "price" => query.SortDescending
                ? products.OrderByDescending(product => product.Precio.Amount).ThenBy(product => product.Nombre)
                : products.OrderBy(product => product.Precio.Amount).ThenBy(product => product.Nombre),

            "createdat" => query.SortDescending
                ? products.OrderByDescending(product => product.FechaCreacionUtc)
                : products.OrderBy(product => product.FechaCreacionUtc),

            "rating" => query.SortDescending
                ? products.OrderByDescending(_ => 0m).ThenBy(product => product.Nombre)
                : products.OrderBy(_ => 0m).ThenBy(product => product.Nombre),

            "sales" => query.SortDescending
                ? products.OrderByDescending(_ => 0).ThenBy(product => product.Nombre)
                : products.OrderBy(_ => 0).ThenBy(product => product.Nombre),

            "relevance" => query.SortDescending
                ? products.OrderByDescending(product => CalculateFeaturedScore(product, query))
                    .ThenBy(product => product.Nombre)
                : products.OrderBy(product => CalculateFeaturedScore(product, query))
                    .ThenBy(product => product.Nombre),

            "displayorder" => query.SortDescending
                ? products.OrderByDescending(product => CalculateFeaturedScore(product, query))
                    .ThenByDescending(product => product.FechaCreacionUtc)
                : products.OrderBy(product => product.Nombre)
                    .ThenBy(product => product.FechaCreacionUtc),

            _ => query.SortDescending
                ? products.OrderByDescending(product => CalculateFeaturedScore(product, query))
                    .ThenByDescending(product => product.FechaCreacionUtc)
                : products.OrderBy(product => product.Nombre)
                    .ThenBy(product => product.FechaCreacionUtc)
        };
    }

    #endregion

    #region Mapeos privados

    /// <summary>
    /// Proyecta una entidad <see cref="Producto"/> hacia un <see cref="CatalogProductDto"/>.
    /// </summary>
    /// <param name="product">Producto origen.</param>
    /// <param name="includeImageGallery">Indica si debe incluirse galería de imágenes.</param>
    /// <param name="includeCommercialMetrics">Indica si deben incluirse métricas comerciales.</param>
    /// <returns>DTO de catálogo.</returns>
    private static CatalogProductDto MapToCatalogProductDto(
        Producto product,
        bool includeImageGallery,
        bool includeCommercialMetrics)
    {
        ArgumentNullException.ThrowIfNull(product);

        IReadOnlyCollection<string> tags = BuildProductTags(product);
        IReadOnlyCollection<string> imageUrls = includeImageGallery
            ? BuildImageGallery(product)
            : Array.Empty<string>();

        return new CatalogProductDto
        {
            Id = product.Id,
            Sku = product.Sku.Value,
            Name = product.Nombre,
            ShortName = BuildShortName(product.Nombre),
            Slug = product.Slug,
            Description = product.Descripcion,
            ShortDescription = BuildShortDescription(product.Descripcion),
            Brand = BuildBrand(product),
            CategoryName = BuildCategoryName(product),
            SubcategoryName = BuildSubcategoryName(product),
            Tags = tags,
            ProductType = product.TipoProducto,
            IsAvailable = product.EstaDisponible(),
            IsActive = product.Activo,
            IsFeatured = product.Destacado,
            IsNew = IsNewProduct(product),
            IsRecommended = product.Destacado,
            HasStock = product.TieneStock(),
            AvailableStock = product.Stock,
            Price = product.Precio.Amount,
            Currency = product.Precio.Currency,
            PreviousPrice = null,
            DiscountAmount = null,
            DiscountPercentage = null,
            IsOnSale = false,
            MainImageUrl = product.ImagenPrincipalUrl,
            SecondaryImageUrl = null,
            ImageUrls = imageUrls,
            AverageRating = includeCommercialMetrics ? null : null,
            ReviewCount = includeCommercialMetrics ? 0 : 0,
            SalesCount = includeCommercialMetrics ? null : null,
            CreatedAtUtc = product.FechaCreacionUtc,
            UpdatedAtUtc = product.FechaActualizacionUtc,
            PublishedAtUtc = product.Activo ? product.FechaCreacionUtc : null
        };
    }

    /// <summary>
    /// Proyecta una entidad <see cref="Producto"/> hacia un <see cref="FeaturedProductDto"/>.
    /// </summary>
    /// <param name="product">Producto origen.</param>
    /// <param name="query">Consulta de destacados utilizada como contexto.</param>
    /// <returns>DTO de producto destacado.</returns>
    private static FeaturedProductDto MapToFeaturedProductDto(
        Producto product,
        GetFeaturedProductsQuery query)
    {
        ArgumentNullException.ThrowIfNull(product);

        IReadOnlyCollection<string> imageUrls = query.IncludeVisualAssets
            ? BuildImageGallery(product)
            : Array.Empty<string>();

        string? badgeText = BuildFeaturedBadgeText(product, query);

        return new FeaturedProductDto
        {
            Id = product.Id,
            Sku = product.Sku.Value,
            Name = product.Nombre,
            Slug = product.Slug,
            PromotionalTitle = BuildPromotionalTitle(product, query),
            PromotionalText = BuildPromotionalText(product, query),
            BadgeText = badgeText,
            CategoryName = BuildCategoryName(product),
            Brand = BuildBrand(product),
            ProductType = product.TipoProducto,
            Price = product.Precio.Amount,
            Currency = product.Precio.Currency,
            PreviousPrice = null,
            DiscountAmount = null,
            DiscountPercentage = null,
            IsOnSale = false,
            IsAvailable = product.EstaDisponible(),
            HasStock = product.TieneStock(),
            IsNew = IsNewProduct(product),
            IsRecommended = product.Destacado,
            IsBestSeller = false,
            IsReadyToBuy = product.EstaDisponible(),
            MainImageUrl = product.ImagenPrincipalUrl,
            HeroImageUrl = product.ImagenPrincipalUrl,
            BannerImageUrl = null,
            ImageUrls = imageUrls,
            AverageRating = query.IncludeCommercialMetrics ? null : null,
            ReviewCount = query.IncludeCommercialMetrics ? 0 : 0,
            DisplayOrder = null,
            ProductUrl = BuildProductUrl(product),
            CallToActionText = BuildCallToActionText(product),
            FeaturedFromUtc = null,
            FeaturedToUtc = null,
            CreatedAtUtc = product.FechaCreacionUtc,
            UpdatedAtUtc = product.FechaActualizacionUtc
        };
    }

    #endregion

    #region Métodos auxiliares de búsqueda y coincidencia

    /// <summary>
    /// Determina si un producto coincide con el término de búsqueda del catálogo.
    /// </summary>
    /// <param name="product">Producto a evaluar.</param>
    /// <param name="searchTerm">Término de búsqueda normalizado.</param>
    /// <returns>
    /// <see langword="true"/> si el producto coincide con el término;
    /// en caso contrario, <see langword="false"/>.
    /// </returns>
    private static bool MatchesCatalogSearch(Producto product, string searchTerm)
    {
        return ContainsInsensitive(product.Nombre, searchTerm)
            || ContainsInsensitive(product.Descripcion, searchTerm)
            || ContainsInsensitive(product.Sku.Value, searchTerm)
            || ContainsInsensitive(product.Slug, searchTerm)
            || ContainsInsensitive(BuildBrand(product), searchTerm)
            || ContainsInsensitive(BuildCategoryName(product), searchTerm)
            || ContainsInsensitive(BuildSubcategoryName(product), searchTerm)
            || BuildProductTags(product).Any(tag => ContainsInsensitive(tag, searchTerm));
    }

    /// <summary>
    /// Determina si un producto coincide con la marca solicitada.
    /// </summary>
    private static bool MatchesBrand(Producto product, string brand)
    {
        return ContainsInsensitive(BuildBrand(product), brand);
    }

    /// <summary>
    /// Determina si un producto coincide con la categoría solicitada.
    /// </summary>
    private static bool MatchesCategory(Producto product, string categoryName)
    {
        return ContainsInsensitive(BuildCategoryName(product), categoryName);
    }

    /// <summary>
    /// Determina si un producto coincide con la subcategoría solicitada.
    /// </summary>
    private static bool MatchesSubcategory(Producto product, string subcategoryName)
    {
        return ContainsInsensitive(BuildSubcategoryName(product), subcategoryName);
    }

    /// <summary>
    /// Determina si un producto coincide con una etiqueta solicitada.
    /// </summary>
    private static bool MatchesTag(Producto product, string tag)
    {
        return BuildProductTags(product).Any(currentTag => ContainsInsensitive(currentTag, tag));
    }

    /// <summary>
    /// Determina si un producto coincide con una etiqueta destacada solicitada.
    /// </summary>
    private static bool MatchesFeaturedBadge(Producto product, string badgeText)
    {
        string? effectiveBadge = BuildFeaturedBadgeText(product, null);
        return ContainsInsensitive(effectiveBadge, badgeText);
    }

    /// <summary>
    /// Determina si un producto coincide con una campaña lógica solicitada.
    /// </summary>
    private static bool MatchesFeaturedCampaign(Producto product, string campaignKey)
    {
        if (ContainsInsensitive(campaignKey, "recommended"))
        {
            return product.Destacado;
        }

        if (ContainsInsensitive(campaignKey, "new"))
        {
            return IsNewProduct(product);
        }

        if (ContainsInsensitive(campaignKey, "digital"))
        {
            return product.TipoProducto == TipoProducto.Digital;
        }

        if (ContainsInsensitive(campaignKey, "physical"))
        {
            return product.TipoProducto == TipoProducto.Fisico;
        }

        if (ContainsInsensitive(campaignKey, "home"))
        {
            return product.Destacado;
        }

        return true;
    }

    /// <summary>
    /// Determina si un producto coincide con un slot visual solicitado.
    /// </summary>
    private static bool MatchesFeaturedPlacement(Producto product, string placement)
    {
        if (ContainsInsensitive(placement, "hero"))
        {
            return product.Destacado && product.EstaDisponible();
        }

        if (ContainsInsensitive(placement, "slider"))
        {
            return product.Destacado;
        }

        if (ContainsInsensitive(placement, "home-grid"))
        {
            return product.Destacado && product.Activo;
        }

        return true;
    }

    #endregion

    #region Métodos auxiliares de scoring

    /// <summary>
    /// Calcula un puntaje de relevancia aproximado para el catálogo.
    /// </summary>
    /// <param name="product">Producto a evaluar.</param>
    /// <param name="query">Consulta de catálogo.</param>
    /// <returns>Puntaje de relevancia aproximado.</returns>
    private static int CalculateCatalogRelevanceScore(Producto product, GetCatalogProductsQuery query)
    {
        int score = 0;

        if (product.Destacado)
        {
            score += 20;
        }

        if (product.EstaDisponible())
        {
            score += 15;
        }

        if (product.TieneStock())
        {
            score += 10;
        }

        if (IsNewProduct(product))
        {
            score += 5;
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            string normalizedSearchTerm = query.SearchTerm.Trim();

            if (ContainsInsensitive(product.Nombre, normalizedSearchTerm))
            {
                score += 30;
            }

            if (ContainsInsensitive(product.Sku.Value, normalizedSearchTerm))
            {
                score += 25;
            }

            if (ContainsInsensitive(product.Descripcion, normalizedSearchTerm))
            {
                score += 10;
            }

            if (ContainsInsensitive(product.Slug, normalizedSearchTerm))
            {
                score += 10;
            }
        }

        return score;
    }

    /// <summary>
    /// Calcula un puntaje aproximado para priorizar productos destacados.
    /// </summary>
    /// <param name="product">Producto a evaluar.</param>
    /// <param name="query">Consulta de destacados.</param>
    /// <returns>Puntaje de priorización.</returns>
    private static int CalculateFeaturedScore(Producto product, GetFeaturedProductsQuery query)
    {
        int score = 0;

        if (product.Destacado)
        {
            score += 30;
        }

        if (product.EstaDisponible())
        {
            score += 20;
        }

        if (product.TieneStock())
        {
            score += 10;
        }

        if (IsNewProduct(product))
        {
            score += 5;
        }

        if (query.OnlyRecommended == true && product.Destacado)
        {
            score += 10;
        }

        if (query.ProductType.HasValue && product.TipoProducto == query.ProductType.Value)
        {
            score += 5;
        }

        return score;
    }

    #endregion

    #region Métodos auxiliares de construcción de DTOs

    /// <summary>
    /// Construye una marca aproximada a partir de la información disponible del producto.
    /// </summary>
    private static string? BuildBrand(Producto product)
    {
        string firstToken = product.Nombre
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? string.Empty;

        return string.IsNullOrWhiteSpace(firstToken) ? null : firstToken;
    }

    /// <summary>
    /// Construye una categoría aproximada a partir del tipo de producto.
    /// </summary>
    private static string BuildCategoryName(Producto product)
    {
        return product.TipoProducto switch
        {
            TipoProducto.Fisico => "Productos físicos",
            TipoProducto.Digital => "Productos digitales",
            _ => "Catálogo general"
        };
    }

    /// <summary>
    /// Construye una subcategoría aproximada según la especialización del producto.
    /// </summary>
    private static string? BuildSubcategoryName(Producto product)
    {
        return product switch
        {
            ProductoFisico physicalProduct when physicalProduct.RequiereEnvio => "Con envío",
            ProductoFisico => "Entrega física",
            ProductoDigital digitalProduct when digitalProduct.RequiereLicencia => "Con licencia",
            ProductoDigital => "Descarga inmediata",
            _ => null
        };
    }

    /// <summary>
    /// Construye el conjunto de etiquetas base del producto.
    /// </summary>
    private static IReadOnlyCollection<string> BuildProductTags(Producto product)
    {
        List<string> tags =
        [
            product.TipoProducto == TipoProducto.Fisico ? "Físico" : "Digital"
        ];

        if (product.Destacado)
        {
            tags.Add("Destacado");
        }

        if (product.EstaDisponible())
        {
            tags.Add("Disponible");
        }

        if (product.TieneStock())
        {
            tags.Add("Con stock");
        }

        if (IsNewProduct(product))
        {
            tags.Add("Nuevo");
        }

        if (product is ProductoDigital digitalProduct && digitalProduct.RequiereLicencia)
        {
            tags.Add("Licencia");
        }

        if (product is ProductoFisico physicalProduct && physicalProduct.RequiereEnvio)
        {
            tags.Add("Envío");
        }

        return tags
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Construye una galería simple de imágenes a partir de la imagen principal.
    /// </summary>
    private static IReadOnlyCollection<string> BuildImageGallery(Producto product)
    {
        if (string.IsNullOrWhiteSpace(product.ImagenPrincipalUrl))
        {
            return Array.Empty<string>();
        }

        return new[] { product.ImagenPrincipalUrl };
    }

    /// <summary>
    /// Construye un nombre corto a partir del nombre del producto.
    /// </summary>
    private static string BuildShortName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        string trimmedName = name.Trim();

        return trimmedName.Length <= 60
            ? trimmedName
            : $"{trimmedName[..57]}...";
    }

    /// <summary>
    /// Construye una descripción corta a partir de la descripción completa.
    /// </summary>
    private static string BuildShortDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return string.Empty;
        }

        string trimmedDescription = description.Trim();

        return trimmedDescription.Length <= ShortDescriptionMaxLength
            ? trimmedDescription
            : $"{trimmedDescription[..(ShortDescriptionMaxLength - 3)]}...";
    }

    /// <summary>
    /// Determina si un producto puede considerarse nuevo.
    /// </summary>
    private static bool IsNewProduct(Producto product)
    {
        return product.FechaCreacionUtc >= DateTime.UtcNow.AddDays(-30);
    }

    /// <summary>
    /// Construye el texto de la insignia comercial principal del producto destacado.
    /// </summary>
    private static string? BuildFeaturedBadgeText(Producto product, GetFeaturedProductsQuery? query)
    {
        if (!string.IsNullOrWhiteSpace(query?.BadgeText))
        {
            return query.BadgeText!.Trim();
        }

        if (IsNewProduct(product))
        {
            return "Nuevo";
        }

        if (product.Destacado)
        {
            return "Destacado";
        }

        return null;
    }

    /// <summary>
    /// Construye un título promocional para el producto destacado.
    /// </summary>
    private static string BuildPromotionalTitle(Producto product, GetFeaturedProductsQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.CampaignKey))
        {
            return $"{product.Nombre} · Selección especial";
        }

        return product.Destacado
            ? $"{product.Nombre} destacado para ti"
            : product.Nombre;
    }

    /// <summary>
    /// Construye un texto promocional resumido para el producto destacado.
    /// </summary>
    private static string BuildPromotionalText(Producto product, GetFeaturedProductsQuery query)
    {
        if (product.TipoProducto == TipoProducto.Digital)
        {
            return "Disponible para compra y acceso digital según configuración del producto.";
        }

        if (product.EstaDisponible())
        {
            return "Producto disponible para compra inmediata en el catálogo.";
        }

        return BuildShortDescription(product.Descripcion);
    }

    /// <summary>
    /// Construye la URL pública del producto.
    /// </summary>
    private static string BuildProductUrl(Producto product)
    {
        return string.IsNullOrWhiteSpace(product.Slug)
            ? $"/catalog/products/{product.Id}"
            : $"/catalog/{product.Slug}";
    }

    /// <summary>
    /// Construye el texto de llamada a la acción del producto.
    /// </summary>
    private static string BuildCallToActionText(Producto product)
    {
        return product.EstaDisponible()
            ? "Ver producto"
            : "Conocer más";
    }

    /// <summary>
    /// Evalúa si un texto contiene otro valor ignorando mayúsculas y minúsculas.
    /// </summary>
    private static bool ContainsInsensitive(string? source, string? value)
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return source.Contains(value.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}