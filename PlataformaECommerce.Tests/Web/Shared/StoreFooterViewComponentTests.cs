using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Categories.Commands;
using PlataformaECommerce.Application.Features.Categories.DTOs;
using PlataformaECommerce.Application.Features.Categories.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Categories;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.ViewComponents;

namespace PlataformaECommerce.Tests.Web.Shared;

[TestFixture]
public class StoreFooterViewComponentTests
{
    [Test]
    public async Task InvokeAsync_CategoriasActivasRaiz_ProyectaSeccionDeCategorias()
    {
        StoreFooterViewComponent component = CreateComponent(new FakeCategoryApplicationService(
        [
            new CategoryDto { Id = Guid.NewGuid(), Name = "Tecnología", IsActive = true, IsRootCategory = true, Slug = "tecnologia" },
            new CategoryDto { Id = Guid.NewGuid(), Name = "Hogar", IsActive = true, IsRootCategory = true, Slug = "hogar" },
            new CategoryDto { Id = Guid.NewGuid(), Name = "Audio", IsActive = true, IsRootCategory = false, ParentCategoryId = Guid.NewGuid(), Slug = "audio" }
        ]));

        IViewComponentResult result = await component.InvokeAsync();

        StoreFooterViewComponent.StoreFooterViewModel model = ExtractModel(result);

        Assert.That(model.CategoryLinks.Select(link => link.Text), Is.EqualTo(new[] { "Hogar", "Tecnología" }));
    }

    [Test]
    public async Task InvokeAsync_UsuarioAnonimo_ProyectaAccesosPublicos()
    {
        StoreFooterViewComponent component = CreateComponent(new FakeCategoryApplicationService([]));

        IViewComponentResult result = await component.InvokeAsync();

        StoreFooterViewComponent.StoreFooterViewModel model = ExtractModel(result);

        Assert.That(model.AccessLinks.Select(link => link.Text), Is.EqualTo(new[] { "Ingresar", "Crear cuenta", "Carrito" }));
    }

    [Test]
    public async Task InvokeAsync_ConfiguraBrandingYSoporteDesdeOpciones()
    {
        StoreFooterViewComponent component = CreateComponent(new FakeCategoryApplicationService([]));

        IViewComponentResult result = await component.InvokeAsync();

        StoreFooterViewComponent.StoreFooterViewModel model = ExtractModel(result);

        Assert.That(model.BrandName, Is.EqualTo("NovaShop"));
        Assert.That(model.SupportEmail, Is.EqualTo("support@novashop.example"));
    }

    [Test]
    public async Task InvokeAsync_AdministradorAutenticado_ProyectaPanelYCorreo()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Email, "admin@novashop.com"),
            new Claim(ClaimTypes.Role, "Administrador")
        ], "Cookies"));
        StoreFooterViewComponent component = CreateComponent(new FakeCategoryApplicationService([]), user);

        IViewComponentResult result = await component.InvokeAsync();

        StoreFooterViewComponent.StoreFooterViewModel model = ExtractModel(result);

        Assert.That(model.AccessLinks.Select(link => link.Text), Is.EqualTo(new[] { "Panel administrativo", "Carrito", "admin@novashop.com" }));
    }

    private static StoreFooterViewComponent.StoreFooterViewModel ExtractModel(IViewComponentResult result)
    {
        ViewViewComponentResult viewResult = result as ViewViewComponentResult ?? throw new AssertionException("Se esperaba un resultado de vista.");
        return viewResult.ViewData?.Model as StoreFooterViewComponent.StoreFooterViewModel
            ?? throw new AssertionException("Se esperaba un modelo de footer comercial.");
    }

    private static StoreFooterViewComponent CreateComponent(
        ICategoryApplicationService categoryApplicationService,
        ClaimsPrincipal? user = null)
    {
        DefaultHttpContext httpContext = new();
        httpContext.User = user ?? new ClaimsPrincipal(new ClaimsIdentity());

        StoreFooterViewComponent component = new(
            categoryApplicationService,
            Options.Create(new ClientExperienceOptions
            {
                StorefrontName = "NovaShop",
                StorefrontTagline = "Tienda configurable para una sola marca.",
                SupportEmail = "support@novashop.example",
                SupportPhone = "+57 300 000 0000",
                SupportHours = "Lunes a viernes, 08:00 a 18:00 UTC-5"
            }))
        {
            ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new Microsoft.AspNetCore.Mvc.Rendering.ViewContext
                {
                    HttpContext = httpContext
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
}
