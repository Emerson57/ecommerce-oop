using PlataformaECommerce.Web.Extensions.Startup;

namespace PlataformaECommerce.Web.Middlewares;

/// <summary>
/// Centraliza la transformación de excepciones no manejadas en respuestas HTTP seguras y trazables.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// Inicializa una nueva instancia del middleware de manejo de excepciones.
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// Ejecuta el middleware.
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            UnhandledExceptionDescriptor descriptor = UnhandledExceptionMapper.Map(exception, context.RequestAborted.IsCancellationRequested);

            _logger.Log(descriptor.LogLevel, exception,
                "Se produjo una excepción durante el procesamiento de la solicitud. StatusCode: {StatusCode}. TraceId: {TraceId}. CorrelationId: {CorrelationId}. Path: {Path}",
                descriptor.StatusCode,
                context.TraceIdentifier,
                RequestCorrelationContextResolver.Resolve(context),
                context.Request.Path);

            if (descriptor.SuppressResponseBody)
            {
                context.Response.Clear();
                context.Response.StatusCode = descriptor.StatusCode;
                return;
            }

            await ProblemDetailsExceptionResponseWriter.WriteAsync(context, descriptor, exception, context.RequestAborted);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception,
                "La respuesta ya había comenzado cuando ocurrió una excepción. TraceId: {TraceId}. Path: {Path}",
                context.TraceIdentifier,
                context.Request.Path);

            throw;
        }
    }
}
