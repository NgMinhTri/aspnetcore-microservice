using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Contracts.Domains
{
    public class MongoEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public virtual string Id { get; protected init; }

        [BsonElement("createdDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [BsonElement("lastModifiedDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? LastModifiedDate { get; set; } = DateTime.UtcNow;
    }
}
