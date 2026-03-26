using Microsoft.AspNetCore.Http;
using PlataformaECommerce.Application.Interfaces.Services.Common;

namespace PlataformaECommerce.Infrastructure.Services.Common;

/// <summary>
/// Implementa el acceso desacoplado a información contextual de ejecución a partir
/// del entorno HTTP administrado por ASP.NET Core.
/// </summary>
/// <remarks>
/// Este adaptador permite exponer a la capa Application un identificador de correlación
/// estable para auditoría y trazabilidad sin introducir dependencia directa con
/// <see cref="HttpContext"/> ni con mecanismos específicos del pipeline web.
/// </remarks>
public sealed class ExecutionContextAccessor : IExecutionContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ExecutionContextAccessor"/>.
    /// </summary>
    /// <param name="httpContextAccessor">Accesor al contexto HTTP actual.</param>
    public ExecutionContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public string? CorrelationId => _httpContextAccessor.HttpContext?.TraceIdentifier;
}
