using PlataformaECommerce.Domain.Entities.Categories;
using PlataformaECommerce.Domain.Exceptions;

namespace PlataformaECommerce.Tests.Domain.Categories;

[TestFixture]
public class CategoriaProductoTests
{
    [Test]
    public void Constructor_SinPadre_CreaCategoriaRaiz()
    {
        CategoriaProducto categoria = new("Tecnología", "tecnologia");

        Assert.That(categoria.EsCategoriaRaiz, Is.True);
    }

    [Test]
    public void ReasignarPadre_ConMismoId_LanzaDomainException()
    {
        CategoriaProducto categoria = new("Tecnología", "tecnologia");

        Assert.Throws<DomainException>(() => categoria.ReasignarPadre(categoria.Id));
    }

    [Test]
    public void ConvertirEnCategoriaRaiz_ConPadre_EliminaReferenciaPadre()
    {
        CategoriaProducto categoria = new("Laptops", "laptops", parentCategoryId: Guid.NewGuid());

        categoria.ConvertirEnCategoriaRaiz();

        Assert.That(categoria.EsCategoriaRaiz, Is.True);
    }

    [Test]
    public void TienePadre_ConPadreAsignado_RetornaTrue()
    {
        Guid parentCategoryId = Guid.NewGuid();
        CategoriaProducto categoria = new("Laptops", "laptops", parentCategoryId: parentCategoryId);

        Assert.That(categoria.TienePadre(parentCategoryId), Is.True);
    }
}
