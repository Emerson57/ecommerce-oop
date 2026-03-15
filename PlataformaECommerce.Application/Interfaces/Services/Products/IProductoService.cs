using PlataformaECommerce.Application.Features.Products.DTOs;

namespace PlataformaECommerce.Application.Interfaces.Services.Products
{
    public interface IProductoService
    {
        /// Obtiene todos los productos del sistema.
        Task<IReadOnlyList<ProductResponse>> ObtenerTodosAsync();

        /// Obtiene un producto por su identificador.
        Task<ProductResponse?> ObtenerPorIdAsync(int id);

        /// Crea un nuevo producto.
        Task<ProductResponse> CrearAsync(CreateProductRequest request);

        /// Actualiza un producto existente.
        Task<ProductResponse?> ActualizarAsync(int id, UpdateProductRequest request);

        /// Elimina un producto por su identificador.
        Task<bool> EliminarAsync(int id);
    }
}