using MongoDB.Bson;
using NovinTamas.TaskManager.Domain.Models.Comments;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments.CommentMongo
{
    public class CommentDocument : BaseDocument
    {
        public string CompanyId { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Files { get; set; } = new();
    }

    public static class CommentMapper
    {
        public static TaskComment ToDomain(CommentDocument document)
        {
            if (document == null) return null!;

            return new TaskComment(document.CompanyId, document.TaskId, document.UserId, document.Role, document.Content)
            {
                Id = document.Id.ToString(),
                UserName = document.UserName,
                Files = document.Files ?? new List<string>(),
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt
            };
        }

        public static CommentDocument ToDocument(TaskComment domain)
        {
            if (domain == null) return null!;

            var objectId = string.IsNullOrWhiteSpace(domain.Id)
                ? ObjectId.GenerateNewId()
                : ObjectId.Parse(domain.Id);

            return new CommentDocument
            {
                Id = objectId,
                CompanyId = domain.CompanyId,
                TaskId = domain.TaskId,
                UserId = domain.UserId,
                UserName = domain.UserName,
                Role = domain.Role,
                Content = domain.Content,
                Files = domain.Files ?? new List<string>(),
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }
    }
}
