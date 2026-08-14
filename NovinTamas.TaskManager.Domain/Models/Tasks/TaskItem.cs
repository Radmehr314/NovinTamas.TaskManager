using NovinTamas.TaskManager.Domain.Enums;

namespace NovinTamas.TaskManager.Domain.Models.Tasks
{
    public class TaskItem : BaseEntity<string>
    {
        public TaskItem(string companyId, string title)
        {
            CompanyId = companyId;
            Title = title;
            Status = TaskState.Todo;
            Priority = TaskPriority.Medium;
            CreatedAt = DateTime.UtcNow;
        }

        public string Code { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ProjectId { get; set; }

        public string CreatedByUserId { get; set; } = string.Empty;
        public string CreatedByUserName { get; set; } = string.Empty;
        public string CreatedByRole { get; set; } = string.Empty;

        public List<string> AssigneeIds { get; set; } = new();

        public TaskState Status { get; set; }
        public TaskPriority Priority { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }

        public int Progress { get; set; }
        public double? EstimatedHours { get; set; }

        public List<ChecklistItem> Checklist { get; set; } = new();
        public List<string> Labels { get; set; } = new();
        public List<string> Files { get; set; } = new();

        // جای وظیفه داخل ستون کانبان؛ کوچک‌تر یعنی بالاتر
        public double BoardOrder { get; set; }
        public bool IsArchived { get; set; }

        public bool IsOverdue(DateTime nowUtc) =>
            DueDate.HasValue && DueDate.Value < nowUtc && Status != TaskState.Done && Status != TaskState.Cancelled;

        // درصد پیشرفت وقتی چک‌لیست دارد از روی آیتم‌های انجام‌شده محاسبه می‌شود
        public int ResolveProgress()
        {
            if (Status == TaskState.Done) return 100;
            if (Checklist.Count == 0) return Progress;
            return (int)Math.Round(Checklist.Count(x => x.IsDone) * 100d / Checklist.Count);
        }
    }

    public class ChecklistItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Title { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public string? DoneByUserId { get; set; }
        public DateTime? DoneAt { get; set; }
    }
}
