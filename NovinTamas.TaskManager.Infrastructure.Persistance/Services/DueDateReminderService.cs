using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NovinTamas.TaskManager.Domain.Enums;
using NovinTamas.TaskManager.Domain.Models.Notifications;
using NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments.TaskMongo;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Services;

// هر ۳۰ دقیقه وظایفی که مهلتشان نزدیک یا گذشته را پیدا می‌کند و برای مسئولین اعلان می‌سازد.
// برای جلوگیری از اعلان تکراری، زمان آخرین یادآوری روی خود سند وظیفه ثبت می‌شود.
public class DueDateReminderService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan DueSoonWindow = TimeSpan.FromHours(24);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DueDateReminderService> _logger;

    public DueDateReminderService(IServiceScopeFactory scopeFactory, ILogger<DueDateReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // اجرای بلافاصله بعد از بالا آمدن سرویس مفید نیست؛ اول اجازه بده اتصال‌ها برقرار شوند
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DueDateReminderService failed");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task RunOnceAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        var notifications = scope.ServiceProvider.GetRequiredService<ITaskNotificationRepository>();

        var collection = database.GetCollection<TaskDocument>("Tasks");
        var now = DateTime.UtcNow;
        var f = Builders<TaskDocument>.Filter;

        var filter = f.And(
            f.Eq(x => x.IsArchived, false),
            f.Nin(x => x.Status, new[] { TaskState.Done, TaskState.Cancelled }),
            f.Ne(x => x.DueDate, null),
            f.Lte(x => x.DueDate, now.Add(DueSoonWindow)),
            f.SizeGt(x => x.AssigneeIds, 0));

        var tasks = await collection.Find(filter).Limit(500).ToListAsync();

        if (tasks.Count == 0) return;

        var pending = new List<TaskNotification>();
        var reminded = new List<TaskDocument>();

        foreach (var task in tasks)
        {
            var isOverdue = task.DueDate!.Value < now;

            // یادآوری هر وظیفه حداکثر یک بار در ۲۰ ساعت
            if (task.LastReminderAt.HasValue && task.LastReminderAt.Value > now.AddHours(-20))
                continue;

            var message = isOverdue
                ? $"مهلت انجام «{task.Title}» گذشته است"
                : $"مهلت انجام «{task.Title}» کمتر از ۲۴ ساعت دیگر است";

            foreach (var assigneeId in task.AssigneeIds)
            {
                pending.Add(new TaskNotification
                {
                    CompanyId = task.CompanyId,
                    TaskId = task.Id.ToString(),
                    TaskTitle = task.Title,
                    SenderUserId = string.Empty,
                    SenderName = "سیستم",
                    ReceiverUserId = assigneeId,
                    Message = message,
                    Type = isOverdue ? NotificationType.TaskOverdue : NotificationType.TaskDueSoon,
                    CreatedAt = now
                });
            }

            reminded.Add(task);
        }

        if (pending.Count > 0)
            await notifications.AddManyAsync(pending);

        if (reminded.Count > 0)
        {
            var updates = reminded.Select(task => new UpdateOneModel<TaskDocument>(
                Builders<TaskDocument>.Filter.Eq(x => x.Id, task.Id),
                Builders<TaskDocument>.Update.Set(x => x.LastReminderAt, now))).ToList();

            await collection.BulkWriteAsync(updates);
        }

        _logger.LogInformation("DueDateReminderService created {Count} reminders", pending.Count);
    }
}
