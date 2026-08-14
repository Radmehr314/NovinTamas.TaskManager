using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Application.Contracts.Messages.Session;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Events
{
    // روی هر revoke شدن ActiveSession در IAM خبردار می‌شود و فقط cache این‌مموری محلی را آپدیت می‌کند؛
    // هیچ call ای به IAM روی مسیر request انجام نمی‌شود.
    public sealed class SessionRevocationConsumer : BackgroundService
    {
        private readonly IConfiguration _cfg;
        private readonly ISessionRevocationCache _cache;

        private IConnection? _conn;
        private IModel? _ch;

        public SessionRevocationConsumer(IConfiguration cfg, ISessionRevocationCache cache)
        {
            _cfg = cfg;
            _cache = cache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var exchange = _cfg["Rabbit:Exchange"] ?? "novintamas.events";
            var queue = _cfg["Rabbit:QueueSession"] ?? "novintamas.taskmanager.session.queue";
            var routingKey = _cfg["Rabbit:RoutingKeySession"] ?? "session.revoked";

            var factory = new ConnectionFactory
            {
                HostName = _cfg["Rabbit:Host"] ?? "localhost",
                Port = int.TryParse(_cfg["Rabbit:Port"], out var p) ? p : 5672,
                UserName = _cfg["Rabbit:User"] ?? "guest",
                Password = _cfg["Rabbit:Pass"] ?? "guest",
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    SafeCloseAndDispose();

                    _conn = factory.CreateConnection();
                    _ch = _conn.CreateModel();

                    _ch.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);
                    _ch.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
                    _ch.QueueBind(queue, exchange, routingKey);
                    _ch.BasicQos(0, 20, false);

                    var consumer = new AsyncEventingBasicConsumer(_ch);

                    consumer.Received += (_, ea) =>
                    {
                        try
                        {
                            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                            var msg = JsonSerializer.Deserialize<SessionRevokedEvent>(json);

                            if (msg != null && !string.IsNullOrWhiteSpace(msg.Jti))
                                _cache.MarkRevoked(msg.Jti, msg.ExpiresAt);

                            _ch.BasicAck(ea.DeliveryTag, false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"SessionRevocationConsumer error: {ex}");
                            _ch?.BasicNack(ea.DeliveryTag, false, true);
                        }

                        return Task.CompletedTask;
                    };

                    _ch.BasicConsume(queue, autoAck: false, consumer: consumer);

                    await WaitUntilDisconnected(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SessionRevocationConsumer connection error: {ex}");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task WaitUntilDisconnected(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (_conn?.IsOpen != true || _ch?.IsOpen != true)
                    return;

                await Task.Delay(1000, ct);
            }
        }

        private void SafeCloseAndDispose()
        {
            try { _ch?.Close(); } catch { }
            try { _conn?.Close(); } catch { }
            try { _ch?.Dispose(); } catch { }
            try { _conn?.Dispose(); } catch { }

            _ch = null;
            _conn = null;
        }

        public override void Dispose()
        {
            SafeCloseAndDispose();
            base.Dispose();
        }
    }
}
