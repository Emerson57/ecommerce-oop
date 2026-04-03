using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Web.Middlewares
{
    /// <summary>
    /// Centraliza la transformación de excepciones no manejadas en respuestas HTTP seguras y trazables.
    /// </summary>
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IProblemDetailsService _problemDetailsService;

        /// Inicializa una nueva instancia del middleware de manejo de excepciones.
        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IProblemDetailsService problemDetailsService)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _problemDetailsService = problemDetailsService ?? throw new ArgumentNullException(nameof(problemDetailsService));
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
                (int statusCode, LogLevel logLevel, string title, string detail) = MapException(exception, context.RequestAborted.IsCancellationRequested);

                _logger.Log(logLevel, exception,
                    "Se produjo una excepción durante el procesamiento de la solicitud. StatusCode: {StatusCode}. TraceId: {TraceId}. Path: {Path}",
                    statusCode,
                    context.TraceIdentifier,
                    context.Request.Path);

                if (statusCode == 499 && context.RequestAborted.IsCancellationRequested)
                {
                    context.Response.Clear();
                    context.Response.StatusCode = statusCode;
                    return;
                }

                await WriteProblemDetailsAsync(context, statusCode, title, detail, exception);
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

        private static (int StatusCode, LogLevel LogLevel, string Title, string Detail) MapException(Exception exception, bool requestAborted)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return exception switch
            {
                OperationCanceledException when requestAborted => (499, LogLevel.Information, "La solicitud fue cancelada.", "El cliente canceló la solicitud antes de completarse."),
                UnauthorizedAccessException => (StatusCodes.Status403Forbidden, LogLevel.Warning, "No tienes permisos suficientes para esta operación.", "El recurso solicitado requiere privilegios adicionales."),
                KeyNotFoundException => (StatusCodes.Status404NotFound, LogLevel.Information, "No se encontró el recurso solicitado.", "El recurso solicitado no existe o ya no está disponible."),
                TimeoutException => (StatusCodes.Status503ServiceUnavailable, LogLevel.Warning, "La dependencia no respondió a tiempo.", "La operación no pudo completarse dentro del tiempo esperado."),
                DomainException domainException => (StatusCodes.Status422UnprocessableEntity, LogLevel.Warning, "Se infringió una regla de negocio.", ResolveDomainDetail(domainException)),
                ArgumentException => (StatusCodes.Status400BadRequest, LogLevel.Warning, "La solicitud no es válida.", "La solicitud no pudo procesarse correctamente."),
                InvalidOperationException => (StatusCodes.Status400BadRequest, LogLevel.Warning, "La operación no es válida.", "La solicitud no pudo procesarse correctamente."),
                _ => (StatusCodes.Status500InternalServerError, LogLevel.Error, "Ocurrió un error interno en el servidor.", "Se produjo un error inesperado mientras se procesaba la solicitud.")
            };
        }

        private static string ResolveDomainDetail(DomainException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return string.IsNullOrWhiteSpace(exception.Message)
                ? "La operación incumple una regla del dominio del negocio."
                : exception.Message;
        }

        /// Construye una respuesta RFC 7807 consistente para la excepción recibida.
        private async Task WriteProblemDetailsAsync(
            HttpContext context,
            int statusCode,
            string title,
            string detail,
            Exception exception)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(title);
            ArgumentNullException.ThrowIfNull(detail);
            ArgumentNullException.ThrowIfNull(exception);

            context.Response.Clear();
            context.Response.StatusCode = statusCode;

            ProblemDetails problemDetails = new()
            {
                Title = title,
                Detail = detail,
                Status = statusCode,
                Type = $"https://httpstatuses.io/{statusCode}",
                Instance = context.Request.Path
            };

            string correlationId = context.Items.TryGetValue(RequestCorrelationMiddleware.CorrelationIdItemKey, out object? correlationIdValue)
                ? Convert.ToString(correlationIdValue, System.Globalization.CultureInfo.InvariantCulture) ?? context.TraceIdentifier
                : context.TraceIdentifier;

            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
            problemDetails.Extensions["correlationId"] = correlationId;
            problemDetails.Extensions["timestampUtc"] = DateTime.UtcNow;

            bool written = await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = problemDetails,
                Exception = exception
            });

            if (!written)
            {
                throw new InvalidOperationException("No fue posible serializar la respuesta Problem Details para la excepción actual.");
            }
        }
    }
}