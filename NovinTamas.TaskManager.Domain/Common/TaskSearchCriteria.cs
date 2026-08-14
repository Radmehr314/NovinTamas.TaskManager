using NovinTamas.TaskManager.Domain.Enums;

namespace NovinTamas.TaskManager.Domain.Common
{
    public class TaskSearchCriteria
    {
        public TaskState? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public string? ProjectId { get; set; }
        public string? AssigneeId { get; set; }
        public string? CreatedByUserId { get; set; }
        public bool? IsAssigned { get; set; }
        public bool OnlyAssignedToMe { get; set; }
        public bool OnlyOverdue { get; set; }
        public bool IncludeArchived { get; set; }
        public DateTime? DueFrom { get; set; }
        public DateTime? DueTo { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public List<string>? Labels { get; set; }
        public string? SearchText { get; set; }
    }
}
