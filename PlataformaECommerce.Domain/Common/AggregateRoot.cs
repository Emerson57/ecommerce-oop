using PlataformaECommerce.Domain.Events;

namespace PlataformaECommerce.Domain.Common;

/// <summary>
/// Representa la base común para agregados del dominio con identidad, trazabilidad y eventos de dominio.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = new();

    /// <summary>
    /// Identificador único del agregado.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Fecha y hora UTC en que fue creado el agregado.
    /// </summary>
    public DateTime FechaCreacionUtc { get; protected set; }

    /// <summary>
    /// Fecha y hora UTC de la última modificación relevante del agregado.
    /// </summary>
    public DateTime? FechaActualizacionUtc { get; protected set; }

    /// <summary>
    /// Colección de eventos de dominio generados por el agregado.
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Inicializa la identidad y trazabilidad base del agregado.
    /// </summary>
    protected void InicializarAggregateRoot()
    {
        Id = Guid.NewGuid();
        FechaCreacionUtc = DateTime.UtcNow;
        FechaActualizacionUtc = null;
    }

    /// <summary>
    /// Registra la fecha de modificación del agregado en tiempo UTC.
    /// </summary>
    protected void MarcarActualizacion()
    {
        FechaActualizacionUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Registra un nuevo evento de dominio dentro del agregado.
    /// </summary>
    /// <param name="domainEvent">Evento de dominio a registrar.</param>
    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Elimina todos los eventos de dominio registrados por el agregado.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
