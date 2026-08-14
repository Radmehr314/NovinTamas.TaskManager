using NovinTamas.TaskManager.Domain.Enums;

namespace NovinTamas.TaskManager.Domain.Models.Histories
{
    public class TaskHistory : BaseEntity<string>
    {
        public TaskHistory(string companyId, string taskId, string userId, HistoryAction action)
        {
            CompanyId = companyId;
            TaskId = taskId;
            UserId = userId;
            Action = action;
            CreatedAt = DateTime.UtcNow;
        }

        public string CompanyId { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public HistoryAction Action { get; set; }
        public Dictionary<string, ChangeDetail> Changes { get; set; } = new();
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class ChangeDetail
    {
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
    }
}
