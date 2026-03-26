using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Categories.Commands;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Categories;
using PlataformaECommerce.Web.ViewComponents;

namespace PlataformaECommerce.Tests.Web.Shared;

[TestFixture]
public class StoreCategoryNavViewComponentTests
{
    [Test]
    public async Task InvokeAsync_CategoriasActivasRaiz_ProyectaMenuDinamico()
    {
        StoreCategoryNavViewComponent component = CreateComponent(new FakeCategoryApplicationService(
        [
            new CategoryDto { Id = Guid.NewGuid(), Name = "Tecnología", IsActive = true, IsRootCategory = true, Slug = "tecnologia" },
            new CategoryDto { Id = Guid.NewGuid(), Name = "Hogar", IsActive = true, IsRootCategory = true, Slug = "hogar" },
            new CategoryDto { Id = Guid.NewGuid(), Name = "Audio", IsActive = true, IsRootCategory = false, ParentCategoryId = Guid.NewGuid(), Slug = "audio" }
        ]));

        IViewComponentResult result = await component.InvokeAsync();

        ViewViewComponentResult viewResult = result as ViewViewComponentResult ?? throw new AssertionException("Se esperaba un resultado de vista.");
        IReadOnlyCollection<StoreCategoryNavViewComponent.StoreCategoryNavItemViewModel> model = viewResult.ViewData?.Model as IReadOnlyCollection<StoreCategoryNavViewComponent.StoreCategoryNavItemViewModel>
            ?? throw new AssertionException("Se esperaba un modelo con categorías proyectadas.");

        Assert.That(model.Select(item => item.Name), Is.EqualTo(new[] { "Hogar", "Tecnología" }));
    }

    [Test]
    public async Task InvokeAsync_FalloAlConsultarCategorias_RetornaColeccionVacia()
    {
        StoreCategoryNavViewComponent component = CreateComponent(new FailingCategoryApplicationService());

        IViewComponentResult result = await component.InvokeAsync();

        ViewViewComponentResult viewResult = result as ViewViewComponentResult ?? throw new AssertionException("Se esperaba un resultado de vista.");
        IReadOnlyCollection<StoreCategoryNavViewComponent.StoreCategoryNavItemViewModel> model = viewResult.ViewData?.Model as IReadOnlyCollection<StoreCategoryNavViewComponent.StoreCategoryNavItemViewModel>
            ?? throw new AssertionException("Se esperaba un modelo con categorías proyectadas.");

        Assert.That(model, Is.Empty);
    }

    private static StoreCategoryNavViewComponent CreateComponent(ICategoryApplicationService categoryApplicationService)
    {
        StoreCategoryNavViewComponent component = new(categoryApplicationService)
        {
            ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new Microsoft.AspNetCore.Mvc.Rendering.ViewContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            }
        };

        return component;
    }

    private sealed class FakeCategoryApplicationService(IReadOnlyCollection<CategoryDto> categories) : ICategoryApplicationService
    {
        public Task<Result<IReadOnlyCollection<CategoryDto>>> GetCategoriesAsync(GetCategoriesQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(categories));

        public Task<Result<CategoryDto>> GetCategoryByIdAsync(GetCategoryByIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<Guid>> CreateCategoryAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CategoryImportResultDto>> ImportCategoriesFromXmlAsync(ImportCategoriesFromXmlCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CategoryDto>> UpdateCategoryAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CategoryDto>> ChangeCategoryStatusAsync(ChangeCategoryStatusCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FailingCategoryApplicationService : ICategoryApplicationService
    {
        public Task<Result<IReadOnlyCollection<CategoryDto>>> GetCategoriesAsync(GetCategoriesQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Failure<IReadOnlyCollection<CategoryDto>>(Error.Failure("Categories.QueryFailed", "No fue posible consultar categorías.")));

        public Task<Result<CategoryDto>> GetCategoryByIdAsync(GetCategoryByIdQuery query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<Guid>> CreateCategoryAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CategoryImportResultDto>> ImportCategoriesFromXmlAsync(ImportCategoriesFromXmlCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CategoryDto>> UpdateCategoryAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<CategoryDto>> ChangeCategoryStatusAsync(ChangeCategoryStatusCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
