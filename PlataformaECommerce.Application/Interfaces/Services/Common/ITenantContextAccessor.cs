using System;

namespace PlataformaECommerce.Application.Interfaces.Services.Common;

/// <summary>
/// Expone el tenant activo del contexto de ejecución actual para aislamiento de datos y trazabilidad SaaS.
/// </summary>
public interface ITenantContextAccessor
{
    /// <summary>
    /// Obtiene el identificador lógico del tenant activo.
    /// </summary>
    string TenantId { get; }

    /// <summary>
    /// Indica si el contexto actual dispone de un tenant resuelto de forma válida.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Inicia un alcance temporal que fuerza un tenant específico dentro del flujo de ejecución actual.
    /// </summary>
    /// <param name="tenantId">Identificador lógico del tenant a forzar.</param>
    /// <returns>Un manejador que restaura el tenant anterior al finalizar el alcance.</returns>
    IDisposable BeginTenantScope(string tenantId)
    {
        throw new NotSupportedException("La implementación actual del contexto de tenant no soporta alcances temporales explícitos.");
    }
}
