namespace PlataformaECommerce.Domain.Events;

/// <summary>
/// Representa la clase base para todos los eventos de dominio del sistema.
/// </summary>
/// <remarks>
/// Un evento de dominio expresa que un hecho relevante del negocio ocurrió
/// dentro del modelo de dominio. Estos eventos permiten desacoplar reacciones
/// posteriores como notificaciones, auditoría, integración con otros módulos,
/// actualización de proyecciones o publicación hacia infraestructura externa.
/// 
/// Todos los eventos de dominio deben heredar de esta clase base para garantizar
/// consistencia en la trazabilidad temporal e identificación del evento.
/// </remarks>
public abstract class DomainEvent
{
    /// <summary>
    /// Inicializa una nueva instancia del evento de dominio.
    /// </summary>
    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OcurrioEnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Identificador único del evento de dominio.
    /// </summary>
    public Guid EventId { get; }

    /// <summary>
    /// Fecha y hora UTC en que ocurrió el evento.
    /// </summary>
    public DateTime OcurrioEnUtc { get; }

    /// <summary>
    /// Devuelve una representación resumida del evento para trazabilidad y depuración.
    /// </summary>
    /// <returns>Cadena representativa del evento.</returns>
    public override string ToString()
    {
        return $"{GetType().Name} | EventId: {EventId} | OcurrioEnUtc: {OcurrioEnUtc:O}";
    }
}