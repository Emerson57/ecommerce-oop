using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Domain.Usuarios;

[TestFixture]
public class UsuarioTests
{
    [Test]
    public void Constructor_DatosValidos_CreaUsuarioCorrectamente()
    {
        Cliente usuario = CrearUsuarioValido();

        Assert.That(usuario.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(usuario.Nombre, Is.EqualTo("Juan Pérez"));
        Assert.That(usuario.CorreoElectronico.Value, Is.EqualTo("juan@email.com"));
        Assert.That(usuario.Activo, Is.True);
        Assert.That(usuario.FechaCreacionUtc, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void Constructor_NombreVacio_LanzaUsuarioNoValidoException()
    {
        Assert.Throws<UsuarioNoValidoException>(() =>
            new Cliente(string.Empty, new Email("juan@email.com"), "hash-de-prueba-seguro-12345"));
    }

    [Test]
    public void ActualizarDatosBasicos_DatosValidos_ActualizaNombreYCorreo()
    {
        Cliente usuario = CrearUsuarioValido();

        usuario.ActualizarDatosBasicos("Carlos Gómez", new Email("carlos@email.com"));

        Assert.That(usuario.Nombre, Is.EqualTo("Carlos Gómez"));
        Assert.That(usuario.CorreoElectronico.Value, Is.EqualTo("carlos@email.com"));
    }

    [Test]
    public void ConfirmarCorreoElectronico_UsuarioValido_MarcaCorreoComoConfirmado()
    {
        Cliente usuario = CrearUsuarioValido();

        usuario.ConfirmarCorreoElectronico();

        Assert.That(usuario.CorreoConfirmado, Is.True);
    }

    [Test]
    public void EstaHabilitado_ActivoYConCorreoConfirmado_RetornaTrue()
    {
        Cliente usuario = CrearUsuarioValido();
        usuario.ConfirmarCorreoElectronico();

        Assert.That(usuario.EstaHabilitado(), Is.True);
    }

    private static Cliente CrearUsuarioValido()
    {
        return new Cliente(
            "Juan Pérez",
            new Email("juan@email.com"),
            "hash-de-prueba-seguro-12345");
    }
}
