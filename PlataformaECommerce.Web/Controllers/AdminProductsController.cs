using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Contracts.Products;
using PlataformaECommerce.Web.OpenApi;

namespace PlataformaECommerce.Web.Controllers;

/// <summary>
/// Expone endpoints HTTP administrativos para gestionar el catálogo de productos.
/// </summary>
/// <remarks>
/// Este controlador concentra operaciones de escritura y administración sobre productos,
/// protegidas explícitamente por la política del backoffice para mantener una separación
/// clara respecto a la API pública de consulta.
/// </remarks>
[ApiController]
[Route("api/admin/products")]
[ApiExplorerSettings(GroupName = SwaggerGroups.Admin)]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[EnableRateLimiting(RateLimitingOptions.AdministrationPolicyName)]
public sealed class AdminProductsController : ControllerBase
{
    private readonly IProductCommandService _productCommandService;
    private readonly IProductPromotionService _productPromotionService;
    private readonly IProductStockService _productStockService;

    /// <summary>
    /// Inicializa una nueva instancia del controlador administrativo de productos.
    /// </summary>
    /// <param name="productCommandService">Contrato del servicio de escritura de productos.</param>
    /// <param name="productStockService">Contrato del servicio de inventario y disponibilidad.</param>
    /// <param name="productPromotionService">Contrato del servicio promocional y de merchandising.</param>
    public AdminProductsController(
        IProductCommandService productCommandService,
        IProductStockService productStockService,
        IProductPromotionService productPromotionService)
    {
        _productCommandService = productCommandService ?? throw new ArgumentNullException(nameof(productCommandService));
        _productStockService = productStockService ?? throw new ArgumentNullException(nameof(productStockService));
        _productPromotionService = productPromotionService ?? throw new ArgumentNullException(nameof(productPromotionService));
    }

