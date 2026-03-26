using Microsoft.AspNetCore.Http;
using PlataformaECommerce.Infrastructure.Services.Common;

namespace PlataformaECommerce.Tests.Infrastructure.Security;

[TestFixture]
public class ExecutionContextAccessorTests
{
    [Test]
    public void CorrelationId_HttpContextDisponible_RetornaTraceIdentifierActual()
    {
        DefaultHttpContext httpContext = new()
        {
            TraceIdentifier = "trace-123"
        };

        HttpContextAccessor accessor = new() { HttpContext = httpContext };
        ExecutionContextAccessor service = new(accessor);

        Assert.That(service.CorrelationId, Is.EqualTo("trace-123"));
    }
}
