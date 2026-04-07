using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Web.Middlewares;

/// <summary>
/// Traduce excepciones no controladas a descriptores HTTP seguros y homogéneos.
/// </summary>
internal static class UnhandledExceptionMapper
{
    public static UnhandledExceptionDescriptor Map(Exception exception, bool requestAborted)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            OperationCanceledException when requestAborted => new UnhandledExceptionDescriptor(
                499,
                LogLevel.Information,
                "La solicitud fue cancelada.",
                "El cliente canceló la solicitud antes de completarse.",
                SuppressResponseBody: true),
            UnauthorizedAccessException => new UnhandledExceptionDescriptor(
                StatusCodes.Status403Forbidden,
                LogLevel.Warning,
                "No tienes permisos suficientes para esta operación.",
                "El recurso solicitado requiere privilegios adicionales."),
            KeyNotFoundException => new UnhandledExceptionDescriptor(
                StatusCodes.Status404NotFound,
                LogLevel.Information,
                "No se encontró el recurso solicitado.",
                "El recurso solicitado no existe o ya no está disponible."),
            TimeoutException => new UnhandledExceptionDescriptor(
                StatusCodes.Status503ServiceUnavailable,
                LogLevel.Warning,
                "La dependencia no respondió a tiempo.",
                "La operación no pudo completarse dentro del tiempo esperado."),
            DomainException domainException => new UnhandledExceptionDescriptor(
                StatusCodes.Status422UnprocessableEntity,
                LogLevel.Warning,
                "Se infringió una regla de negocio.",
                ResolveDomainDetail(domainException)),
            ArgumentException => new UnhandledExceptionDescriptor(
                StatusCodes.Status400BadRequest,
                LogLevel.Warning,
                "La solicitud no es válida.",
                "La solicitud no pudo procesarse correctamente."),
            InvalidOperationException => new UnhandledExceptionDescriptor(
                StatusCodes.Status400BadRequest,
                LogLevel.Warning,
                "La operación no es válida.",
                "La solicitud no pudo procesarse correctamente."),
            _ => new UnhandledExceptionDescriptor(
                StatusCodes.Status500InternalServerError,
                LogLevel.Error,
                "Ocurrió un error interno en el servidor.",
                "Se produjo un error inesperado mientras se procesaba la solicitud.")
        };
    }

    private static string ResolveDomainDetail(DomainException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return string.IsNullOrWhiteSpace(exception.Message)
            ? "La operación incumple una regla del dominio del negocio."
            : exception.Message;
    }
}
