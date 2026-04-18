using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Audit;
using PlataformaECommerce.Application.Interfaces.Repositories.Categories;
using PlataformaECommerce.Application.Interfaces.Repositories.Cart;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Application.Interfaces.Services.Orders;
using PlataformaECommerce.Application.Interfaces.Services.Users;
using PlataformaECommerce.Infrastructure.Configurations;
using PlataformaECommerce.Infrastructure.Mongo;
using PlataformaECommerce.Infrastructure.Mongo.Repositories;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Infrastructure.Repositories.Cart;
using PlataformaECommerce.Infrastructure.Repositories.Categories;
using PlataformaECommerce.Infrastructure.Repositories.Common;
using PlataformaECommerce.Infrastructure.Repositories.Orders;
using PlataformaECommerce.Infrastructure.Repositories.Products;
using PlataformaECommerce.Infrastructure.Repositories.Users;
using PlataformaECommerce.Infrastructure.Services.Auth;
using PlataformaECommerce.Infrastructure.Services.Common;
using PlataformaECommerce.Infrastructure.Services.Orders;
using PlataformaECommerce.Infrastructure.Services.Users;

namespace PlataformaECommerce.Infrastructure.DependencyInjection;

/// <summary>
/// Proporciona el punto de entrada centralizado para registrar los componentes técnicos
/// de la capa Infrastructure dentro del contenedor de dependencias.
/// </summary>
/// <remarks>
/// Esta clase consolida la configuración de persistencia transaccional sobre SQL Server,
/// auditoría documental sobre MongoDB y los adaptadores de seguridad requeridos por la capa
/// Application, manteniendo una composición clara, consistente y profesional.
/// </remarks>
public static class InfrastructureServiceRegistration
{
    private const int DevelopmentJwtSigningKeyLengthInBytes = 48;

    /// <summary>
    /// Registra los servicios de infraestructura requeridos por la solución,
    /// incluyendo persistencia SQL Server, auditoría MongoDB y adaptadores
    /// de seguridad basados en JWT y contexto HTTP.
    /// </summary>
    /// <param name="services">Colección de servicios de la aplicación.</param>
    /// <param name="configuration">Configuración raíz del entorno.</param>
    /// <param name="hostEnvironment">Entorno de ejecución actual.</param>
    /// <returns>La colección de servicios para encadenar registro.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        RegisterOptions(services, configuration, hostEnvironment);
        RegisterPersistence(services, configuration, hostEnvironment);
        RegisterMongo(services, configuration, hostEnvironment);
        RegisterSecurity(services, configuration, hostEnvironment);
        RegisterPayments(services, configuration);

