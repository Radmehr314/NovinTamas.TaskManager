using MongoDB.Bson;
using MongoDB.Driver;
using NovinTamas.TaskManager.Domain.Common;
using NovinTamas.TaskManager.Domain.Models.OutboxMessages;
using NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments.OutboxMessageMongo;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Repositories;

public class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly IMongoCollection<OutboxMessageDocument> _collection;

    public OutboxMessageRepository(IMongoDatabase mongoDatabase)
    {
        _collection = mongoDatabase.GetCollection<OutboxMessageDocument>("OutboxMessages");
    }

    public async Task AddAsync(OutboxMessage message)
    {
        var doc = OutboxMessageMapper.ToDocument(message);
        await _collection.InsertOneAsync(doc);
        message.Id = doc.Id.ToString();
    }

    public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int limit = 50)
    {
        var filter = Builders<OutboxMessageDocument>.Filter.Eq(x => x.IsProcessed, false);
        var docs = await _collection.Find(filter)
            .SortBy(x => x.CreatedAt)
            .Limit(limit)
            .ToListAsync();

        return docs.Select(OutboxMessageMapper.ToDomain).ToList();
    }

    public async Task MarkAsProcessedAsync(string messageId)
    {
        var filter = Builders<OutboxMessageDocument>.Filter.Eq(x => x.Id, ObjectId.Parse(messageId));
        var update = Builders<OutboxMessageDocument>.Update
            .Set(x => x.IsProcessed, true)
            .Set(x => x.ProcessedAt, DateTime.UtcNow);

        await _collection.UpdateOneAsync(filter, update);
    }

    public async Task MarkAsFailedAsync(string messageId, string errorMessage)
    {
        var filter = Builders<OutboxMessageDocument>.Filter.Eq(x => x.Id, ObjectId.Parse(messageId));
        var update = Builders<OutboxMessageDocument>.Update
            .Inc(x => x.RetryCount, 1)
            .Set(x => x.ErrorMessage, errorMessage)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await _collection.UpdateOneAsync(filter, update);
    }
}
