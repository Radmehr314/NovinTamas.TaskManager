using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Domain.Enums;

namespace NovinTamas.TaskManager.Application.Contracts.Commands.Task
{
    public abstract class AuditedCommand : ICommand
    {
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class CreateTaskCommand : AuditedCommand
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ProjectId { get; set; }
        public List<string> AssigneeIds { get; set; } = new();
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public TaskState? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public double? EstimatedHours { get; set; }
        public List<string> Labels { get; set; } = new();
        public List<string> Files { get; set; } = new();
        public List<string> ChecklistTitles { get; set; } = new();
    }

    public class UpdateTaskCommand : AuditedCommand
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ProjectId { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public double? EstimatedHours { get; set; }
        public List<string> Labels { get; set; } = new();
        public List<string> Files { get; set; } = new();
    }

    public class ChangeTaskStatusCommand : AuditedCommand
    {
        public string Id { get; set; } = string.Empty;
        public TaskState Status { get; set; }
        // برای drag & drop روی برد: شناسه‌ی وظیفه‌ی بالایی و پایینیِ محل رها شدن
        public string? BeforeTaskId { get; set; }
        public string? AfterTaskId { get; set; }
    }

    public class ChangeTaskPriorityCommand : AuditedCommand
    {
        public string Id { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; }
    }

    public class AssignTaskCommand : AuditedCommand
    {
        public string Id { get; set; } = string.Empty;
        public List<string> AssigneeIds { get; set; } = new();
    }

    public class SetTaskProgressCommand : AuditedCommand
    {
        public string Id { get; set; } = string.Empty;
        public int Progress { get; set; }
    }

    public class AddChecklistItemCommand : AuditedCommand
    {
        public string TaskId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    public class ToggleChecklistItemCommand : AuditedCommand
    {
        public string TaskId { get; set; } = string.Empty;
        public string ItemId { get; set; } = string.Empty;
        public bool IsDone { get; set; }
    }

    public class DeleteChecklistItemCommand : AuditedCommand
    {
        public string TaskId { get; set; } = string.Empty;
        public string ItemId { get; set; } = string.Empty;
    }

    public class ArchiveTaskCommand : AuditedCommand
    {
        public string Id { get; set; } = string.Empty;
        public bool IsArchived { get; set; } = true;
    }

    public class DeleteTaskCommand : AuditedCommand
    {
        public string Id { get; set; } = string.Empty;
    }
}
