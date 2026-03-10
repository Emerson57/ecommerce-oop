using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PlataformaECommerce.Infrastructure.Mongo.Documents
{
    public sealed class ProductAuditDocument
    {
        /// Identificador único del documento en MongoDB.
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// Identificador del producto afectado.
        public int ProductoId { get; set; }

        /// Acción realizada sobre el producto.
        public string Accion { get; set; } = string.Empty;

        /// Descripción detallada del evento.
        public string Detalle { get; set; } = string.Empty;

        /// Usuario o actor responsable del cambio.
        public string UsuarioResponsable { get; set; } = string.Empty;

        /// Fecha y hora del evento en UTC.
        public DateTime FechaEvento { get; set; }
    }
}