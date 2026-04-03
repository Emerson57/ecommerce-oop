using System.Diagnostics;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;
using Serilog.Context;

namespace PlataformaECommerce.Web.Middlewares;

/// <summary>
/// Garantiza un identificador de correlación estable durante todo el ciclo de vida de la solicitud.
/// </summary>
public sealed class RequestCorrelationMiddleware
{
    /// <summary>
    /// Clave utilizada para almacenar el identificador de correlación en <see cref="HttpContext.Items"/>.
    /// </summary>
    public const string CorrelationIdItemKey = "RequestCorrelationId";

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestCorrelationMiddleware> _logger;
    private readonly RequestCorrelationOptions _options;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="RequestCorrelationMiddleware"/>.
    /// </summary>
    public RequestCorrelationMiddleware(
        RequestDelegate next,
        ILogger<RequestCorrelationMiddleware> logger,
        IOptions<RequestCorrelationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options.Value;
    }

    /// <summary>
    /// Resuelve o genera un identificador de correlación y lo expone al resto del pipeline.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string correlationId = ResolveCorrelationId(context);
        context.TraceIdentifier = correlationId;
        context.Items[CorrelationIdItemKey] = correlationId;

        if (_options.EmitResponseHeader)
        {
            context.Response.Headers[_options.CorrelationHeaderName] = correlationId;
        }

        Activity.Current?.SetTag("correlation.id", correlationId);
        Activity.Current?.SetBaggage("correlation.id", correlationId);

        using IDisposable? correlationScope = LogContext.PushProperty("CorrelationId", correlationId);
        using IDisposable? pathScope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["TraceIdentifier"] = context.TraceIdentifier,
            ["RequestPath"] = context.Request.Path.Value
        });

        await _next(context);
    }

    private string ResolveCorrelationId(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Request.Headers.TryGetValue(_options.CorrelationHeaderName, out Microsoft.Extensions.Primitives.StringValues values))
        {
            string? candidate = values.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(candidate) && candidate.Length <= _options.MaxCorrelationIdLength)
            {
                return candidate.Trim();
            }

            _logger.LogWarning(
                "Se ignoró un identificador de correlación entrante inválido. Header: {HeaderName}. TraceIdentifierOriginal: {TraceIdentifier}",
                _options.CorrelationHeaderName,
                context.TraceIdentifier);
        }

        return Guid.NewGuid().ToString("N");
    }
}