        return services;
    }

    /// <summary>
    /// Registra y valida las opciones tipadas requeridas por la infraestructura.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración raíz del entorno.</param>
    /// <param name="hostEnvironment">Entorno de ejecución actual.</param>
    private static void RegisterOptions(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        services
            .AddOptions<SaaSPlatformOptions>()
            .Bind(configuration.GetSection(SaaSPlatformOptions.SectionName))
            .Validate(options => HasValidSaaSSettings(options), "La configuración SaaS contiene tenants, planes o features inválidos para aislamiento de datos y operación comercial.")
            .ValidateOnStart();

        services
            .AddOptions<MongoDbSettings>()
            .Bind(configuration.GetSection(MongoDbSettings.SectionName))
            .Validate(settings => HasValidMongoDbSettings(settings, hostEnvironment), BuildMongoValidationMessage(hostEnvironment))
            .ValidateOnStart();

        services
            .AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .Validate(settings => HasValidJwtSettings(settings, hostEnvironment), BuildJwtValidationMessage(hostEnvironment))
            .ValidateOnStart();

        services
            .AddOptions<DataProtectionKeyManagementSettings>()
            .Bind(configuration.GetSection(DataProtectionKeyManagementSettings.SectionName))
            .ValidateDataAnnotations()
            .Validate(settings => HasValidDataProtectionSettings(settings), "La configuración de Data Protection requiere un ApplicationName compartido y una vida útil válida para despliegues multi-instancia.")
            .ValidateOnStart();

        services
            .AddOptions<SmtpEmailSettings>()
            .Bind(configuration.GetSection(SmtpEmailSettings.SectionName))
            .Validate(settings => HasValidSmtpSettings(settings), "La configuración SMTP contiene valores inválidos para la entrega de notificaciones por correo.")
            .ValidateOnStart();

        services
            .AddOptions<WompiPaymentGatewaySettings>()
            .Bind(configuration.GetSection(WompiPaymentGatewaySettings.SectionName))
            .Validate(settings => HasValidWompiSettings(settings), "La configuración de pagos Wompi contiene valores inválidos para el entorno actual.")
            .ValidateOnStart();
    }

    /// <summary>
    /// Registra la infraestructura transaccional basada en SQL Server y Entity Framework Core.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración raíz del entorno.</param>
    private static void RegisterPersistence(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException($"No se encontró la cadena de conexión 'DefaultConnection'. {BuildSecretSourceGuidance(hostEnvironment)}");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"La cadena de conexión 'DefaultConnection' no puede estar vacía. {BuildSecretSourceGuidance(hostEnvironment)}");
        }

        services.AddDbContext<ECommerceDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    /// <summary>
    /// Registra la infraestructura documental utilizada para auditoría sobre MongoDB.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    private static void RegisterMongo(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        MongoDbSettings mongoSettings = configuration
            .GetSection(MongoDbSettings.SectionName)
            .Get<MongoDbSettings>()
            ?? throw new InvalidOperationException("No se encontró la configuración de MongoDB requerida por la solución.");

        if (!mongoSettings.Enabled)
        {
            if (!hostEnvironment.IsDevelopment())
            {
                throw new InvalidOperationException("La auditoría MongoDB solo puede deshabilitarse en Development. En entornos no locales debe permanecer habilitada.");
            }

            services.TryAddSingleton<IAuditRepository, NullAuditRepository>();
            return;
        }

        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            MongoDbSettings settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        services.AddSingleton<IMongoDatabase>(serviceProvider =>
        {
            IMongoClient client = serviceProvider.GetRequiredService<IMongoClient>();
            MongoDbSettings settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return client.GetDatabase(settings.DatabaseName);
        });

        services.AddScoped<IAuditRepository, MongoAuditRepository>();
    }

    /// <summary>
    /// Registra los servicios técnicos requeridos para autenticación, tokenización
    /// y resolución del usuario actual dentro del contexto HTTP.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración raíz del entorno.</param>
    /// <param name="hostEnvironment">Entorno de ejecución actual.</param>
    private static void RegisterSecurity(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        JwtSettings jwtSettings = configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>()
            ?? throw new InvalidOperationException("No se encontró la configuración JWT requerida por la solución.");

        DataProtectionKeyManagementSettings dataProtectionSettings = configuration
            .GetSection(DataProtectionKeyManagementSettings.SectionName)
            .Get<DataProtectionKeyManagementSettings>()
            ?? throw new InvalidOperationException("No se encontró la configuración de Data Protection requerida por la solución.");

        string signingKey = ResolveJwtSigningKey(jwtSettings, hostEnvironment);

        services.PostConfigure<JwtSettings>(options =>
        {
            if (string.IsNullOrWhiteSpace(options.SigningKey) || options.SigningKey.Length < 32)
            {
                options.SigningKey = signingKey;
            }
        });

        byte[] signingKeyBytes = Encoding.UTF8.GetBytes(signingKey);

        services
            .AddDataProtection()
            .SetApplicationName(dataProtectionSettings.ApplicationName.Trim())
            .SetDefaultKeyLifetime(TimeSpan.FromDays(dataProtectionSettings.KeyLifetimeDays))
            .PersistKeysToDbContext<ECommerceDbContext>();
        services.TryAddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.TryAddSingleton<IPasswordResetTokenService, PasswordResetTokenService>();
        services.TryAddSingleton<IEmailConfirmationTokenService, EmailConfirmationTokenService>();
        services.TryAddScoped<ITokenService, JwtTokenService>();
        services.TryAddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.TryAddScoped<IEmailNotificationService, SmtpEmailNotificationService>();
        services.TryAddScoped<IExecutionContextAccessor, ExecutionContextAccessor>();
        services.TryAddScoped<ITenantContextAccessor, TenantContextAccessor>();
        services.TryAddScoped<ITenantCatalogService, TenantCatalogService>();
        services.TryAddScoped<ITenantCatalogProvisioningService, TenantCatalogProvisioningService>();
        services.TryAddScoped<ICurrentUserService, CurrentUserService>();
        services.TryAddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();

        services
            .AddAuthentication()
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = jwtSettings.RequireHttpsMetadata;
                options.SaveToken = true;
                options.MapInboundClaims = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });
    }

    private static bool HasValidDataProtectionSettings(DataProtectionKeyManagementSettings settings)
    {
        if (settings is null)
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(settings.ApplicationName)
            && settings.KeyLifetimeDays is >= 7 and <= 365;
    }

    private static bool HasValidMongoDbSettings(MongoDbSettings settings, IHostEnvironment hostEnvironment)
    {
        if (settings is null)
        {
            return false;
        }

        if (!settings.Enabled)
        {
            return hostEnvironment.IsDevelopment();
        }

        return !string.IsNullOrWhiteSpace(settings.ConnectionString)
            && !string.IsNullOrWhiteSpace(settings.DatabaseName)
            && !string.IsNullOrWhiteSpace(settings.AuditCollectionName);
    }

    private static bool HasValidSaaSSettings(SaaSPlatformOptions settings)
    {
        if (settings is null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(settings.DataIsolationMode))
        {
            return false;
        }

        IReadOnlyCollection<string> featureIds = settings.Features
            .Select(feature => feature.FeatureId?.Trim())
            .Where(featureId => !string.IsNullOrWhiteSpace(featureId))
            .Cast<string>()
            .ToArray();

        if (featureIds.Count != settings.Features.Count || featureIds.Distinct(StringComparer.OrdinalIgnoreCase).Count() != featureIds.Count)
        {
            return false;
        }

        IReadOnlyCollection<string> planIds = settings.Plans
            .Select(plan => plan.PlanId?.Trim())
            .Where(planId => !string.IsNullOrWhiteSpace(planId))
            .Cast<string>()
            .ToArray();

        if (planIds.Count != settings.Plans.Count || planIds.Distinct(StringComparer.OrdinalIgnoreCase).Count() != planIds.Count)
        {
            return false;
        }

        IReadOnlyCollection<string> tenantIds = settings.Tenants
            .Select(tenant => tenant.TenantId?.Trim())
            .Where(tenantId => !string.IsNullOrWhiteSpace(tenantId))
            .Cast<string>()
            .ToArray();

        if (tenantIds.Count != settings.Tenants.Count || tenantIds.Count == 0 || tenantIds.Distinct(StringComparer.OrdinalIgnoreCase).Count() != tenantIds.Count)
        {
            return false;
        }

        if (settings.Tenants.All(tenant => !tenant.Enabled))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(settings.ActiveTenantId)
            && !tenantIds.Contains(settings.ActiveTenantId.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        HashSet<string> featureCatalog = featureIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        HashSet<string> planCatalog = planIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

        bool hasValidPlans = settings.Plans.All(plan =>
            !string.IsNullOrWhiteSpace(plan.DisplayName)
            && plan.MonthlyPrice >= 0
            && plan.IncludedAdministrators >= 0
            && plan.IncludedProducts >= 0
            && plan.IncludedFeatureIds
                .Where(featureId => !string.IsNullOrWhiteSpace(featureId))
                .All(featureCatalog.Contains));

        if (!hasValidPlans)
        {
            return false;
        }

        return settings.Tenants.All(tenant =>
            !string.IsNullOrWhiteSpace(tenant.DisplayName)
            && (string.IsNullOrWhiteSpace(tenant.PlanId) || planCatalog.Contains(tenant.PlanId.Trim()))
            && tenant.EnabledFeatureIds
                .Where(featureId => !string.IsNullOrWhiteSpace(featureId))
                .All(featureCatalog.Contains));
    }

    private static string BuildMongoValidationMessage(IHostEnvironment hostEnvironment)
    {
        return hostEnvironment.IsDevelopment()
            ? "La configuración MongoDB requiere `DatabaseName` y `AuditCollectionName` siempre. Si `MongoDb:Enabled` es true, también requiere `ConnectionString`. En Development puede deshabilitarse estableciendo `MongoDb:Enabled=false`."
            : "La configuración MongoDB requiere `Enabled=true`, `ConnectionString`, `DatabaseName` y `AuditCollectionName` válidos en entornos no locales.";
    }

    private static bool HasValidJwtSettings(JwtSettings settings, IHostEnvironment hostEnvironment)
    {
        if (settings is null)
        {
            return false;
        }

        bool hasValidCoreSettings =
            !string.IsNullOrWhiteSpace(settings.Issuer)
            && !string.IsNullOrWhiteSpace(settings.Audience)
            && settings.AccessTokenExpirationMinutes is >= 1 and <= 1440
            && settings.RefreshTokenExpirationDays is >= 1 and <= 90;

        if (!hasValidCoreSettings)
        {
            return false;
        }

        return hostEnvironment.IsDevelopment()
            || (!string.IsNullOrWhiteSpace(settings.SigningKey) && settings.SigningKey.Length >= 32);
    }

    private static string BuildJwtValidationMessage(IHostEnvironment hostEnvironment)
    {
        return hostEnvironment.IsDevelopment()
            ? "La configuración JWT requiere emisor, audiencia y expiraciones válidas. En Development la clave de firma puede omitirse porque se genera temporalmente en memoria."
            : "La configuración JWT requiere emisor, audiencia, expiraciones válidas y una clave de firma de al menos 32 caracteres.";
    }

    private static string ResolveJwtSigningKey(JwtSettings jwtSettings, IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(jwtSettings);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        if (!string.IsNullOrWhiteSpace(jwtSettings.SigningKey) && jwtSettings.SigningKey.Length >= 32)
        {
            return jwtSettings.SigningKey;
        }

        if (hostEnvironment.IsDevelopment())
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(DevelopmentJwtSigningKeyLengthInBytes));
        }

        throw new InvalidOperationException(
            $"La configuración JWT requiere una clave de firma de al menos 32 caracteres. {BuildSecretSourceGuidance(hostEnvironment, "Configure 'Jwt:SigningKey'")}");
    }

    private static string BuildSecretSourceGuidance(IHostEnvironment hostEnvironment, string? prefix = null)
    {
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        string guidance = hostEnvironment.IsDevelopment()
            ? "Configure el valor mediante User Secrets, variables de entorno o un archivo local no versionado como 'appsettings.Development.local.json'."
            : "Configure el valor mediante variables de entorno o un proveedor seguro equivalente del entorno.";

        return string.IsNullOrWhiteSpace(prefix)
            ? guidance
            : $"{prefix} mediante User Secrets, variables de entorno{(hostEnvironment.IsDevelopment() ? " o un archivo local no versionado como 'appsettings.Development.local.json'" : " o un proveedor seguro equivalente del entorno")}.";
    }

    private static void RegisterPayments(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<IPaymentGateway, WompiPaymentGateway>((serviceProvider, httpClient) =>
        {
            WompiPaymentGatewaySettings settings = serviceProvider.GetRequiredService<IOptions<WompiPaymentGatewaySettings>>().Value;
            httpClient.BaseAddress = new Uri(settings.TransactionsApiBaseUrl, UriKind.Absolute);
            httpClient.Timeout = TimeSpan.FromSeconds(15);
        });
    }

    private static bool HasValidWompiSettings(WompiPaymentGatewaySettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (!settings.Enabled)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(settings.ProviderName)
            && Uri.TryCreate(settings.CheckoutBaseUrl, UriKind.Absolute, out Uri? checkoutUri)
            && checkoutUri.Scheme == Uri.UriSchemeHttps
            && Uri.TryCreate(settings.TransactionsApiBaseUrl, UriKind.Absolute, out Uri? transactionsUri)
            && transactionsUri.Scheme == Uri.UriSchemeHttps
            && !string.IsNullOrWhiteSpace(settings.PublicKey)
            && !string.IsNullOrWhiteSpace(settings.IntegritySecret);
    }

    private static bool HasValidSmtpSettings(SmtpEmailSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (!settings.Enabled)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(settings.Host)
            && settings.Port is >= 1 and <= 65535
            && !string.IsNullOrWhiteSpace(settings.FromAddress);
    }

    private sealed class NullAuditRepository : IAuditRepository
    {
        private static int _warningLogged;
        private readonly ILogger<NullAuditRepository> _logger;

        public NullAuditRepository(ILogger<NullAuditRepository> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        Task IAuditRepository.RegisterEventAsync(AuditEntry entry, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(entry);
            LogDisabledAuditWarning();
            return Task.CompletedTask;
        }

        Task<IReadOnlyCollection<AuditEntry>> IAuditRepository.GetHistoryAsync(Guid aggregateId, string aggregateType, CancellationToken cancellationToken)
        {
            if (aggregateId == Guid.Empty)
            {
                throw new ArgumentException("El identificador del agregado auditado es obligatorio.", nameof(aggregateId));
            }

            if (string.IsNullOrWhiteSpace(aggregateType))
            {
                throw new ArgumentException("El tipo de agregado auditado es obligatorio.", nameof(aggregateType));
            }

            LogDisabledAuditWarning();
            return Task.FromResult<IReadOnlyCollection<AuditEntry>>(Array.Empty<AuditEntry>());
        }

        Task<AuditSearchResult> IAuditRepository.SearchAsync(AuditSearchFilter filter, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(filter);
            LogDisabledAuditWarning();

            return Task.FromResult(new AuditSearchResult
            {
                Items = Array.Empty<AuditEntry>(),
                TotalCount = 0,
                PageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber,
                PageSize = filter.PageSize <= 0 ? 25 : filter.PageSize
            });
        }

        private void LogDisabledAuditWarning()
        {
            if (Interlocked.Exchange(ref _warningLogged, 1) == 1)
            {
                return;
            }

            _logger.LogWarning("La auditoría MongoDB está deshabilitada en Development. Los eventos de auditoría no se persistirán hasta configurar `MongoDb:ConnectionString` y habilitar nuevamente el proveedor.");
        }
    }
}