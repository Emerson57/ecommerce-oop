using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using PlataformaECommerce.Web.Middlewares;

namespace PlataformaECommerce.Tests.Web.Middlewares;

[TestFixture]
public class ExceptionHandlingMiddlewareTests
{
    [Test]
    public async Task InvokeAsync_InvalidOperationException_NoExponeDetalleTecnicoEnLaRespuesta()
    {
        ExceptionHandlingMiddleware middleware = new(
            _ => throw new InvalidOperationException("Detalle interno sensible."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);
        DefaultHttpContext httpContext = new();
        httpContext.TraceIdentifier = "trace-admin-001";
        httpContext.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(httpContext);

        httpContext.Response.Body.Position = 0;
        using StreamReader reader = new(httpContext.Response.Body);
        string responseBody = await reader.ReadToEndAsync();

        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        Assert.That(responseBody, Does.Not.Contain("InvalidOperationException"));
        Assert.That(responseBody, Does.Not.Contain("Detalle interno sensible"));
        Assert.That(responseBody, Does.Contain("trace-admin-001"));
    }

    [Test]
    public async Task InvokeAsync_ExceptionNoControlada_RetornaMensajeGenerico()
    {
        ExceptionHandlingMiddleware middleware = new(
            _ => throw new Exception("Fallo interno sensible."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);
        DefaultHttpContext httpContext = new();
        httpContext.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(httpContext);

        httpContext.Response.Body.Position = 0;
        JsonDocument payload = await JsonDocument.ParseAsync(httpContext.Response.Body);

        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        Assert.That(payload.RootElement.GetProperty("mensaje").GetString(), Is.EqualTo("Ocurrió un error interno en el servidor."));
        Assert.That(payload.RootElement.TryGetProperty("detalleTecnico", out _), Is.False);
    }
}
