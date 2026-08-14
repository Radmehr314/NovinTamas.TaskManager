using MongoDB.Bson;
using NovinTamas.TaskManager.Domain.Enums;
using NovinTamas.TaskManager.Domain.Models.Histories;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments.HistoryMongo
{
    public class HistoryDocument : BaseDocument
    {
        public string CompanyId { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public HistoryAction Action { get; set; }
        public Dictionary<string, ChangeDetailDocument> Changes { get; set; } = new();
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class ChangeDetailDocument
    {
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
    }

    public static class HistoryMapper
    {
        public static TaskHistory ToDomain(HistoryDocument document)
        {
            if (document == null) return null!;

            return new TaskHistory(document.CompanyId, document.TaskId, document.UserId, document.Action)
            {
                Id = document.Id.ToString(),
                UserName = document.UserName,
                IpAddress = document.IpAddress,
                UserAgent = document.UserAgent,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                Changes = (document.Changes ?? new Dictionary<string, ChangeDetailDocument>())
                    .ToDictionary(x => x.Key, x => new ChangeDetail
                    {
                        OldValue = x.Value.OldValue,
                        NewValue = x.Value.NewValue,
                        FieldName = x.Value.FieldName
                    })
            };
        }

        public static HistoryDocument ToDocument(TaskHistory domain)
        {
            if (domain == null) return null!;

            var objectId = string.IsNullOrWhiteSpace(domain.Id)
                ? ObjectId.GenerateNewId()
                : ObjectId.Parse(domain.Id);

            return new HistoryDocument
            {
                Id = objectId,
                CompanyId = domain.CompanyId,
                TaskId = domain.TaskId,
                UserId = domain.UserId,
                UserName = domain.UserName,
                Action = domain.Action,
                IpAddress = domain.IpAddress,
                UserAgent = domain.UserAgent,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt,
                Changes = (domain.Changes ?? new Dictionary<string, ChangeDetail>())
                    .ToDictionary(x => x.Key, x => new ChangeDetailDocument
                    {
                        OldValue = x.Value.OldValue,
                        NewValue = x.Value.NewValue,
                        FieldName = x.Value.FieldName
                    })
            };
        }
    }
}
