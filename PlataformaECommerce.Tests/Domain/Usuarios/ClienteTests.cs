using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Domain.Usuarios;

[TestFixture]
public class ClienteTests
{
    [Test]
    public void RegistrarCompra_IdValido_RegistraCompraCorrectamente()
    {
        Cliente cliente = CrearClienteValido();
        Guid pedidoId = Guid.NewGuid();

        cliente.RegistrarCompra(pedidoId);

        Assert.That(cliente.TieneCompraRegistrada(pedidoId), Is.True);
    }

    [Test]
    public void RegistrarCompra_IdVacio_LanzaUserException()
    {
        Cliente cliente = CrearClienteValido();

        Assert.Throws<UserException>(() => cliente.RegistrarCompra(Guid.Empty));
    }

    [Test]
    public void RegistrarCompra_Duplicada_LanzaUserException()
    {
        Cliente cliente = CrearClienteValido();
        Guid pedidoId = Guid.NewGuid();
        cliente.RegistrarCompra(pedidoId);

        Assert.Throws<UserException>(() => cliente.RegistrarCompra(pedidoId));
    }

    [Test]
    public void AgregarPreferencia_ValorValido_SeAgregaCorrectamente()
    {
        Cliente cliente = CrearClienteValido();

        cliente.AgregarPreferencia("Tecnología");

        Assert.That(cliente.TienePreferencia("Tecnología"), Is.True);
    }

    [Test]
    public void EliminarPreferencia_Existente_SeEliminaCorrectamente()
    {
        Cliente cliente = CrearClienteValido();
        cliente.AgregarPreferencia("Tecnología");

        cliente.EliminarPreferencia("Tecnología");

        Assert.That(cliente.TienePreferencia("Tecnología"), Is.False);
    }

    [Test]
    public void LimpiarPreferencias_ConDatos_EliminaTodasLasPreferencias()
    {
        Cliente cliente = CrearClienteValido();
        cliente.AgregarPreferencia("Gaming");
        cliente.AgregarPreferencia("Tecnología");

        cliente.LimpiarPreferencias();

        Assert.That(cliente.Preferencias, Is.Empty);
    }

    private static Cliente CrearClienteValido()
    {
        return new Cliente(
            "Laura Gómez",
            new Email("laura@email.com"),
            "hash-de-prueba-seguro-12345");
    }
}
