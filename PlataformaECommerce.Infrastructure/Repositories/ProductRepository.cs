using Microsoft.EntityFrameworkCore;
using PlataformaECommerce.Application.Interfaces.Repositories;
using PlataformaECommerce.Domain.Entities;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Repositories
{
    public sealed class ProductRepository : IProductoRepository
    {
        #region Campos privados

        private readonly ECommerceDbContext _context;

        #endregion

        #region Constructor

        /// Inicializa una nueva instancia del repositorio de productos.
        public ProductRepository(ECommerceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #endregion

        #region Métodos públicos

        /// Obtiene todos los productos registrados en la base de datos.
        public async Task<IReadOnlyList<Producto>> ObtenerTodosAsync()
        {
            List<ProductEntity> entities = await _context.Products
                .AsNoTracking()
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return entities
                .Select(MapearADominio)
                .ToList()
                .AsReadOnly();
        }

        /// Obtiene un producto por su identificador.
        public async Task<Producto?> ObtenerPorIdAsync(int id)
        {
            ProductEntity? entity = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (entity is null)
                return null;

            return MapearADominio(entity);
        }

        /// Verifica si existe un producto con el identificador indicado.
        public async Task<bool> ExistePorIdAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }

        /// Agrega un nuevo producto a la base de datos.
        public async Task AgregarAsync(Producto producto)
        {
            if (producto is null)
                throw new ArgumentNullException(nameof(producto));

            ProductEntity entity = MapearAEntity(producto);

            await _context.Products.AddAsync(entity);
        }

        /// Actualiza un producto existente en la base de datos.
        public async Task ActualizarAsync(Producto producto)
        {
            if (producto is null)
                throw new ArgumentNullException(nameof(producto));

            ProductEntity? entityExistente = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == producto.Id);

            if (entityExistente is null)
                throw new InvalidOperationException($"No se encontró el producto con Id {producto.Id} para actualizar.");

            ActualizarEntityDesdeDominio(entityExistente, producto);
        }

        /// Elimina un producto de la base de datos por su identificador.
        public async Task EliminarAsync(int id)
        {
            ProductEntity? entity = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (entity is null)
                return;

            _context.Products.Remove(entity);
        }

        #endregion

        #region Métodos privados de mapeo

        /// Convierte una entidad de persistencia en una entidad del dominio.
        private static Producto MapearADominio(ProductEntity entity)
        {
            if (EsTipoFisico(entity.TipoProducto))
            {
                ProductoFisico productoFisico = new(
                    entity.Id,
                    entity.Nombre,
                    entity.Descripcion,
                    entity.Precio,
                    entity.Stock,
                    entity.PesoKg ?? 0,
                    entity.AltoCm ?? 0,
                    entity.AnchoCm ?? 0,
                    entity.LargoCm ?? 0);

                if (!entity.Activo)
                    productoFisico.Desactivar();

                return productoFisico;
            }

            if (EsTipoDigital(entity.TipoProducto))
            {
                ProductoDigital productoDigital = new(
                    entity.Id,
                    entity.Nombre,
                    entity.Descripcion,
                    entity.Precio,
                    entity.Stock,
                    entity.FormatoArchivo ?? string.Empty,
                    entity.TamanoMB ?? 0);

                if (!entity.Activo)
                    productoDigital.Desactivar();

                return productoDigital;
            }

            throw new InvalidOperationException($"El tipo de producto '{entity.TipoProducto}' no es válido.");
        }

        /// Convierte una entidad del dominio en una entidad de persistencia.
        private static ProductEntity MapearAEntity(Producto producto)
        {
            ProductEntity entity = new()
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Stock = producto.Stock,
                Activo = producto.Activo,
                TipoProducto = ObtenerTipoProducto(producto),
                FechaCreacion = producto.FechaCreacion,
                FechaActualizacion = producto.FechaActualizacion
            };

            if (producto is ProductoDigital digital)
            {
                entity.FormatoArchivo = digital.FormatoArchivo;
                entity.TamanoMB = digital.TamanoMB;
            }
            else if (producto is ProductoFisico fisico)
            {
                entity.PesoKg = fisico.PesoKg;
                entity.AltoCm = fisico.AltoCm;
                entity.AnchoCm = fisico.AnchoCm;
                entity.LargoCm = fisico.LargoCm;
            }

            return entity;
        }

        /// Actualiza una entidad de persistencia a partir de una entidad del dominio.
        private static void ActualizarEntityDesdeDominio(ProductEntity entity, Producto producto)
        {
            entity.Nombre = producto.Nombre;
            entity.Descripcion = producto.Descripcion;
            entity.Precio = producto.Precio;
            entity.Stock = producto.Stock;
            entity.Activo = producto.Activo;
            entity.TipoProducto = ObtenerTipoProducto(producto);
            entity.FechaCreacion = producto.FechaCreacion;
            entity.FechaActualizacion = producto.FechaActualizacion;

            // Limpiar campos específicos antes de volver a asignar
            entity.FormatoArchivo = null;
            entity.TamanoMB = null;
            entity.PesoKg = null;
            entity.AltoCm = null;
            entity.AnchoCm = null;
            entity.LargoCm = null;

            if (producto is ProductoDigital digital)
            {
                entity.FormatoArchivo = digital.FormatoArchivo;
                entity.TamanoMB = digital.TamanoMB;
            }
            else if (producto is ProductoFisico fisico)
            {
                entity.PesoKg = fisico.PesoKg;
                entity.AltoCm = fisico.AltoCm;
                entity.AnchoCm = fisico.AnchoCm;
                entity.LargoCm = fisico.LargoCm;
            }
        }

        #endregion

        #region Métodos privados auxiliares

        /// Determina si el tipo indicado corresponde a un producto físico.
        private static bool EsTipoFisico(string tipoProducto)
        {
            return tipoProducto.Trim().Equals("Fisico", StringComparison.OrdinalIgnoreCase);
        }

        /// Determina si el tipo indicado corresponde a un producto digital.
        private static bool EsTipoDigital(string tipoProducto)
        {
            return tipoProducto.Trim().Equals("Digital", StringComparison.OrdinalIgnoreCase);
        }

        /// Obtiene el tipo lógico del producto según su tipo concreto.
        private static string ObtenerTipoProducto(Producto producto)
        {
            return producto switch
            {
                ProductoFisico => "Fisico",
                ProductoDigital => "Digital",
                _ => throw new InvalidOperationException("Tipo de producto no soportado.")
            };
        }

        #endregion
    }
}