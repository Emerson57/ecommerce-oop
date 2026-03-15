using PlataformaECommerce.Domain.Entities.Products;

namespace PlataformaECommerce.Application.Interfaces.Repositories.Products
{
    public interface IProductoRepository
    {
        /// Obtiene todos los productos registrados.
        Task<IReadOnlyList<Producto>> ObtenerTodosAsync();

        /// Obtiene un producto por su identificador.
        Task<Producto?> ObtenerPorIdAsync(int id);

        /// Verifica si existe un producto con el identificador indicado.
        Task<bool> ExistePorIdAsync(int id);

        /// Agrega un nuevo producto al repositorio.
        Task AgregarAsync(Producto producto);

        /// Actualiza un producto existente en el repositorio.
        Task ActualizarAsync(Producto producto);

        /// Elimina un producto del repositorio por su identificador.
        Task EliminarAsync(int id);
    }
}