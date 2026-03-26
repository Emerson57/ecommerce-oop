using PlataformaECommerce.Domain.Common;
using PlataformaECommerce.Domain.Events;

namespace PlataformaECommerce.Tests.Domain.Common;

[TestFixture]
public class AggregateRootTests
{
    [Test]
    public void InicializarAggregateRoot_AsignaIdentidadYFechaCreacion()
    {
        AggregateRootDePrueba aggregateRoot = new();

        aggregateRoot.Inicializar();

        Assert.That(aggregateRoot.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(aggregateRoot.FechaCreacionUtc, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void AddDomainEvent_ConEventoValido_LoRegistraEnLaColeccion()
    {
        AggregateRootDePrueba aggregateRoot = new();
        DomainEventDePrueba domainEvent = new();

        aggregateRoot.RegistrarEvento(domainEvent);

        Assert.That(aggregateRoot.DomainEvents, Has.Count.EqualTo(1));
    }

    [Test]
    public void ClearDomainEvents_ConEventosPrevios_VaciaLaColeccion()
    {
        AggregateRootDePrueba aggregateRoot = new();
        aggregateRoot.RegistrarEvento(new DomainEventDePrueba());

        aggregateRoot.ClearDomainEvents();

        Assert.That(aggregateRoot.DomainEvents, Is.Empty);
    }

    [Test]
    public void MarcarActualizacion_DelAggregate_AsignaFechaActualizacion()
    {
        AggregateRootDePrueba aggregateRoot = new();
        aggregateRoot.Inicializar();

        aggregateRoot.Actualizar();

        Assert.That(aggregateRoot.FechaActualizacionUtc, Is.Not.Null);
    }

    [Test]
    public void AddDomainEvent_EventoNulo_LanzaArgumentNullException()
    {
        AggregateRootDePrueba aggregateRoot = new();

        Assert.Throws<ArgumentNullException>(() => aggregateRoot.RegistrarEvento(null!));
    }

    private sealed class AggregateRootDePrueba : AggregateRoot
    {
        public void Inicializar()
        {
            InicializarAggregateRoot();
        }

        public void RegistrarEvento(DomainEvent domainEvent)
        {
            AddDomainEvent(domainEvent);
        }

        public void Actualizar()
        {
            MarcarActualizacion();
        }
    }

    private sealed class DomainEventDePrueba : DomainEvent
    {
    }
}
