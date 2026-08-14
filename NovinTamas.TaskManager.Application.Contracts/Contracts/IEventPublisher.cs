namespace NovinTamas.TaskManager.Application.Contracts.Contracts;

public interface IEventPublisher
{
    Task PublishAsync(string eventType, ReadOnlyMemory<byte> body, string routingKey);
}
