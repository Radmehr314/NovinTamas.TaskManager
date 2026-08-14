using MongoDB.Bson;
using NovinTamas.TaskManager.Domain.Models.Projects;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments.ProjectMongo
{
    public static class ProjectMapper
    {
        public static Project ToDomain(ProjectDocument document)
        {
            if (document == null) return null!;

            return new Project(document.CompanyId, document.Name)
            {
                Id = document.Id.ToString(),
                Description = document.Description,
                Color = document.Color,
                CreatedByUserId = document.CreatedByUserId,
                MemberIds = document.MemberIds ?? new List<string>(),
                IsArchived = document.IsArchived,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt
            };
        }

        public static ProjectDocument ToDocument(Project domain)
        {
            if (domain == null) return null!;

            var objectId = string.IsNullOrWhiteSpace(domain.Id)
                ? ObjectId.GenerateNewId()
                : ObjectId.Parse(domain.Id);

            return new ProjectDocument
            {
                Id = objectId,
                CompanyId = domain.CompanyId,
                Name = domain.Name,
                Description = domain.Description,
                Color = domain.Color,
                CreatedByUserId = domain.CreatedByUserId,
                MemberIds = domain.MemberIds ?? new List<string>(),
                IsArchived = domain.IsArchived,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }
    }
}
