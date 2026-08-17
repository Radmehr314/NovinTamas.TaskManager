using MongoDB.Bson;
using NovinTamas.TaskManager.Domain.Models.Permissions;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments.PermissionMongo
{
    public class PermissionDocument : BaseDocument
    {
        public string CompanyId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public bool CanCreateTask { get; set; }
        public bool CanCreateForOthers { get; set; }
        public bool CanAssignToOthers { get; set; }
        public bool CanViewOthersTasks { get; set; }
        public bool CanAssignToManager { get; set; }
    }

    public static class PermissionMapper
    {
        public static MemberPermission ToDomain(PermissionDocument document)
        {
            if (document == null) return null!;

            return new MemberPermission
            {
                Id = document.Id.ToString(),
                CompanyId = document.CompanyId,
                UserId = document.UserId,
                CanCreateTask = document.CanCreateTask,
                CanCreateForOthers = document.CanCreateForOthers,
                CanAssignToOthers = document.CanAssignToOthers,
                CanViewOthersTasks = document.CanViewOthersTasks,
                CanAssignToManager = document.CanAssignToManager,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt
            };
        }

        public static PermissionDocument ToDocument(MemberPermission domain)
        {
            if (domain == null) return null!;

            var objectId = string.IsNullOrWhiteSpace(domain.Id)
                ? ObjectId.GenerateNewId()
                : ObjectId.Parse(domain.Id);

            return new PermissionDocument
            {
                Id = objectId,
                CompanyId = domain.CompanyId,
                UserId = domain.UserId,
                CanCreateTask = domain.CanCreateTask,
                CanCreateForOthers = domain.CanCreateForOthers,
                CanAssignToOthers = domain.CanAssignToOthers,
                CanViewOthersTasks = domain.CanViewOthersTasks,
                CanAssignToManager = domain.CanAssignToManager,
                CreatedAt = domain.CreatedAt == default ? DateTime.UtcNow : domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }
    }
}
