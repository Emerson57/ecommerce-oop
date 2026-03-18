using Microsoft.AspNetCore.Mvc;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.Commands;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Features.Products.Services;

namespace PlataformaECommerce.Web.Controllers;

/// <summary>
/// Expone endpoints HTTP para consultar y administrar productos del sistema.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly ProductApplicationService _productApplicationService;

    /// <summary>
    /// Inicializa una nueva instancia del controlador de productos.
    /// </summary>
    /// <param name="productApplicationService">Servicio de aplicación de productos.</param>
    public ProductsController(ProductApplicationService productApplicationService)
    {
        _productApplicationService = productApplicationService ?? throw new ArgumentNullException(nameof(productApplicationService));
    }

    /// <summary>
    /// Obtiene un listado paginado de productos aplicando filtros opcionales.
    /// </summary>
    /// <param name="query">Parámetros de consulta y filtrado.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Colección proyectada de productos.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyCollection<ProductDto>>> GetAll(
        [FromQuery] GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<ProductDto>> result = await _productApplicationService.GetProductsAsync(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapFailure<IReadOnlyCollection<ProductDto>>(result.Error);
    }

    /// <summary>
    /// Obtiene el detalle de un producto por su identificador.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Detalle del producto cuando existe.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        Result<ProductDetailDto> result = await _productApplicationService.GetProductByIdAsync(
            new GetProductByIdQuery { ProductId = id },
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapFailure<ProductDetailDto>(result.Error);
    }

    /// <summary>
    /// Crea un nuevo producto físico dentro del sistema.
    /// </summary>
    /// <param name="command">Comando de creación del producto físico.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Identificador del producto creado.</returns>
    [HttpPost("physical")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> CreatePhysical(
        [FromBody] CreatePhysicalProductCommand command,
        CancellationToken cancellationToken)
    {
        if (command is null)
        {
            return BadRequest(CreateProblemDetails("Products.InvalidRequest", "La solicitud no puede ser nula.", StatusCodes.Status400BadRequest));
        }

        Result<Guid> result = await _productApplicationService.CreatePhysicalProductAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return MapFailure<Guid>(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Crea un nuevo producto digital dentro del sistema.
    /// </summary>
    /// <param name="command">Comando de creación del producto digital.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Identificador del producto creado.</returns>
    [HttpPost("digital")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> CreateDigital(
        [FromBody] CreateDigitalProductCommand command,
        CancellationToken cancellationToken)
    {
        if (command is null)
        {
            return BadRequest(CreateProblemDetails("Products.InvalidRequest", "La solicitud no puede ser nula.", StatusCodes.Status400BadRequest));
        }

        Result<Guid> result = await _productApplicationService.CreateDigitalProductAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return MapFailure<Guid>(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Actualiza un producto existente.
    /// </summary>
    /// <param name="id">Identificador del producto a actualizar.</param>
    /// <param name="command">Comando de actualización.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Representación actualizada del producto.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponseDto>> Update(
        Guid id,
        [FromBody] UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        if (command is null)
        {
            return BadRequest(CreateProblemDetails("Products.InvalidRequest", "La solicitud no puede ser nula.", StatusCodes.Status400BadRequest));
        }

        UpdateProductCommand adjustedCommand = new UpdateProductCommand
        {
            Id = id,
            Name = command.Name,
            Description = command.Description,
            Sku = command.Sku,
            Price = command.Price,
            Currency = command.Currency,
            Stock = command.Stock,
            Slug = command.Slug,
            MainImageUrl = command.MainImageUrl,
            IsActive = command.IsActive,
            IsFeatured = command.IsFeatured,
            ProductType = command.ProductType,
            CategoryId = command.CategoryId,
            Tags = command.Tags,
            WeightKg = command.WeightKg,
            HeightCm = command.HeightCm,
            WidthCm = command.WidthCm,
            LengthCm = command.LengthCm,
            RequiresShipping = command.RequiresShipping,
            FileFormat = command.FileFormat,
            FileSizeMb = command.FileSizeMb,
            RequiresLicense = command.RequiresLicense
        };
        Result<ProductResponseDto> result = await _productApplicationService.UpdateProductAsync(adjustedCommand, cancellationToken);

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