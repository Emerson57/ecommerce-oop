using System.Net;
using System.Text.Json;

namespace PlataformaECommerce.Web.Middlewares
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// Inicializa una nueva instancia del middleware de manejo de excepciones.
        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// Ejecuta el middleware.
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                await ManejarExcepcionAsync(context, ex, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (ArgumentException ex)
            {
                await ManejarExcepcionAsync(context, ex, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await ManejarExcepcionAsync(context, ex, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo una excepción no controlada.");
                await ManejarExcepcionAsync(
                    context,
                    ex,
                    HttpStatusCode.InternalServerError,
                    "Ocurrió un error interno en el servidor.");
            }
        }

        /// Construye y devuelve una respuesta JSON estructurada para la excepción recibida.
        private static async Task ManejarExcepcionAsync(
            HttpContext context,
            Exception exception,
            HttpStatusCode statusCode,
            string mensaje)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var respuesta = new
            {
                mensaje,
                detalleTecnico = exception.GetType().Name,
                codigo = (int)statusCode
            };

            var json = JsonSerializer.Serialize(respuesta);

            await context.Response.WriteAsync(json);
        }
    }
}