using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Exceptions;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Domain.Usuarios;

[TestFixture]
public class AdministradorTests
{
    [Test]
    public void Constructor_AreaValida_AsignaAreaIndicada()
    {
        Administrador administrador = CrearAdministrador("Inventario");

        Assert.That(administrador.Area, Is.EqualTo("Inventario"));
    }

    [Test]
    public void Constructor_AreaVacia_LanzaUsuarioNoValidoException()
    {
        Assert.Throws<UsuarioNoValidoException>(() => CrearAdministrador(string.Empty));
    }

    [Test]
    public void ActualizarArea_ValorValido_ActualizaArea()
    {
        Administrador administrador = CrearAdministrador("Inventario");

        administrador.ActualizarArea("Tecnología");

        Assert.That(administrador.Area, Is.EqualTo("Tecnología"));
    }

    private static Administrador CrearAdministrador(string area)
    {
        return new Administrador(
            "Admin Principal",
            new Email("admin@email.com"),
            "hash-de-prueba-seguro-12345",
            area);
    }
}
