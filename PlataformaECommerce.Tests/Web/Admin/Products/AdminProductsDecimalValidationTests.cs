using System.Globalization;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using PlataformaECommerce.Web.Pages.Admin.Products;
using ValidationRangeAttribute = System.ComponentModel.DataAnnotations.RangeAttribute;

namespace PlataformaECommerce.Tests.Web.Admin.Products;

[TestFixture]
public class AdminProductsDecimalValidationTests
{
    [Test]
    public void CreatePriceRangeAttribute_ConCulturaDecimalConComa_NoFallaAlConstruirAdaptador()
    {
        RunWithCulture("es-CO", () =>
        {
            ValidationRangeAttribute attribute = GetPriceRangeAttribute(typeof(CreateModel.InputModel), nameof(CreateModel.InputModel.Price));
            ValidationAttributeAdapterProvider provider = new();

            Assert.That(() => provider.GetAttributeAdapter(attribute, stringLocalizer: null), Throws.Nothing);
        });
    }

    [Test]
    public void EditPriceRangeAttribute_ConCulturaDecimalConComa_NoFallaAlConstruirAdaptador()
    {
        RunWithCulture("es-CO", () =>
        {
            ValidationRangeAttribute attribute = GetPriceRangeAttribute(typeof(EditModel.InputModel), nameof(EditModel.InputModel.Price));
            ValidationAttributeAdapterProvider provider = new();

            Assert.That(() => provider.GetAttributeAdapter(attribute, stringLocalizer: null), Throws.Nothing);
        });
    }

    private static ValidationRangeAttribute GetPriceRangeAttribute(Type containerType, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(containerType);

        var property = containerType.GetProperty(propertyName)
            ?? throw new InvalidOperationException($"No se encontró la propiedad '{propertyName}' en '{containerType.FullName}'.");

        return property.GetCustomAttributes(typeof(ValidationRangeAttribute), inherit: true)
            .OfType<ValidationRangeAttribute>()
            .Single();
    }

    private static void RunWithCulture(string cultureName, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            action();
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }
}
