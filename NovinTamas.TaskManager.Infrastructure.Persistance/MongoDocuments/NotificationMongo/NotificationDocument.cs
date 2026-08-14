using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NovinTamas.TaskManager.Domain.Enums;
using NovinTamas.TaskManager.Domain.Models.Notifications;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments.NotificationMongo
{
    public class NotificationDocument : BaseDocument
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
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? ReadAt { get; set; }
    }

    public static class NotificationMapper
    {
        public static TaskNotification ToDomain(NotificationDocument document)
        {
            if (document == null) return null!;

            return new TaskNotification
            {
                Id = document.Id.ToString(),
                CompanyId = document.CompanyId,
                TaskId = document.TaskId,
                TaskTitle = document.TaskTitle,
                SenderUserId = document.SenderUserId,
                SenderName = document.SenderName,
                ReceiverUserId = document.ReceiverUserId,
                Message = document.Message,
                Type = document.Type,
                IsRead = document.IsRead,
                ReadAt = document.ReadAt,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt
            };
        }

        public static NotificationDocument ToDocument(TaskNotification domain)
        {
            if (domain == null) return null!;

            var objectId = string.IsNullOrWhiteSpace(domain.Id)
                ? ObjectId.GenerateNewId()
                : ObjectId.Parse(domain.Id);

            return new NotificationDocument
            {
                Id = objectId,
                CompanyId = domain.CompanyId,
                TaskId = domain.TaskId,
                TaskTitle = domain.TaskTitle,
                SenderUserId = domain.SenderUserId,
                SenderName = domain.SenderName,
                ReceiverUserId = domain.ReceiverUserId,
                Message = domain.Message,
                Type = domain.Type,
                IsRead = domain.IsRead,
                ReadAt = domain.ReadAt,
                CreatedAt = domain.CreatedAt == default ? DateTime.UtcNow : domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }
    }
}
