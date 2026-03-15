using MongoDB.Driver;
using PlataformaECommerce.Application.Interfaces.Repositories.Audit;
using PlataformaECommerce.Infrastructure.Mongo.Repositories.Products;

namespace PlataformaECommerce.Infrastructure.Mongo.Repositories
{
    public sealed class MongoProductAuditRepository : IProductoAuditRepository
    {
        #region Campos privados

        private readonly IMongoCollection<ProductAuditDocument> _collection;

        #endregion

        #region Constructor

        /// Inicializa una nueva instancia del repositorio de auditoría de productos.
        public MongoProductAuditRepository(IMongoDatabase database)
        {
            if (database is null)
                throw new ArgumentNullException(nameof(database));

            _collection = database.GetCollection<ProductAuditDocument>("product_audit");
        }

        #endregion

        #region Métodos públicos

        /// Registra un evento de auditoría asociado a un producto.
        public async Task RegistrarEventoAsync(int productoId, string accion, string detalle, string usuarioResponsable)
        {
            if (productoId <= 0)
                throw new ArgumentOutOfRangeException(nameof(productoId), "El Id del producto debe ser mayor que cero.");

            if (string.IsNullOrWhiteSpace(accion))
                throw new ArgumentException("La acción es obligatoria.", nameof(accion));

            if (string.IsNullOrWhiteSpace(detalle))
                throw new ArgumentException("El detalle del evento es obligatorio.", nameof(detalle));

            if (string.IsNullOrWhiteSpace(usuarioResponsable))
                throw new ArgumentException("El usuario responsable es obligatorio.", nameof(usuarioResponsable));

            ProductAuditDocument document = new()
            {
                ProductoId = productoId,
                Accion = accion.Trim().ToUpperInvariant(),
                Detalle = detalle.Trim(),
                UsuarioResponsable = usuarioResponsable.Trim(),
                FechaEvento = DateTime.UtcNow
            };

            await _collection.InsertOneAsync(document);
        }

        /// Obtiene el historial de auditoría de un producto en formato legible.
        public async Task<IReadOnlyList<string>> ObtenerHistorialAsync(int productoId)
        {
            if (productoId <= 0)
                throw new ArgumentOutOfRangeException(nameof(productoId), "El Id del producto debe ser mayor que cero.");

            List<ProductAuditDocument> documents = await _collection
                .Find(d => d.ProductoId == productoId)
                .SortByDescending(d => d.FechaEvento)
                .ToListAsync();

            return documents
                .Select(d => $"{d.FechaEvento:u} | {d.Accion} | {d.Detalle} | Usuario: {d.UsuarioResponsable}")
                .ToList()
                .AsReadOnly();
        }

        #endregion
    }
}