using NovinTamas.TaskManager.Domain.Common;
using NovinTamas.TaskManager.Domain.Enums;

namespace NovinTamas.TaskManager.Domain.Models.Tasks
{
    public interface ITaskItemRepository
    {
        Task<TaskItem?> GetByIdAsync(string companyId, string id);
        Task<string> AddAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(string companyId, string id);

        Task<PagedResult<TaskItem>> GetPagedAsync(string companyId, QueryOptions options, TaskSearchCriteria? criteria, string? currentUserId);
        Task<List<TaskItem>> GetBoardAsync(string companyId, TaskSearchCriteria? criteria, string? currentUserId, int perColumnLimit);

        Task ChangeStatusAsync(string companyId, string id, TaskState status, double? boardOrder, DateTime? completedAt);
        Task ChangePriorityAsync(string companyId, string id, TaskPriority priority);
        Task ChangeAssigneesAsync(string companyId, string id, List<string> assigneeIds);
        Task SetArchivedAsync(string companyId, string id, bool isArchived);
        Task SetChecklistAsync(string companyId, string id, List<ChecklistItem> checklist, int progress);

        Task<double> GetNextBoardOrderAsync(string companyId, TaskState status);
        Task<TaskStatistics> GetStatisticsAsync(string companyId, string? assigneeId);
        Task<TaskPriorityStatistics> GetPriorityStatisticsAsync(string companyId, string? assigneeId);
        Task<List<MemberWorkload>> GetWorkloadAsync(string companyId);
        Task<List<TaskItem>> GetUpcomingAsync(string companyId, string? assigneeId, int days, int limit);
        Task<int> CountByProjectAsync(string companyId, string projectId);
    }
}
