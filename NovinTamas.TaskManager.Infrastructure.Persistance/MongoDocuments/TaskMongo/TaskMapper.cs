using MongoDB.Bson;
using NovinTamas.TaskManager.Domain.Models.Tasks;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments.TaskMongo
{
    public static class TaskMapper
    {
        public static TaskItem ToDomain(TaskDocument document)
        {
            if (document == null) return null!;

            return new TaskItem(document.CompanyId, document.Title)
            {
                Id = document.Id.ToString(),
                Code = document.Code,
                Description = document.Description,
                ProjectId = document.ProjectId,
                CreatedByUserId = document.CreatedByUserId,
                CreatedByUserName = document.CreatedByUserName,
                CreatedByRole = document.CreatedByRole,
                AssigneeIds = document.AssigneeIds ?? new List<string>(),
                Status = document.Status,
                Priority = document.Priority,
                StartDate = document.StartDate,
                DueDate = document.DueDate,
                CompletedAt = document.CompletedAt,
                Progress = document.Progress,
                EstimatedHours = document.EstimatedHours,
                Checklist = (document.Checklist ?? new List<ChecklistItemDocument>())
                    .Select(x => new ChecklistItem
                    {
                        Id = x.Id,
                        Title = x.Title,
                        IsDone = x.IsDone,
                        DoneByUserId = x.DoneByUserId,
                        DoneAt = x.DoneAt
                    }).ToList(),
                Labels = document.Labels ?? new List<string>(),
                Files = document.Files ?? new List<string>(),
                BoardOrder = document.BoardOrder,
                IsArchived = document.IsArchived,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt
            };
        }

        public static TaskDocument ToDocument(TaskItem domain)
        {
            if (domain == null) return null!;

            var objectId = string.IsNullOrWhiteSpace(domain.Id)
                ? ObjectId.GenerateNewId()
                : ObjectId.Parse(domain.Id);

            return new TaskDocument
            {
                Id = objectId,
                Code = domain.Code,
                CompanyId = domain.CompanyId,
                Title = domain.Title,
                Description = domain.Description,
                ProjectId = domain.ProjectId,
                CreatedByUserId = domain.CreatedByUserId,
                CreatedByUserName = domain.CreatedByUserName,
                CreatedByRole = domain.CreatedByRole,
                AssigneeIds = domain.AssigneeIds ?? new List<string>(),
                Status = domain.Status,
                Priority = domain.Priority,
                StartDate = domain.StartDate,
                DueDate = domain.DueDate,
                CompletedAt = domain.CompletedAt,
                Progress = domain.Progress,
                EstimatedHours = domain.EstimatedHours,
                Checklist = (domain.Checklist ?? new List<ChecklistItem>())
                    .Select(x => new ChecklistItemDocument
                    {
                        Id = x.Id,
                        Title = x.Title,
                        IsDone = x.IsDone,
                        DoneByUserId = x.DoneByUserId,
                        DoneAt = x.DoneAt
                    }).ToList(),
                Labels = domain.Labels ?? new List<string>(),
                Files = domain.Files ?? new List<string>(),
                BoardOrder = domain.BoardOrder,
                IsArchived = domain.IsArchived,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }
    }
}
