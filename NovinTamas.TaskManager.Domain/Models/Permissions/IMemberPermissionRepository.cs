namespace NovinTamas.TaskManager.Domain.Models.Permissions
{
    public interface IMemberPermissionRepository
    {
        Task<MemberPermission?> GetAsync(string companyId, string userId);
        Task<List<MemberPermission>> GetByCompanyIdAsync(string companyId);
        Task UpsertAsync(MemberPermission permission);
        Task DeleteAsync(string companyId, string userId);
    }
}
