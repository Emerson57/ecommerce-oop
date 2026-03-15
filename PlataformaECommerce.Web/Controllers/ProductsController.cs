using Microsoft.AspNetCore.Mvc;
using PlataformaECommerce.Application.Features.Products.DTOs;
using PlataformaECommerce.Application.Interfaces.Services.Products;

namespace PlataformaECommerce.Web.Controllers
{
    /// Controlador API para la gestión de productos.
    [ApiController]
    [Route("api/[controller]")]
    public sealed class ProductsController : ControllerBase
    {
        #region Campos privados

        private readonly IProductoService _productoService;

        #endregion

        #region Constructor

        /// Inicializa una nueva instancia del controlador de productos.
        public ProductsController(IProductoService productoService)
        {
            _productoService = productoService ?? throw new ArgumentNullException(nameof(productoService));
        }

        #endregion

        #region Endpoints GET

        /// Obtiene todos los productos registrados.
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll()
        {
            IReadOnlyList<ProductResponse> productos = await _productoService.ObtenerTodosAsync();
            return Ok(productos);
        }

        /// Obtiene un producto por su identificador.
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductResponse>> GetById(int id)
        {
            ProductResponse? producto = await _productoService.ObtenerPorIdAsync(id);

            if (producto is null)
                return NotFound(new
                {
                    mensaje = $"No se encontró un producto con Id {id}."
                });

            return Ok(producto);
        }

        #endregion

        #region Endpoint POST

        /// Crea un nuevo producto.
        [HttpPost]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductRequest request)
        {
            if (request is null)
                return BadRequest(new
                {
                    mensaje = "La solicitud no puede ser nula."
                });

            ProductResponse productoCreado = await _productoService.CrearAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = productoCreado.Id },
                productoCreado);
        }

        #endregion

        #region Endpoint PUT

        /// Actualiza un producto existente.
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductResponse>> Update(int id, [FromBody] UpdateProductRequest request)
        {
            if (request is null)
                return BadRequest(new
                {
                    mensaje = "La solicitud no puede ser nula."
                });

            ProductResponse? productoActualizado = await _productoService.ActualizarAsync(id, request);

            if (productoActualizado is null)
                return NotFound(new
                {
                    mensaje = $"No se encontró un producto con Id {id} para actualizar."
                });

            return Ok(productoActualizado);
        }

        #endregion

        #region Endpoint DELETE

        /// Elimina un producto por su identificador.
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            bool eliminado = await _productoService.EliminarAsync(id);

            if (!eliminado)
                return NotFound(new
                {
                    mensaje = $"No se encontró un producto con Id {id} para eliminar."
                });

            return NoContent();
        }

        #endregion
    }
}