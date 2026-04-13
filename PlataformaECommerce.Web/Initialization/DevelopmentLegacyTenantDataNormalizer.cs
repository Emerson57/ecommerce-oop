using Microsoft.EntityFrameworkCore;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Infrastructure.Persistence.Context;

namespace PlataformaECommerce.Web.Initialization;

/// <summary>
/// Normaliza filas legacy sin tenant en bases locales antiguas para que respeten el aislamiento SaaS actual.
/// </summary>
public sealed class DevelopmentLegacyTenantDataNormalizer
{
    private static readonly EventId NoLegacyRowsDetectedEvent = new(41001, nameof(NoLegacyRowsDetectedEvent));
    private static readonly EventId LegacyRowsNormalizedEvent = new(41002, nameof(LegacyRowsNormalizedEvent));

    private readonly ECommerceDbContext _dbContext;
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<DevelopmentLegacyTenantDataNormalizer> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="DevelopmentLegacyTenantDataNormalizer"/>.
    /// </summary>
    public DevelopmentLegacyTenantDataNormalizer(
        ECommerceDbContext dbContext,
        ITenantContextAccessor tenantContextAccessor,
        IHostEnvironment hostEnvironment,
        ILogger<DevelopmentLegacyTenantDataNormalizer> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _tenantContextAccessor = tenantContextAccessor ?? throw new ArgumentNullException(nameof(tenantContextAccessor));
        _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Inspecciona de forma no destructiva si existen filas legacy sin tenant para el contexto actual.
    /// </summary>
    public async Task<LegacyTenantNormalizationInspectionResult> InspectAsync(CancellationToken cancellationToken = default)
    {
        if (!_hostEnvironment.IsDevelopment())
        {
            return new LegacyTenantNormalizationInspectionResult(
                TenantId: null,
                EnvironmentAllowsExecution: false,
                TenantResolved: false,
                Products: 0,
                Categories: 0,
                Users: 0,
                Orders: 0,
                OrderItems: 0,
                Carts: 0,
                CartItems: 0);
        }

        if (!_tenantContextAccessor.IsAvailable || string.IsNullOrWhiteSpace(_tenantContextAccessor.TenantId))
        {
            return new LegacyTenantNormalizationInspectionResult(
                TenantId: null,
                EnvironmentAllowsExecution: true,
                TenantResolved: false,
                Products: 0,
                Categories: 0,
                Users: 0,
                Orders: 0,
                OrderItems: 0,
                Carts: 0,
                CartItems: 0);
        }

        string tenantId = _tenantContextAccessor.TenantId.Trim();
        LegacyTenantRowCounts legacyCounts = await CountLegacyRowsAsync(cancellationToken).ConfigureAwait(false);

        return new LegacyTenantNormalizationInspectionResult(
            tenantId,
            EnvironmentAllowsExecution: true,
            TenantResolved: true,
            legacyCounts.Products,
            legacyCounts.Categories,
            legacyCounts.Users,
            legacyCounts.Orders,
            legacyCounts.OrderItems,
            legacyCounts.Carts,
            legacyCounts.CartItems);
    }

    /// <summary>
    /// Ejecuta de forma explícita la normalización de filas legacy sin tenant para entornos de desarrollo.
    /// </summary>
    public async Task NormalizeAsync(CancellationToken cancellationToken = default)
    {
        LegacyTenantNormalizationInspectionResult inspectionResult = await InspectAsync(cancellationToken).ConfigureAwait(false);
        if (!inspectionResult.EnvironmentAllowsExecution)
        {
            _logger.LogDebug("La normalización de datos legacy por tenant se omite fuera de Development.");
            return;
        }

        if (!inspectionResult.TenantResolved || string.IsNullOrWhiteSpace(inspectionResult.TenantId))
        {
            _logger.LogWarning("Se omitió la normalización de datos legacy por tenant porque no se pudo resolver un tenant activo en Development.");
            return;
        }

        string tenantId = inspectionResult.TenantId;
        if (!inspectionResult.HasLegacyRows)
        {
            _logger.LogInformation(
                NoLegacyRowsDetectedEvent,
                "No se detectaron filas legacy sin tenant para normalizar en Development. Tenant activo: '{TenantId}'.",
                tenantId);
            return;
        }

        _logger.LogWarning(
            "Se detectaron filas legacy sin tenant en Development. Se normalizarán hacia el tenant '{TenantId}'. Products: {Products}. Categories: {Categories}. Users: {Users}. Orders: {Orders}. OrderItems: {OrderItems}. Carts: {Carts}. CartItems: {CartItems}.",
            tenantId,
            inspectionResult.Products,
            inspectionResult.Categories,
            inspectionResult.Users,
            inspectionResult.Orders,
            inspectionResult.OrderItems,
            inspectionResult.Carts,
            inspectionResult.CartItems);

        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await DisableCompositeTenantConstraintsAsync(cancellationToken).ConfigureAwait(false);

            int normalizedUsers = await NormalizeUsersAsync(tenantId, cancellationToken).ConfigureAwait(false);
            int normalizedCategories = await NormalizeCategoriesAsync(tenantId, cancellationToken).ConfigureAwait(false);
            int normalizedProducts = await NormalizeProductsAsync(tenantId, cancellationToken).ConfigureAwait(false);
            int normalizedOrders = await NormalizeOrdersAsync(tenantId, cancellationToken).ConfigureAwait(false);
            int normalizedOrderItems = await NormalizeOrderItemsAsync(tenantId, cancellationToken).ConfigureAwait(false);
            int normalizedCarts = await NormalizeCartsAsync(tenantId, cancellationToken).ConfigureAwait(false);
            int normalizedCartItems = await NormalizeCartItemsAsync(tenantId, cancellationToken).ConfigureAwait(false);

            await EnableCompositeTenantConstraintsAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            LegacyTenantRowCounts normalizedCounts = new(
                normalizedProducts,
                normalizedCategories,
                normalizedUsers,
                normalizedOrders,
                normalizedOrderItems,
                normalizedCarts,
                normalizedCartItems);

            _logger.LogWarning(
                LegacyRowsNormalizedEvent,
                "Se normalizaron filas legacy sin tenant en Development para el tenant '{TenantId}'. Total corregido: {TotalRows}. Products: {Products}. Categories: {Categories}. Users: {Users}. Orders: {Orders}. OrderItems: {OrderItems}. Carts: {Carts}. CartItems: {CartItems}.",
                tenantId,
                normalizedCounts.TotalRows,
                normalizedCounts.Products,
                normalizedCounts.Categories,
                normalizedCounts.Users,
                normalizedCounts.Orders,
                normalizedCounts.OrderItems,
                normalizedCounts.Carts,
                normalizedCounts.CartItems);
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogError(exception, "Falló la normalización de datos legacy por tenant en Development.");
            throw;
        }
    }

