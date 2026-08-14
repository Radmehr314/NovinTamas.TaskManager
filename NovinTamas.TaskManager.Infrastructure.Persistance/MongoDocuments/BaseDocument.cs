using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments
{
    public class BaseDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
