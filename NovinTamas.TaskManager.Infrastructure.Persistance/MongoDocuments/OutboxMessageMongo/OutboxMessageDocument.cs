using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NovinTamas.TaskManager.Domain.Common;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments.OutboxMessageMongo;

public class OutboxMessageDocument : BaseDocument
{
    public string EventType { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? ProcessedAt { get; set; }
    public bool IsProcessed { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
}

public static class OutboxMessageMapper
{
    public static OutboxMessage ToDomain(OutboxMessageDocument document)
    {
        if (document == null) return null!;

        return new OutboxMessage
        {
            Id = document.Id.ToString(),
            EventType = document.EventType,
            RoutingKey = document.RoutingKey,
            Body = document.Body,
            CreatedAt = document.CreatedAt,
            ProcessedAt = document.ProcessedAt,
            IsProcessed = document.IsProcessed,
            RetryCount = document.RetryCount,
            ErrorMessage = document.ErrorMessage
        };
    }

    public static OutboxMessageDocument ToDocument(OutboxMessage domain)
    {
        if (domain == null) return null!;

        var objectId = string.IsNullOrWhiteSpace(domain.Id)
            ? ObjectId.GenerateNewId()
            : ObjectId.Parse(domain.Id);

        return new OutboxMessageDocument
        {
            Id = objectId,
            EventType = domain.EventType,
            RoutingKey = domain.RoutingKey,
            Body = domain.Body,
            CreatedAt = domain.CreatedAt,
            ProcessedAt = domain.ProcessedAt,
            IsProcessed = domain.IsProcessed,
            RetryCount = domain.RetryCount,
            ErrorMessage = domain.ErrorMessage
        };
    }
}
