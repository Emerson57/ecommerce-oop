using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Features.Products.Queries;
using PlataformaECommerce.Application.Interfaces.Services.Products;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.OpenApi;

namespace PlataformaECommerce.Web.Controllers;

/// <summary>
/// Expone endpoints HTTP públicos para consultar el catálogo de productos.
/// </summary>
/// <remarks>
/// Este controlador concentra únicamente operaciones de lectura del catálogo,
/// manteniendo separadas las capacidades administrativas de escritura y gestión.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = SwaggerGroups.Public)]
[EnableRateLimiting(WebRateLimitingOptions.PublicApiPolicyName)]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductQueryService _productQueryService;

    /// <summary>
    /// Inicializa una nueva instancia del controlador público de productos.
    /// </summary>
    /// <param name="productQueryService">Contrato del servicio de consulta de productos.</param>
    public ProductsController(IProductQueryService productQueryService)
    {
        _productQueryService = productQueryService ?? throw new ArgumentNullException(nameof(productQueryService));
    }

    /// <summary>
    /// Obtiene un listado paginado de productos aplicando filtros opcionales.
    /// </summary>
    /// <param name="query">Parámetros de consulta y filtrado.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación.</param>
    /// <returns>Resultado paginado de productos.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ProductQueryResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductQueryResultDto>> GetAll(
        [FromQuery] GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        Result<ProductQueryResultDto> result = await _productQueryService.GetProductsAsync(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapFailure<ProductQueryResultDto>(result.Error);
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
        Result<ProductDetailDto> result = await _productQueryService.GetProductByIdAsync(
            new GetProductByIdQuery { ProductId = id },
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapFailure<ProductDetailDto>(result.Error);
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