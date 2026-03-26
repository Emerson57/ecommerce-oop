namespace PlataformaECommerce.Application.Interfaces.Services.Common;

/// <summary>
/// Define el contrato del servicio responsable de exponer información contextual
/// de ejecución útil para trazabilidad, auditoría y correlación operativa.
/// </summary>
/// <remarks>
/// Este contrato permite que la capa Application obtenga información transversal
/// como un identificador de correlación sin depender directamente de <c>HttpContext</c>,
/// middlewares concretos ni detalles del transporte subyacente.
/// </remarks>
public interface IExecutionContextAccessor
{
    /// <summary>
    /// Obtiene el identificador de correlación asociado al flujo de ejecución actual.
    /// </summary>
    /// <remarks>
    /// Debe retornar <see langword="null"/> cuando el contexto actual no disponga
    /// de un identificador de correlación observable.
    /// </remarks>
    string? CorrelationId { get; }
}
