using NovinTamas.TaskManager.Domain.Enums;

namespace NovinTamas.TaskManager.Domain.Models.Notifications
{
    public class TaskNotification : BaseEntity<string>
    {
        public string CompanyId { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string SenderUserId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string ReceiverUserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
