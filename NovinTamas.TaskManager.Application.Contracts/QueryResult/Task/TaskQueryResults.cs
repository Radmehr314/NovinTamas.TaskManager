namespace NovinTamas.TaskManager.Application.Contracts.QueryResult.Task
{
    public class AssigneeSummary
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    public class ChecklistItemResult
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public DateTime? DoneAt { get; set; }
    }

    // مدل مشترک کارت وظیفه در لیست و برد
    public class TaskSummaryResult
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? ProjectColor { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string PriorityName { get; set; } = string.Empty;
        public List<AssigneeSummary> Assignees { get; set; } = new();
        public string CreatedByUserId { get; set; } = string.Empty;
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Progress { get; set; }
        public int ChecklistTotal { get; set; }
        public int ChecklistDone { get; set; }
        public int CommentCount { get; set; }
        public int AttachmentCount { get; set; }
        public List<string> Labels { get; set; } = new();
        public bool IsOverdue { get; set; }
        public bool IsArchived { get; set; }
        public double BoardOrder { get; set; }
    }

    public class GetTaskByIdQueryResult : TaskSummaryResult
    {
        public List<ChecklistItemResult> Checklist { get; set; } = new();
        public List<string> Files { get; set; } = new();
        public double? EstimatedHours { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public class BoardColumnResult
    {
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public List<TaskSummaryResult> Items { get; set; } = new();
    }

    public class GetTaskBoardQueryResult
    {
        public List<BoardColumnResult> Columns { get; set; } = new();
    }

    public class GetTaskHistoryQueryResult
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int Action { get; set; }
        public string ActionName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, ChangeDetailResult> Changes { get; set; } = new();
    }

    public class ChangeDetailResult
    {
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class GetTaskStatisticsQueryResult
    {
        public int TotalCount { get; set; }
        public int TodoCount { get; set; }
        public int InProgressCount { get; set; }
        public int InReviewCount { get; set; }
        public int DoneCount { get; set; }
        public int CancelledCount { get; set; }
        public int OverdueCount { get; set; }
        public int DueTodayCount { get; set; }
        public int UnassignedCount { get; set; }
        public int CompletedThisWeekCount { get; set; }
        public int CompletionRate { get; set; }
        public int LowCount { get; set; }
        public int MediumCount { get; set; }
        public int HighCount { get; set; }
        public int UrgentCount { get; set; }
    }

    public class MemberWorkloadResult
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int OpenCount { get; set; }
        public int DoneCount { get; set; }
        public int OverdueCount { get; set; }
    }

    public class ConfigItemResult
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class GetTaskConfigQueryResult
    {
        public List<ConfigItemResult> Status { get; set; } = new();
        public List<ConfigItemResult> Priority { get; set; } = new();
        public List<AssigneeSummary> Members { get; set; } = new();
        public List<string> Labels { get; set; } = new();
        public bool IsCompanyOwner { get; set; }
        public string CurrentUserId { get; set; } = string.Empty;

        // شناسه‌ی مدیر شرکت تا کلاینت بتواند او را در لیست مسئولین تشخیص دهد
        public string ManagerUserId { get; set; } = string.Empty;

        // دسترسی‌های مؤثر کاربر جاری تا کلاینت بتواند دکمه‌های غیرمجاز را نشان ندهد؛
        // اعمال واقعی همچنان در سرور انجام می‌شود.
        public bool CanCreateTask { get; set; }
        public bool CanCreateForOthers { get; set; }
        public bool CanAssignToOthers { get; set; }
        public bool CanViewOthersTasks { get; set; }
        public bool CanAssignToManager { get; set; }
    }
}
