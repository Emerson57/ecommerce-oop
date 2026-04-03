using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PlataformaECommerce.Domain.Exceptions;
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
            NullLogger<ExceptionHandlingMiddleware>.Instance,
            CreateProblemDetailsService());
        DefaultHttpContext httpContext = new();
        httpContext.TraceIdentifier = "trace-admin-001";
        httpContext.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(httpContext);

        httpContext.Response.Body.Position = 0;
        using StreamReader reader = new(httpContext.Response.Body);
        string responseBody = await reader.ReadToEndAsync();
        JsonDocument payload = JsonDocument.Parse(responseBody);

        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        Assert.That(responseBody, Does.Not.Contain("InvalidOperationException"));
        Assert.That(responseBody, Does.Not.Contain("Detalle interno sensible"));
        Assert.That(payload.RootElement.GetProperty("traceId").GetString(), Is.EqualTo("trace-admin-001"));
        Assert.That(payload.RootElement.GetProperty("title").GetString(), Is.EqualTo("La operación no es válida."));
    }

    [Test]
    public async Task InvokeAsync_ExceptionNoControlada_RetornaMensajeGenerico()
    {
        ExceptionHandlingMiddleware middleware = new(
            _ => throw new Exception("Fallo interno sensible."),
            NullLogger<ExceptionHandlingMiddleware>.Instance,
            CreateProblemDetailsService());
        DefaultHttpContext httpContext = new();
        httpContext.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(httpContext);

        httpContext.Response.Body.Position = 0;
        JsonDocument payload = await JsonDocument.ParseAsync(httpContext.Response.Body);

        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        Assert.That(payload.RootElement.GetProperty("title").GetString(), Is.EqualTo("Ocurrió un error interno en el servidor."));
        Assert.That(payload.RootElement.TryGetProperty("detalleTecnico", out _), Is.False);
    }

    [Test]
    public async Task InvokeAsync_CancelacionDeCliente_RetornaEstadoControladoSinExponerContenido()
    {
        ExceptionHandlingMiddleware middleware = new(
            _ => throw new OperationCanceledException("Cancelada por cliente."),
            NullLogger<ExceptionHandlingMiddleware>.Instance,
            CreateProblemDetailsService());
        DefaultHttpContext httpContext = new();
        httpContext.TraceIdentifier = "corr-cancel-001";
        httpContext.Items[RequestCorrelationMiddleware.CorrelationIdItemKey] = "corr-cancel-001";
        httpContext.RequestAborted = new CancellationToken(canceled: true);
        httpContext.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(httpContext);

        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(499));
        Assert.That(httpContext.Response.Body.Length, Is.EqualTo(0));
    }

    [Test]
    public async Task InvokeAsync_DomainException_RetornaUnprocessableEntityConMensajeDeNegocio()
    {
        ExceptionHandlingMiddleware middleware = new(
            _ => throw new DomainException("No hay inventario suficiente para completar la operación."),
            NullLogger<ExceptionHandlingMiddleware>.Instance,
            CreateProblemDetailsService());
        DefaultHttpContext httpContext = new();
        httpContext.TraceIdentifier = "domain-trace-001";
        httpContext.Items[RequestCorrelationMiddleware.CorrelationIdItemKey] = "corr-domain-001";
        httpContext.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(httpContext);

        httpContext.Response.Body.Position = 0;
        JsonDocument payload = await JsonDocument.ParseAsync(httpContext.Response.Body);

        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status422UnprocessableEntity));
        Assert.That(payload.RootElement.GetProperty("title").GetString(), Is.EqualTo("Se infringió una regla de negocio."));
        Assert.That(payload.RootElement.GetProperty("detail").GetString(), Does.Contain("inventario suficiente"));
        Assert.That(payload.RootElement.GetProperty("correlationId").GetString(), Is.EqualTo("corr-domain-001"));
    }

    private static IProblemDetailsService CreateProblemDetailsService()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddProblemDetails();
        return services.BuildServiceProvider().GetRequiredService<IProblemDetailsService>();
    }
}
