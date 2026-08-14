using MongoDB.Bson.Serialization.Attributes;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments
{
    public class CounterDocument
    {
        [BsonId]
        public string Id { get; set; } = string.Empty;
        public long Sequence { get; set; }
    }
}
