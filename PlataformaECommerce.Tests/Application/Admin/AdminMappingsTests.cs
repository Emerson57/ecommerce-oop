using PlataformaECommerce.Application.Features.Admin.Mappings;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Application.Admin;

[TestFixture]
public class AdminMappingsTests
{
    [Test]
    public void ToAdminDto_AdministradorConArea_ProyectaArea()
    {
        Administrador admin = CrearAdministrador();

        var dto = admin.ToAdminDto();

        Assert.That(dto.Area, Is.EqualTo("Operaciones"));
    }

    [Test]
    public void ToAdminDto_AdministradorCreado_ProyectaRolAdministrador()
    {
        Administrador admin = CrearAdministrador();

        var dto = admin.ToAdminDto();

        Assert.That(dto.Role, Is.EqualTo(admin.Rol));
    }

    private static Administrador CrearAdministrador()
    {
        return new Administrador(
            "Admin Demo",
            new Email("admin@plataforma.com"),
            "hash-admin-seguro-2026",
            "Operaciones");
    }
}