    private async Task<LegacyTenantRowCounts> CountLegacyRowsAsync(CancellationToken cancellationToken)
    {
        int products = await _dbContext.Products.IgnoreQueryFilters().CountAsync(entity => entity.TenantId == string.Empty, cancellationToken).ConfigureAwait(false);
        int categories = await _dbContext.Categories.IgnoreQueryFilters().CountAsync(entity => entity.TenantId == string.Empty, cancellationToken).ConfigureAwait(false);
        int users = await _dbContext.Users.IgnoreQueryFilters().CountAsync(entity => entity.TenantId == string.Empty, cancellationToken).ConfigureAwait(false);
        int orders = await _dbContext.Orders.IgnoreQueryFilters().CountAsync(entity => entity.TenantId == string.Empty, cancellationToken).ConfigureAwait(false);
        int orderItems = await _dbContext.OrderItems.IgnoreQueryFilters().CountAsync(entity => entity.TenantId == string.Empty, cancellationToken).ConfigureAwait(false);
        int carts = await _dbContext.Carts.IgnoreQueryFilters().CountAsync(entity => entity.TenantId == string.Empty, cancellationToken).ConfigureAwait(false);
        int cartItems = await _dbContext.CartItems.IgnoreQueryFilters().CountAsync(entity => entity.TenantId == string.Empty, cancellationToken).ConfigureAwait(false);

        return new LegacyTenantRowCounts(products, categories, users, orders, orderItems, carts, cartItems);
    }

    private async Task DisableCompositeTenantConstraintsAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE [dbo].[Categories] NOCHECK CONSTRAINT ALL;", cancellationToken).ConfigureAwait(false);
        await _dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE [dbo].[Products] NOCHECK CONSTRAINT ALL;", cancellationToken).ConfigureAwait(false);
        await _dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE [dbo].[Orders] NOCHECK CONSTRAINT ALL;", cancellationToken).ConfigureAwait(false);
        await _dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE [dbo].[OrderItems] NOCHECK CONSTRAINT ALL;", cancellationToken).ConfigureAwait(false);
        await _dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE [dbo].[Carts] NOCHECK CONSTRAINT ALL;", cancellationToken).ConfigureAwait(false);
        await _dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE [dbo].[CartItems] NOCHECK CONSTRAINT ALL;", cancellationToken).ConfigureAwait(false);
    }

    private async Task EnableCompositeTenantConstraintsAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE [dbo].[Categories] WITH CHECK CHECK CONSTRAINT ALL;", cancellationToken).ConfigureAwait(false);
        await _dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE [dbo].[Products] WITH CHECK CHECK CONSTRAINT ALL;", cancellationToken).ConfigureAwait(false);
        await _dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE [dbo].[Orders] WITH CHECK CHECK CONSTRAINT ALL;", cancellationToken).ConfigureAwait(false);
        await _dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE [dbo].[OrderItems] WITH CHECK CHECK CONSTRAINT ALL;", cancellationToken).ConfigureAwait(false);
        await _dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE [dbo].[Carts] WITH CHECK CHECK CONSTRAINT ALL;", cancellationToken).ConfigureAwait(false);
        await _dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE [dbo].[CartItems] WITH CHECK CHECK CONSTRAINT ALL;", cancellationToken).ConfigureAwait(false);
    }

    private Task<int> NormalizeUsersAsync(string tenantId, CancellationToken cancellationToken)
    {
        return _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [dbo].[Users]
            SET [TenantId] = {tenantId}
            WHERE ISNULL(LTRIM(RTRIM([TenantId])), '') = '';
            """, cancellationToken);
    }

    private Task<int> NormalizeCategoriesAsync(string tenantId, CancellationToken cancellationToken)
    {
        return _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [dbo].[Categories]
            SET [TenantId] = {tenantId}
            WHERE ISNULL(LTRIM(RTRIM([TenantId])), '') = '';
            """, cancellationToken);
    }

    private Task<int> NormalizeProductsAsync(string tenantId, CancellationToken cancellationToken)
    {
        return _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [dbo].[Products]
            SET [TenantId] = {tenantId}
            WHERE ISNULL(LTRIM(RTRIM([TenantId])), '') = '';
            """, cancellationToken);
    }

    private Task<int> NormalizeOrdersAsync(string tenantId, CancellationToken cancellationToken)
    {
        return _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [dbo].[Orders]
            SET [TenantId] = {tenantId}
            WHERE ISNULL(LTRIM(RTRIM([TenantId])), '') = '';
            """, cancellationToken);
    }

    private Task<int> NormalizeOrderItemsAsync(string tenantId, CancellationToken cancellationToken)
    {
        return _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [dbo].[OrderItems]
            SET [TenantId] = {tenantId}
            WHERE ISNULL(LTRIM(RTRIM([TenantId])), '') = '';
            """, cancellationToken);
    }

    private Task<int> NormalizeCartsAsync(string tenantId, CancellationToken cancellationToken)
    {
        return _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [dbo].[Carts]
            SET [TenantId] = {tenantId}
            WHERE ISNULL(LTRIM(RTRIM([TenantId])), '') = '';
            """, cancellationToken);
    }

    private Task<int> NormalizeCartItemsAsync(string tenantId, CancellationToken cancellationToken)
    {
        return _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [dbo].[CartItems]
            SET [TenantId] = {tenantId}
            WHERE ISNULL(LTRIM(RTRIM([TenantId])), '') = '';
            """, cancellationToken);
    }

    private sealed record LegacyTenantRowCounts(
        int Products,
        int Categories,
        int Users,
        int Orders,
        int OrderItems,
        int Carts,
        int CartItems)
    {
        public bool HasLegacyRows => Products > 0
            || Categories > 0
            || Users > 0
            || Orders > 0
            || OrderItems > 0
            || Carts > 0
            || CartItems > 0;

        public int TotalRows => Products + Categories + Users + Orders + OrderItems + Carts + CartItems;
    }
}
