using NovinTamas.TaskManager.Domain.Common;

namespace NovinTamas.TaskManager.Domain.Models.OutboxMessages;

public interface IOutboxMessageRepository
{
    Task AddAsync(OutboxMessage message);
    Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int limit = 50);
    Task MarkAsProcessedAsync(string messageId);
    Task MarkAsFailedAsync(string messageId, string errorMessage);
}