    /// <summary>
    /// Crea un nuevo producto físico dentro del sistema.
    /// </summary>
    /// <param name="request">Solicitud HTTP de creación del producto físico.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Identificador del producto creado.</returns>
    [HttpPost("physical")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> CreatePhysical(
        [FromBody] CreatePhysicalProductRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(CreateProblemDetails("Products.InvalidRequest", "La solicitud no puede ser nula.", StatusCodes.Status400BadRequest));
        }

        Result<Guid> result = await _productCommandService.CreatePhysicalProductAsync(
            new CreatePhysicalProductCommand
            {
                Name = request.Name,
                Description = request.Description,
                Sku = request.Sku,
                Price = request.Price,
                Currency = request.Currency,
                Stock = request.Stock,
                Slug = request.Slug,
                MainImageUrl = request.MainImageUrl,
                IsActive = request.IsActive,
                IsFeatured = request.IsFeatured,
                CategoryId = request.CategoryId,
                SubcategoryId = request.SubcategoryId,
                Tags = request.Tags,
                WeightKg = request.WeightKg,
                HeightCm = request.HeightCm,
                WidthCm = request.WidthCm,
                LengthCm = request.LengthCm,
                RequiresShipping = request.RequiresShipping
            },
            cancellationToken);

        if (result.IsFailure)
        {
            return MapFailure<Guid>(result.Error);
        }

        return CreatedAtAction(nameof(ProductsController.GetById), "Products", new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Crea un nuevo producto digital dentro del sistema.
    /// </summary>
    /// <param name="request">Solicitud HTTP de creación del producto digital.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Identificador del producto creado.</returns>
    [HttpPost("digital")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> CreateDigital(
        [FromBody] CreateDigitalProductRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(CreateProblemDetails("Products.InvalidRequest", "La solicitud no puede ser nula.", StatusCodes.Status400BadRequest));
        }

        Result<Guid> result = await _productCommandService.CreateDigitalProductAsync(
            new CreateDigitalProductCommand
            {
                Name = request.Name,
                Description = request.Description,
                Sku = request.Sku,
                Price = request.Price,
                Currency = request.Currency,
                Stock = request.Stock,
                Slug = request.Slug,
                MainImageUrl = request.MainImageUrl,
                IsActive = request.IsActive,
                IsFeatured = request.IsFeatured,
                CategoryId = request.CategoryId,
                Tags = request.Tags,
                FileFormat = request.FileFormat,
                FileSizeMb = request.FileSizeMb,
                RequiresLicense = request.RequiresLicense
            },
            cancellationToken);

        if (result.IsFailure)
        {
            return MapFailure<Guid>(result.Error);
        }

        return CreatedAtAction(nameof(ProductsController.GetById), "Products", new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Actualiza un producto existente.
    /// </summary>
    /// <param name="id">Identificador del producto a actualizar.</param>
    /// <param name="request">Solicitud HTTP de actualización.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Representación actualizada del producto.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponseDto>> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(CreateProblemDetails("Products.InvalidRequest", "La solicitud no puede ser nula.", StatusCodes.Status400BadRequest));
        }

        Result<ProductResponseDto> result = await _productCommandService.UpdateProductAsync(
            new UpdateProductCommand
            {
                Id = id,
                Name = request.Name,
                Description = request.Description,
                Sku = request.Sku,
                Price = request.Price,
                Currency = request.Currency,
                Stock = request.Stock,
                Slug = request.Slug,
                MainImageUrl = request.MainImageUrl,
                IsActive = request.IsActive,
                IsFeatured = request.IsFeatured,
                ProductType = request.ProductType,
                CategoryId = request.CategoryId,
                Tags = request.Tags,
                WeightKg = request.WeightKg,
                HeightCm = request.HeightCm,
                WidthCm = request.WidthCm,
                LengthCm = request.LengthCm,
                RequiresShipping = request.RequiresShipping,
                FileFormat = request.FileFormat,
                FileSizeMb = request.FileSizeMb,
                RequiresLicense = request.RequiresLicense
            },
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapFailure<ProductResponseDto>(result.Error);
    }

    /// <summary>
    /// Activa un producto existente para su operación comercial.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="request">Solicitud HTTP de activación.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Representación actualizada del producto tras la activación.</returns>
    [HttpPost("{id:guid}/activation")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProductResponseDto>> Activate(
        Guid id,
        [FromBody] ActivateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(CreateProblemDetails("Products.InvalidRequest", "La solicitud no puede ser nula.", StatusCodes.Status400BadRequest));
        }

        Result<ProductResponseDto> result = await _productStockService.ActivateProductAsync(
            new ActivateProductCommand
            {
                ProductId = id,
                RequestedByUserId = request.RequestedByUserId,
                Reason = request.Reason,
                ExternalReference = request.ExternalReference
            },
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapFailure<ProductResponseDto>(result.Error);
    }

    /// <summary>
    /// Desactiva un producto existente para retirarlo de la operación comercial.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="request">Solicitud HTTP opcional de desactivación.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Representación actualizada del producto tras la desactivación.</returns>
    [HttpDelete("{id:guid}/activation")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProductResponseDto>> Deactivate(
        Guid id,
        [FromBody] DeactivateProductRequest? request,
        CancellationToken cancellationToken)
    {
        Result<ProductResponseDto> result = await _productStockService.DeactivateProductAsync(
            new DeactivateProductCommand
            {
                ProductId = id,
                RequestedByUserId = request?.RequestedByUserId,
                Reason = request?.Reason,
                ExternalReference = request?.ExternalReference
            },
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapFailure<ProductResponseDto>(result.Error);
    }

    /// <summary>
    /// Marca un producto como destacado dentro del catálogo.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="request">Solicitud HTTP de destacado.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Representación actualizada del producto tras marcarlo como destacado.</returns>
    [HttpPost("{id:guid}/feature")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProductResponseDto>> Feature(
        Guid id,
        [FromBody] FeatureProductRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(CreateProblemDetails("Products.InvalidRequest", "La solicitud no puede ser nula.", StatusCodes.Status400BadRequest));
        }

        Result<ProductResponseDto> result = await _productPromotionService.FeatureProductAsync(
            new FeatureProductCommand
            {
                ProductId = id,
                RequestedByUserId = request.RequestedByUserId,
                Reason = request.Reason
            },
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapFailure<ProductResponseDto>(result.Error);
    }

    /// <summary>
    /// Retira la marca de destacado de un producto.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="request">Solicitud HTTP opcional de retiro de destacado.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Representación actualizada del producto tras retirar el destacado.</returns>
    [HttpDelete("{id:guid}/feature")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProductResponseDto>> Unfeature(
        Guid id,
        [FromBody] UnfeatureProductRequest? request,
        CancellationToken cancellationToken)
    {
        Result<ProductResponseDto> result = await _productPromotionService.UnfeatureProductAsync(
            new UnfeatureProductCommand
            {
                ProductId = id,
                RequestedByUserId = request?.RequestedByUserId,
                Reason = request?.Reason
            },
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapFailure<ProductResponseDto>(result.Error);
    }

    /// <summary>
    /// Ajusta el inventario de un producto mediante una operación administrativa explícita.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="request">Solicitud HTTP de ajuste de inventario.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Representación actualizada del producto tras el ajuste de stock.</returns>
    [HttpPut("{id:guid}/stock")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProductResponseDto>> UpdateStock(
        Guid id,
        [FromBody] UpdateProductStockRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(CreateProblemDetails("Products.InvalidRequest", "La solicitud no puede ser nula.", StatusCodes.Status400BadRequest));
        }

        Result<ProductResponseDto> result = await _productStockService.UpdateProductStockAsync(
            new UpdateProductStockCommand
            {
                ProductId = id,
                UpdateType = request.UpdateType,
                Quantity = request.Quantity,
                Reason = request.Reason,
                RequestedByUserId = request.RequestedByUserId,
                ExternalReference = request.ExternalReference
            },
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapFailure<ProductResponseDto>(result.Error);
    }

    /// <summary>
    /// Aplica una promoción porcentual sobre un producto existente.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="request">Solicitud HTTP de promoción.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Representación actualizada del producto tras aplicar la promoción.</returns>
    [HttpPost("{id:guid}/promotion")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProductResponseDto>> ApplyPromotion(
        Guid id,
        [FromBody] ApplyProductPromotionRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(CreateProblemDetails("Products.InvalidRequest", "La solicitud no puede ser nula.", StatusCodes.Status400BadRequest));
        }

        Result<ProductResponseDto> result = await _productPromotionService.ApplyProductPromotionAsync(
            new ApplyProductPromotionCommand
            {
                ProductId = id,
                DiscountPercentage = request.DiscountPercentage,
                RequestedByUserId = request.RequestedByUserId,
                Reason = request.Reason
            },
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapFailure<ProductResponseDto>(result.Error);
    }

    /// <summary>
    /// Retira la promoción activa de un producto y restaura su precio base.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="request">Solicitud HTTP opcional de retiro de promoción.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Representación actualizada del producto tras restaurar el precio base.</returns>
    [HttpDelete("{id:guid}/promotion")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProductResponseDto>> RemovePromotion(
        Guid id,
        [FromBody] RemoveProductPromotionRequest? request,
        CancellationToken cancellationToken)
    {
        Result<ProductResponseDto> result = await _productPromotionService.RemoveProductPromotionAsync(
            new RemoveProductPromotionCommand
            {
                ProductId = id,
                RequestedByUserId = request?.RequestedByUserId,
                Reason = request?.Reason
            },
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapFailure<ProductResponseDto>(result.Error);
    }

    /// <summary>
    /// Informa que la eliminación de productos aún no se encuentra expuesta en la API web actual.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <returns>Respuesta indicando funcionalidad no implementada.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status501NotImplemented)]
    public IActionResult Delete(Guid id)
    {
        return StatusCode(
            StatusCodes.Status501NotImplemented,
            CreateProblemDetails(
                "Products.DeleteNotImplemented",
                "La eliminación de productos aún no se encuentra implementada en la capa web actual.",
                StatusCodes.Status501NotImplemented));
    }

    private ActionResult<T> MapFailure<T>(Error error)
    {
        ProblemDetails problemDetails = CreateProblemDetails(error.Code, error.Message, ResolveStatusCode(error));
        return StatusCode(problemDetails.Status ?? StatusCodes.Status500InternalServerError, problemDetails);
    }

    private static ProblemDetails CreateProblemDetails(string code, string detail, int statusCode)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = code,
            Detail = detail
        };
    }

    private static int ResolveStatusCode(Error error)
    {
        return error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Failure => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
