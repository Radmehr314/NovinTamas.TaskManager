using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Domain;
using NovinTamas.TaskManager.Domain.Common;
using NovinTamas.TaskManager.Domain.Enums;
using NovinTamas.TaskManager.Domain.Models.Histories;
using NovinTamas.TaskManager.Domain.Models.Notifications;
using NovinTamas.TaskManager.Domain.Models.OutboxMessages;
using NovinTamas.TaskManager.Domain.Models.Tasks;
using System.Text.Json;

namespace NovinTamas.TaskManager.Application.Mapper
{
    // ثبت تاریخچه، ساخت اعلان برای مسئولین و انتشار رویداد؛ سه کاری که تقریباً هر command انجام می‌دهد.
    public class TaskActivityWriter
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IOutboxMessageRepository _outbox;

        public TaskActivityWriter(IUnitOfWork unitOfWork, IUserService userService, IOutboxMessageRepository outbox)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _outbox = outbox;
        }

        public async Task WriteHistoryAsync(
            CurrentUser user,
            string taskId,
            HistoryAction action,
            Dictionary<string, ChangeDetail> changes,
            string? ipAddress = null,
            string? userAgent = null)
        {
            var history = new TaskHistory(user.CompanyId, taskId, user.UserId, action)
            {
                UserName = await _userService.GetDisplayNameAsync(user.CompanyId, user.UserId, user.Role),
                IpAddress = ipAddress == "::1" ? "localhost" : ipAddress,
                UserAgent = userAgent,
                Changes = changes
            };

            await _unitOfWork.HistoryRepository.AddAsync(history);
        }

        public async Task NotifyAsync(
            CurrentUser user,
            TaskItem task,
            IEnumerable<string> receiverUserIds,
            NotificationType type,
            string message)
        {
            // اعلان برای خودِ انجام‌دهنده‌ی عملیات ساخته نمی‌شود
            var receivers = receiverUserIds
                .Where(id => !string.IsNullOrWhiteSpace(id) && id != user.UserId)
                .Distinct()
                .ToList();

            if (receivers.Count == 0) return;

            var senderName = await _userService.GetDisplayNameAsync(user.CompanyId, user.UserId, user.Role);
            var now = DateTime.UtcNow;

            await _unitOfWork.NotificationRepository.AddManyAsync(receivers.Select(receiverId => new TaskNotification
            {
                CompanyId = user.CompanyId,
                TaskId = task.Id!,
                TaskTitle = task.Title,
                SenderUserId = user.UserId,
                SenderName = senderName,
                ReceiverUserId = receiverId,
                Message = message,
                Type = type,
                CreatedAt = now
            }));
        }

        public async Task PublishAsync(string action, string routingKey, object payload)
        {
            var body = JsonSerializer.Serialize(new
            {
                eventId = Guid.NewGuid().ToString(),
                action,
                payload
            });

            await _outbox.AddAsync(new OutboxMessage
            {
                EventType = action,
                RoutingKey = routingKey,
                Body = body,
                CreatedAt = DateTime.UtcNow
            });
        }

        public static Dictionary<string, ChangeDetail> Change(string key, string fieldName, string? oldValue, string? newValue) =>
            new()
            {
                [key] = new ChangeDetail
                {
                    FieldName = fieldName,
                    OldValue = oldValue ?? string.Empty,
                    NewValue = newValue ?? string.Empty
                }
            };
    }
}
