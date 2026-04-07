using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Services.Products;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Agrupa la configuración de la capa de presentación web y sus opciones asociadas.
/// </summary>
public static class PresentationStartupExtensions
{
    /// <summary>
    /// Registra Razor Pages, controladores, localización y opciones propias de la UI web.
    /// </summary>
    /// <param name="services">Colección de servicios a configurar.</param>
    /// <returns>La misma colección de servicios para encadenamiento fluido.</returns>
    public static IServiceCollection AddConfiguredPresentation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRazorPages(options =>
        {
            options.Conventions.AuthorizeFolder("/Admin", AuthorizationPolicies.AdminOnly);
            options.Conventions.AuthorizeFolder("/Admin/Users", AuthorizationPolicies.SuperUserOnly);
        });

        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    ProblemDetailsFactory problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                    ValidationProblemDetails problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                        context.HttpContext,
                        context.ModelState,
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "La solicitud contiene errores de validación.",
                        detail: "Corrige los campos indicados e inténtalo nuevamente.",
                        instance: context.HttpContext.Request.Path);

                    StartupCompositionHelpers.PopulateProblemDetails(context.HttpContext, problemDetails);

                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

        services
            .AddOptions<ClientExperienceOptions>()
            .BindConfiguration(ClientExperienceOptions.SectionName)
            .ValidateDataAnnotations()
            .Validate(options => StartupCompositionHelpers.IsValidHexColor(options.PrimaryColor), "La configuración comercial requiere un color primario hexadecimal válido.")
            .Validate(options => StartupCompositionHelpers.IsValidHexColor(options.AccentColor), "La configuración comercial requiere un color de acento hexadecimal válido.")
            .Validate(options => StartupCompositionHelpers.IsValidHexColor(options.AdminSidebarStartColor), "La configuración comercial requiere un color inicial válido para el sidebar administrativo.")
            .Validate(options => StartupCompositionHelpers.IsValidHexColor(options.AdminSidebarEndColor), "La configuración comercial requiere un color final válido para el sidebar administrativo.")
            .ValidateOnStart();

        services
            .AddOptions<AdminUsersBackofficeOptions>()
            .BindConfiguration(AdminUsersBackofficeOptions.SectionName);

        services
            .AddOptions<ProductImagesOptions>()
            .BindConfiguration(ProductImagesOptions.SectionName)
            .Validate(options => !string.IsNullOrWhiteSpace(options.UploadsDirectory), "La configuración de imágenes de productos requiere un directorio de almacenamiento válido.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.RequestPath) && options.RequestPath.StartsWith('/'), "La configuración de imágenes de productos requiere una ruta pública válida que comience con '/'.")
            .Validate(options => options.MaxFileSizeInBytes > 0, "La configuración de imágenes de productos requiere un tamaño máximo de archivo mayor que cero.")
            .Validate(options => options.AllowedExtensions.Count > 0, "La configuración de imágenes de productos requiere al menos una extensión permitida.")
            .ValidateOnStart();

        services.AddScoped<IProductImageStorageService, ProductImageStorageService>();

        CultureInfo[] supportedCultures =
        [
            CultureInfo.GetCultureInfo("es-CO"),
            CultureInfo.GetCultureInfo("es"),
            CultureInfo.GetCultureInfo("en-US")
        ];

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("es-CO");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.ApplyCurrentCultureToResponseHeaders = true;
            options.FallBackToParentCultures = true;
            options.FallBackToParentUICultures = true;
        });

        return services;
    }
}
