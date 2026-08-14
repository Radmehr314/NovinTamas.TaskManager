using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Application.Contracts.QueryResult.Task;
using NovinTamas.TaskManager.Domain.Enums;
using NovinTamas.TaskManager.Domain.Models.Projects;
using NovinTamas.TaskManager.Domain.Models.Tasks;

namespace NovinTamas.TaskManager.Application.Mapper
{
    public static class TaskResultMapper
    {
        public static TaskSummaryResult ToSummary(
            TaskItem task,
            IReadOnlyDictionary<string, string> memberNames,
            IReadOnlyDictionary<string, Project> projects,
            IReadOnlyDictionary<string, int> commentCounts,
            DateTime nowUtc)
        {
            Project? project = null;

            if (!string.IsNullOrWhiteSpace(task.ProjectId))
                projects.TryGetValue(task.ProjectId, out project);

            return new TaskSummaryResult
            {
                Id = task.Id!,
                Code = task.Code,
                Title = task.Title,
                Description = task.Description,
                ProjectId = task.ProjectId,
                ProjectName = project?.Name,
                ProjectColor = project?.Color,
                Status = (int)task.Status,
                StatusName = task.Status.ToPersian(),
                Priority = (int)task.Priority,
                PriorityName = task.Priority.ToPersian(),
                Assignees = task.AssigneeIds
                    .Select(id => new AssigneeSummary
                    {
                        Id = id,
                        FullName = memberNames.TryGetValue(id, out var name) && !string.IsNullOrWhiteSpace(name) ? name : "کاربر حذف‌شده"
                    })
                    .ToList(),
                CreatedByUserId = task.CreatedByUserId,
                CreatedByUserName = task.CreatedByUserName,
                StartDate = task.StartDate,
                DueDate = task.DueDate,
                CompletedAt = task.CompletedAt,
                CreatedAt = task.CreatedAt,
                Progress = task.ResolveProgress(),
                ChecklistTotal = task.Checklist.Count,
                ChecklistDone = task.Checklist.Count(x => x.IsDone),
                CommentCount = commentCounts.TryGetValue(task.Id!, out var count) ? count : 0,
                AttachmentCount = task.Files.Count,
                Labels = task.Labels,
                IsOverdue = task.IsOverdue(nowUtc),
                IsArchived = task.IsArchived,
                BoardOrder = task.BoardOrder
            };
        }

        public static GetTaskByIdQueryResult ToDetail(
            TaskItem task,
            CurrentUser user,
            IReadOnlyDictionary<string, string> memberNames,
            IReadOnlyDictionary<string, Project> projects,
            IReadOnlyDictionary<string, int> commentCounts,
            DateTime nowUtc)
        {
            var summary = ToSummary(task, memberNames, projects, commentCounts, nowUtc);

            return new GetTaskByIdQueryResult
            {
                Id = summary.Id,
                Code = summary.Code,
                Title = summary.Title,
                Description = summary.Description,
                ProjectId = summary.ProjectId,
                ProjectName = summary.ProjectName,
                ProjectColor = summary.ProjectColor,
                Status = summary.Status,
                StatusName = summary.StatusName,
                Priority = summary.Priority,
                PriorityName = summary.PriorityName,
                Assignees = summary.Assignees,
                CreatedByUserId = summary.CreatedByUserId,
                CreatedByUserName = summary.CreatedByUserName,
                StartDate = summary.StartDate,
                DueDate = summary.DueDate,
                CompletedAt = summary.CompletedAt,
                CreatedAt = summary.CreatedAt,
                Progress = summary.Progress,
                ChecklistTotal = summary.ChecklistTotal,
                ChecklistDone = summary.ChecklistDone,
                CommentCount = summary.CommentCount,
                AttachmentCount = summary.AttachmentCount,
                Labels = summary.Labels,
                IsOverdue = summary.IsOverdue,
                IsArchived = summary.IsArchived,
                BoardOrder = summary.BoardOrder,
                EstimatedHours = task.EstimatedHours,
                Files = task.Files,
                Checklist = task.Checklist.Select(x => new ChecklistItemResult
                {
                    Id = x.Id,
                    Title = x.Title,
                    IsDone = x.IsDone,
                    DoneAt = x.DoneAt
                }).ToList(),
                CanEdit = TaskAccess.CanEdit(user, task),
                CanDelete = TaskAccess.CanDelete(user, task)
            };
        }
    }
}
