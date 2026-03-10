namespace PlataformaECommerce.Infrastructure.Mongo
{
    /// Representa la configuración necesaria para conectarse a MongoDB.
    public sealed class MongoDbSettings
    {
        /// Cadena de conexión a MongoDB.
        public string ConnectionString { get; set; } = string.Empty;

        /// Nombre de la base de datos MongoDB.
        public string DatabaseName { get; set; } = string.Empty;
    }
}