namespace NovinTamas.TaskManager.Domain.Common;

public class OutboxMessage
{
    public string Id { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public bool IsProcessed { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
}
