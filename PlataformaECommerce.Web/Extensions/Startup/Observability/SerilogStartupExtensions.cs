using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Agrupa la configuración de Serilog para host y pipeline HTTP.
/// </summary>
public static class SerilogStartupExtensions
{
    /// <summary>
    /// Registra Serilog como proveedor principal de logging del host web.
    /// </summary>
    /// <param name="hostBuilder">Builder del host a configurar.</param>
    /// <param name="configuration">Configuración raíz actual.</param>
    /// <returns>El mismo builder del host para encadenamiento fluido.</returns>
    public static IHostBuilder ConfigureSerilogLogging(this IHostBuilder hostBuilder, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);
        ArgumentNullException.ThrowIfNull(configuration);

        hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "PlataformaECommerce.Web");
        });

        return hostBuilder;
    }

    /// <summary>
    /// Agrega el middleware de logging estructurado de solicitudes HTTP con el enriquecimiento actual de diagnóstico.
    /// </summary>
    /// <param name="app">Aplicación web a configurar.</param>
    /// <returns>La misma aplicación web para encadenamiento fluido.</returns>
    public static WebApplication UseSerilogRequestLoggingDiagnostics(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = static (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("TraceIdentifier", httpContext.TraceIdentifier);
                diagnosticContext.Set("CorrelationId", RequestCorrelationContextResolver.Resolve(httpContext));
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("RemoteIp", httpContext.Connection.RemoteIpAddress?.ToString());
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                diagnosticContext.Set("EndpointName", httpContext.GetEndpoint()?.DisplayName);
            };
        });

        return app;
    }
}
