namespace PlataformaECommerce.Web.Middlewares;

/// <summary>
/// Describe el contrato interno que representa una excepción HTTP ya clasificada para la capa web.
/// </summary>
internal sealed record UnhandledExceptionDescriptor(
    int StatusCode,
    LogLevel LogLevel,
    string Title,
    string Detail,
    bool SuppressResponseBody = false);
