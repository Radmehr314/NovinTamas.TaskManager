using MongoDB.Bson.Serialization.Attributes;
using NovinTamas.TaskManager.Domain.Enums;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments.TaskMongo
{
    public class TaskDocument : BaseDocument
    {
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

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? StartDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DueDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? CompletedAt { get; set; }

        public int Progress { get; set; }
        public double? EstimatedHours { get; set; }

        public List<ChecklistItemDocument> Checklist { get; set; } = new();
        public List<string> Labels { get; set; } = new();
        public List<string> Files { get; set; } = new();

        public double BoardOrder { get; set; }
        public bool IsArchived { get; set; }

        // فقط برای dedup یادآوری مهلت در DueDateReminderService استفاده می‌شود و به دامنه راه ندارد
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? LastReminderAt { get; set; }
    }

    public class ChecklistItemDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public string? DoneByUserId { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DoneAt { get; set; }
    }
}
