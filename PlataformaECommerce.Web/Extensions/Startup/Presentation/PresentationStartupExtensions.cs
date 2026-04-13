using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using PlataformaECommerce.Web.Authorization;

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
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRazorPages(options =>
        {
            options.Conventions.ConfigureFilter(new AutoValidateAntiforgeryTokenAttribute());
            options.Conventions.AuthorizeFolder("/Admin", AuthorizationPolicies.AdminOnly);
            options.Conventions.AuthorizeFolder("/Admin/Users", AuthorizationPolicies.SuperUserOnly);
        });

        services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            })
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

                    ObservabilityProblemDetailsEnricher.Enrich(context.HttpContext, problemDetails);

                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

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
