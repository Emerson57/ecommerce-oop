using System.Xml;
using System.Xml.Linq;
using PlataformaECommerce.Application.Common.Results;

namespace PlataformaECommerce.Application.Features.Categories.Importing;

/// <summary>
/// Interpreta documentos XML de categorías y proyecta una colección importable alineada con el esquema persistente.
/// </summary>
internal static class CategoryXmlImportParser
{
    /// <summary>
    /// Convierte el contenido XML suministrado en una definición validada de categorías alineada con `Name`, `Slug`, `Description`, `IsActive` y `ParentCategoryName`.
    /// </summary>
    /// <param name="xmlContent">Contenido XML a interpretar.</param>
    /// <returns>Resultado con la jerarquía importable o un error de validación.</returns>
    internal static Result<IReadOnlyCollection<ImportedCategoryDefinition>> Parse(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
        {
            return Result.Failure<IReadOnlyCollection<ImportedCategoryDefinition>>(
                Error.Validation("Categories.ImportXmlEmpty", "El documento XML de categorías no puede estar vacío."));
        }

        XDocument document;
        try
        {
            document = XDocument.Parse(xmlContent, LoadOptions.None);
        }
        catch (XmlException)
        {
            return Result.Failure<IReadOnlyCollection<ImportedCategoryDefinition>>(
                Error.Validation("Categories.ImportXmlInvalid", "El archivo XML de categorías no tiene un formato válido."));
        }

        XElement? root = document.Root;
        if (root is null || !string.Equals(root.Name.LocalName, "Categories", StringComparison.Ordinal))
        {
            return Result.Failure<IReadOnlyCollection<ImportedCategoryDefinition>>(
                Error.Validation("Categories.ImportRootInvalid", "El documento XML debe iniciar con el nodo raíz 'Categories'."));
        }

        XElement[] categoryElements = root.Elements("Category").ToArray();
        if (categoryElements.Length == 0)
        {
            return Result.Failure<IReadOnlyCollection<ImportedCategoryDefinition>>(
                Error.Validation("Categories.ImportWithoutItems", "La plantilla XML debe incluir al menos un nodo 'Category'."));
        }

        List<ImportedCategoryDefinition> categories = [];
        HashSet<string> importedSlugs = new(StringComparer.OrdinalIgnoreCase);

        foreach (XElement categoryElement in categoryElements)
        {
            Result<ImportedCategoryDefinition> categoryResult = ParseCategory(categoryElement, importedSlugs);
            if (categoryResult.IsFailure)
            {
                return Result.Failure<IReadOnlyCollection<ImportedCategoryDefinition>>(categoryResult.Error);
            }

            categories.Add(categoryResult.Value);
        }

        return Result.Success<IReadOnlyCollection<ImportedCategoryDefinition>>(categories);
    }

    private static Result<ImportedCategoryDefinition> ParseCategory(XElement categoryElement, ISet<string> importedSlugs)
    {
        Result<ImportedNodeDefinition> nodeResult = ParseNode(categoryElement, importedSlugs);
        if (nodeResult.IsFailure)
        {
            return Result.Failure<ImportedCategoryDefinition>(nodeResult.Error);
        }

        ImportedNodeDefinition importedNode = nodeResult.Value;
        return Result.Success(new ImportedCategoryDefinition(
            importedNode.Name,
            importedNode.Slug,
            importedNode.Description,
            importedNode.IsActive,
            importedNode.ParentCategoryName));
    }

    private static Result<ImportedNodeDefinition> ParseNode(XElement element, ISet<string> importedSlugs)
    {
        string? name = ReadTrimmedValue(element, "Name");
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<ImportedNodeDefinition>(
                Error.Validation("Categories.ImportNameRequired", "Cada nodo 'Category' debe incluir un valor para 'Name'."));
        }

        string? slug = ReadTrimmedValue(element, "Slug");
        if (string.IsNullOrWhiteSpace(slug))
        {
            return Result.Failure<ImportedNodeDefinition>(
                Error.Validation("Categories.ImportSlugRequired", "Cada nodo 'Category' debe incluir un valor para 'Slug'."));
        }

        if (!importedSlugs.Add(slug))
        {
            return Result.Failure<ImportedNodeDefinition>(
                Error.Validation("Categories.ImportDuplicatedSlug", $"El slug '{slug}' está repetido dentro del archivo XML."));
        }

        string? isActiveValue = ReadTrimmedValue(element, "IsActive");
        bool isActive = true;
        if (!string.IsNullOrWhiteSpace(isActiveValue) && !bool.TryParse(isActiveValue, out isActive))
        {
            return Result.Failure<ImportedNodeDefinition>(
                Error.Validation("Categories.ImportInvalidIsActive", "El valor 'IsActive' del nodo 'Category' debe ser 'true' o 'false'."));
        }

        return Result.Success(new ImportedNodeDefinition(
            name,
            slug,
            ReadTrimmedValue(element, "Description"),
            isActive,
            ReadTrimmedValue(element, "ParentCategoryName")));
    }

    private static string? ReadTrimmedValue(XElement element, string childName)
    {
        string? value = element.Element(childName)?.Value;
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    /// <summary>
    /// Representa una fila lógica importable de categorías alineada con el esquema persistente.
    /// </summary>
    internal sealed record ImportedCategoryDefinition(
        string Name,
        string Slug,
        string? Description,
        bool IsActive,
        string? ParentCategoryName);

    private sealed record ImportedNodeDefinition(
        string Name,
        string Slug,
        string? Description,
        bool IsActive,
        string? ParentCategoryName);
}
