using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Domain.Models.OutboxMessages;
using System.Text;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Services;

public class OutboxDispatcher : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int MaxRetryCount = 10;
    private const int BatchSize = 50;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcher> _logger;

    public OutboxDispatcher(IServiceScopeFactory scopeFactory, ILogger<OutboxDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var messages = await repository.GetUnprocessedMessagesAsync(BatchSize);

        foreach (var message in messages)
        {
            if (ct.IsCancellationRequested) break;
            if (message.RetryCount >= MaxRetryCount) continue;

            try
            {
                await publisher.PublishAsync(message.EventType, Encoding.UTF8.GetBytes(message.Body), message.RoutingKey);
                await repository.MarkAsProcessedAsync(message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish outbox message {MessageId}", message.Id);
                await repository.MarkAsFailedAsync(message.Id, ex.Message);
            }
        }
    }
}
