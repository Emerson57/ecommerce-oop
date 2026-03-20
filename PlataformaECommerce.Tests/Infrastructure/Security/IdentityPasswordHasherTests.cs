using PlataformaECommerce.Infrastructure.Services.Auth;

namespace PlataformaECommerce.Tests.Infrastructure.Security;

[TestFixture]
public class IdentityPasswordHasherTests
{
    [Test]
    public void HashPassword_ContraseñaValida_GeneraHashDistintoAlTextoPlano()
    {
        IdentityPasswordHasher service = new();

        string result = service.HashPassword("ClaveSegura#2026");

        Assert.That(result, Is.Not.EqualTo("ClaveSegura#2026"));
    }

    [Test]
    public void VerifyPassword_HashGeneradoPorElServicio_RetornaTrue()
    {
        IdentityPasswordHasher service = new();
        string hash = service.HashPassword("ClaveSegura#2026");

        bool result = service.VerifyPassword("ClaveSegura#2026", hash);

        Assert.That(result, Is.True);
    }
}
