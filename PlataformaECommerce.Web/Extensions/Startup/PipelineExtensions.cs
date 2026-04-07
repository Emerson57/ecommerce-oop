using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Middlewares;
using PlataformaECommerce.Web.OpenApi;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Centraliza la construcción del pipeline HTTP manteniendo el mismo orden de middleware del arranque original.
/// </summary>
public static class PipelineExtensions
{
    /// <summary>
    /// Configura el pipeline HTTP completo de la aplicación web.
    /// </summary>
    /// <param name="app">Aplicación web a configurar.</param>
    /// <returns>La misma aplicación web para encadenamiento fluido.</returns>
    public static WebApplication UseConfiguredPipeline(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint($"/swagger/{SwaggerGroups.Public}/swagger.json", "API Pública v1");
                options.SwaggerEndpoint($"/swagger/{SwaggerGroups.Admin}/swagger.json", "API Administrativa v1");
                options.DocumentTitle = "PlataformaECommerce Swagger";
            });
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseForwardedHeaders();
        app.UseHttpsRedirection();
        app.UseMiddleware<RequestCorrelationMiddleware>();
        app.UseConfiguredSerilogRequestLogging();

        RequestLocalizationOptions requestLocalizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
        app.UseRequestLocalization(requestLocalizationOptions);
        app.UseMiddleware<SecurityHeadersMiddleware>();
        ProductImagesStaticFileConfigurator.UseConfiguredStaticFiles(app);
        app.UseRouting();
        app.UseRateLimiter();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
