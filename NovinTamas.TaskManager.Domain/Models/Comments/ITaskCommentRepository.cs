namespace NovinTamas.TaskManager.Domain.Models.Comments
{
    public interface ITaskCommentRepository
    {
        Task<TaskComment?> GetByIdAsync(string companyId, string id);
        Task<List<TaskComment>> GetByTaskIdAsync(string companyId, string taskId);
        Task<Dictionary<string, int>> GetCountsByTaskIdsAsync(string companyId, IEnumerable<string> taskIds);
        Task<string> AddAsync(TaskComment comment);
        Task UpdateAsync(TaskComment comment);
        Task DeleteAsync(string companyId, string id);
        Task DeleteByTaskIdAsync(string companyId, string taskId);
    }
}
