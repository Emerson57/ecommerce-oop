using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Audit.DTOs;
using PlataformaECommerce.Application.Features.Audit.Queries;

namespace PlataformaECommerce.Application.Interfaces.Services.Audit;

/// <summary>
/// Define el contrato del servicio de aplicación que constituye la frontera pública del módulo <c>Audit</c>.
/// </summary>
/// <remarks>
/// Este contrato expone los casos de uso públicos del módulo de auditoría para capas consumidoras
/// como Razor Pages administrativas, soporte e integraciones internas. Las consultas recibidas se
/// utilizan como modelos de entrada del caso de uso de exploración del rastro auditado.
/// </remarks>
public interface IAuditApplicationService
{
    /// <summary>
    /// Obtiene eventos del rastro de auditoría aplicando los filtros indicados.
    /// </summary>
    /// <param name="query">Consulta de auditoría a ejecutar.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado con la colección proyectada de eventos de auditoría.</returns>
    Task<Result<AuditQueryResultDto>> GetAuditTrailAsync(
        GetAuditTrailQuery query,
        CancellationToken cancellationToken = default);
}
