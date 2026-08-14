namespace NovinTamas.TaskManager.Application.Contracts.Contracts
{
    public record CompanyMember(string Id, string FullName, string? Username, string? ExtensionNumber, bool IsBan);

    public interface IUserService
    {
        Task<string> GetCompanyNameAsync(string companyId);
        Task<List<CompanyMember>> GetCompanyMembersAsync(string companyId);
        Task<Dictionary<string, string>> GetMemberNamesAsync(string companyId, IEnumerable<string> userIds);
        Task<string> GetDisplayNameAsync(string companyId, string userId, string role);
    }
}
