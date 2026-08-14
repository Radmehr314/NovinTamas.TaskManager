using NovinTamas.TaskManager.Application.Contracts.Contracts;
using RabbitMQ.Client;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Services;

public sealed class EventPublisher : IEventPublisher, IDisposable
{
    private readonly ConnectionFactory _factory;
    private readonly string _exchange;
    private readonly object _sync = new();

    private IConnection? _connection;
    private IModel? _channel;

    public EventPublisher(string host, int port, string user, string pass, string exchange = "novintamas.notifications")
    {
        _exchange = exchange;
        _factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = user,
            Password = pass,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };
    }

    public Task PublishAsync(string eventType, ReadOnlyMemory<byte> body, string routingKey)
    {
        EnsureConnected();

        var props = _channel!.CreateBasicProperties();
        props.ContentType = "application/json";
        props.DeliveryMode = 2;
        props.Type = eventType;
        props.MessageId = Guid.NewGuid().ToString();

        _channel.BasicPublish(_exchange, routingKey, props, body.ToArray());
        return Task.CompletedTask;
    }

    private void EnsureConnected()
    {
        if (_connection?.IsOpen == true && _channel?.IsOpen == true)
            return;

        lock (_sync)
        {
            if (_connection?.IsOpen == true && _channel?.IsOpen == true)
                return;

            SafeCloseAndDispose();
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true, autoDelete: false);
        }
    }

    private void SafeCloseAndDispose()
    {
        try { _channel?.Close(); } catch { }
        try { _connection?.Close(); } catch { }
        try { _channel?.Dispose(); } catch { }
        try { _connection?.Dispose(); } catch { }

        _channel = null;
        _connection = null;
    }

    public void Dispose()
    {
        lock (_sync)
        {
            SafeCloseAndDispose();
        }
    }
}
