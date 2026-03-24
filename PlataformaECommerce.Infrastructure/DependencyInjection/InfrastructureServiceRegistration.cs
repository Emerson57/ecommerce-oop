using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Audit;
using PlataformaECommerce.Application.Interfaces.Repositories.Cart;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Infrastructure.Configurations;
using PlataformaECommerce.Infrastructure.Mongo;
using PlataformaECommerce.Infrastructure.Mongo.Repositories;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Infrastructure.Repositories.Cart;
using PlataformaECommerce.Infrastructure.Repositories.Common;
using PlataformaECommerce.Infrastructure.Repositories.Orders;
using PlataformaECommerce.Infrastructure.Repositories.Products;
using PlataformaECommerce.Infrastructure.Repositories.Users;
using PlataformaECommerce.Infrastructure.Services.Auth;
using PlataformaECommerce.Infrastructure.Services.Common;

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
        RegisterPersistence(services, configuration);
        RegisterMongo(services);
        RegisterSecurity(services, configuration, hostEnvironment);

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
            .AddOptions<MongoDbSettings>()
            .Bind(configuration.GetSection(MongoDbSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(settings => HasValidJwtSettings(settings, hostEnvironment), BuildJwtValidationMessage(hostEnvironment))
            .ValidateOnStart();
    }

    /// <summary>
    /// Registra la infraestructura transaccional basada en SQL Server y Entity Framework Core.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración raíz del entorno.</param>
    private static void RegisterPersistence(IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

        services.AddDbContext<ECommerceDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    /// <summary>
    /// Registra la infraestructura documental utilizada para auditoría sobre MongoDB.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    private static void RegisterMongo(IServiceCollection services)
    {
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

        string signingKey = ResolveJwtSigningKey(jwtSettings, hostEnvironment);

        services.PostConfigure<JwtSettings>(options =>
        {
            if (string.IsNullOrWhiteSpace(options.SigningKey) || options.SigningKey.Length < 32)
            {
                options.SigningKey = signingKey;
            }
        });

        byte[] signingKeyBytes = Encoding.UTF8.GetBytes(signingKey);

        services.AddDataProtection();
        services.TryAddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.TryAddSingleton<IPasswordResetTokenService, PasswordResetTokenService>();
        services.TryAddSingleton<ITokenService, JwtTokenService>();
        services.TryAddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.TryAddScoped<IExecutionContextAccessor, ExecutionContextAccessor>();
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
            "La configuración JWT requiere una clave de firma de al menos 32 caracteres. Configure 'Jwt:SigningKey' mediante variables de entorno, User Secrets o un proveedor seguro equivalente.");
    }
}