namespace NovinTamas.TaskManager.Domain.Models.Histories
{
    public interface ITaskHistoryRepository
    {
        Task<List<TaskHistory>> GetByTaskIdAsync(string companyId, string taskId);
        Task<List<TaskHistory>> GetRecentByCompanyAsync(string companyId, int limit);
        Task<string> AddAsync(TaskHistory history);
        Task DeleteByTaskIdAsync(string companyId, string taskId);
    }
}
