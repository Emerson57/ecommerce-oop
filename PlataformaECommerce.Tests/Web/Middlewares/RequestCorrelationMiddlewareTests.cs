using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Middlewares;

namespace PlataformaECommerce.Tests.Web.Middlewares;

[TestFixture]
public class RequestCorrelationMiddlewareTests
{
    [Test]
    public async Task InvokeAsync_HeaderValido_ReutilizaCorrelationIdEnContextoYRespuesta()
    {
        const string correlationId = "corr-req-0001";
        RequestCorrelationMiddleware middleware = new(
            _ => Task.CompletedTask,
            NullLogger<RequestCorrelationMiddleware>.Instance,
            Options.Create(new RequestCorrelationOptions()));
        DefaultHttpContext httpContext = new();
        httpContext.Request.Headers["X-Correlation-ID"] = correlationId;

        await middleware.InvokeAsync(httpContext);

        Assert.That(httpContext.TraceIdentifier, Is.EqualTo(correlationId));
        Assert.That(httpContext.Response.Headers["X-Correlation-ID"].ToString(), Is.EqualTo(correlationId));
        Assert.That(httpContext.Items[RequestCorrelationMiddleware.CorrelationIdItemKey], Is.EqualTo(correlationId));
    }

    [Test]
    public async Task InvokeAsync_HeaderInvalido_GeneraCorrelationIdSeguro()
    {
        RequestCorrelationMiddleware middleware = new(
            _ => Task.CompletedTask,
            NullLogger<RequestCorrelationMiddleware>.Instance,
            Options.Create(new RequestCorrelationOptions { MaxCorrelationIdLength = 32 }));
        DefaultHttpContext httpContext = new();
        httpContext.Request.Headers["X-Correlation-ID"] = new string('x', 64);

        await middleware.InvokeAsync(httpContext);

        Assert.That(httpContext.TraceIdentifier, Has.Length.EqualTo(32));
        Assert.That(httpContext.TraceIdentifier, Is.Not.EqualTo(new string('x', 64)));
        Assert.That(httpContext.Response.Headers["X-Correlation-ID"].ToString(), Is.EqualTo(httpContext.TraceIdentifier));
    }
}
