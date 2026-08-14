namespace NovinTamas.TaskManager.Domain.Models.Projects
{
    public interface IProjectRepository
    {
        Task<Project?> GetByIdAsync(string companyId, string id);
        Task<List<Project>> GetByCompanyIdAsync(string companyId, bool includeArchived = false);
        Task<Dictionary<string, Project>> GetMapByIdsAsync(string companyId, IEnumerable<string> ids);
        Task<string> AddAsync(Project project);
        Task UpdateAsync(Project project);
        Task SetArchivedAsync(string companyId, string id, bool isArchived);
        Task DeleteAsync(string companyId, string id);
    }
}
