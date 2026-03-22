using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using PlataformaECommerce.Infrastructure.Mongo.Repositories.Audit;

namespace PlataformaECommerce.Tests.Infrastructure.Mongo;

[TestFixture]
public class AuditDocumentTests
{
    [Test]
    public void ToBsonDocument_AggregateIdValido_SerializaGuidConRepresentacionEstandar()
    {
        AuditDocument document = new()
        {
            Id = "507f1f77bcf86cd799439011",
            AggregateId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            AggregateType = "Administrador",
            Module = "Admin",
            Action = "admin.registered",
            Detail = "Alta administrativa",
            PerformedBy = "root@plataforma.com",
            OccurredAtUtc = DateTime.UtcNow
        };

        BsonDocument bson = document.ToBsonDocument();

        Assert.That(bson.Contains("AggregateId"), Is.True);
        Assert.That(bson["AggregateId"].BsonType, Is.EqualTo(BsonType.Binary));
    }
}
