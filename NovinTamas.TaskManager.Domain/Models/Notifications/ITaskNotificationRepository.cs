using NovinTamas.TaskManager.Domain.Common;

namespace NovinTamas.TaskManager.Domain.Models.Notifications
{
    public interface ITaskNotificationRepository
    {
        Task<string> AddAsync(TaskNotification notification);
        Task AddManyAsync(IEnumerable<TaskNotification> notifications);
        Task<PagedResult<TaskNotification>> GetByUserPagedAsync(string companyId, string userId, QueryOptions options, bool onlyUnread);
        Task<long> GetUnreadCountAsync(string companyId, string userId);
        Task MarkAsReadAsync(string companyId, string userId, List<string> ids);
        Task MarkAllAsReadAsync(string companyId, string userId);
        Task DeleteByTaskIdAsync(string companyId, string taskId);
    }
}
