using Microsoft.Extensions.Caching.Memory;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using System.Text.Json.Serialization;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Services
{
    public class UserService : IUserService
    {
        private static readonly TimeSpan MembersCacheTtl = TimeSpan.FromSeconds(60);

        private readonly IHttpClientService _httpClientService;
        private readonly IMemoryCache _cache;

        public UserService(IHttpClientService httpClientService, IMemoryCache cache)
        {
            _httpClientService = httpClientService;
            _cache = cache;
        }

        public async Task<string> GetCompanyNameAsync(string companyId)
        {
            try
            {
                var name = await _httpClientService.PostAsync<object, string>(
                    "userprofile", "/api/Company/GetCompanyName", new { CompanyId = companyId });
                return name ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // لیست پرسنل روی هر بار ساختن کارت و نمایش برد لازم می‌شود؛
        // یک دقیقه cache شدنش تعداد call به UserProfile را به‌شدت کم می‌کند.
        public async Task<List<CompanyMember>> GetCompanyMembersAsync(string companyId)
        {
            var cacheKey = $"members:{companyId}";

            if (_cache.TryGetValue<List<CompanyMember>>(cacheKey, out var cached) && cached != null)
                return cached;

            List<EmployeeDto>? employees = null;

            try
            {
                employees = await _httpClientService.PostAsync<object, List<EmployeeDto>>(
                    "userprofile", "/api/Employee/GetEmployeeByCompanyId", new { CompanyId = companyId });
            }
            catch
            {
                // در دسترس نبودن UserProfile نباید کل صفحه‌ی وظایف را بشکند
            }

            var members = (employees ?? new List<EmployeeDto>())
                .Select(x => new CompanyMember(
                    x.Id,
                    string.Join(" ", new[] { x.FirstName, x.LastName }.Where(p => !string.IsNullOrWhiteSpace(p))).Trim(),
                    x.Username,
                    x.ExtensionNumber,
                    x.IsBan))
                .Select(x => x with { FullName = string.IsNullOrWhiteSpace(x.FullName) ? (x.Username ?? "بدون نام") : x.FullName })
                .ToList();

            _cache.Set(cacheKey, members, MembersCacheTtl);
            return members;
        }

        public async Task<Dictionary<string, string>> GetMemberNamesAsync(string companyId, IEnumerable<string> userIds)
        {
            var ids = userIds.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToHashSet();

            if (ids.Count == 0)
                return new Dictionary<string, string>();

            var members = await GetCompanyMembersAsync(companyId);
            var map = members
                .Where(x => ids.Contains(x.Id))
                .ToDictionary(x => x.Id, x => x.FullName);

            if (ids.Contains(companyId) && !map.ContainsKey(companyId))
                map[companyId] = await GetCompanyNameAsync(companyId);

            return map;
        }

        public async Task<string> GetDisplayNameAsync(string companyId, string userId, string role)
        {
            if (role != "employee")
                return await GetCompanyNameAsync(companyId);

            var members = await GetCompanyMembersAsync(companyId);
            return members.FirstOrDefault(x => x.Id == userId)?.FullName ?? string.Empty;
        }

        private class EmployeeDto
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("firstName")]
            public string? FirstName { get; set; }

            [JsonPropertyName("lastName")]
            public string? LastName { get; set; }

            [JsonPropertyName("username")]
            public string? Username { get; set; }

            [JsonPropertyName("extensionNumber")]
            public string? ExtensionNumber { get; set; }

            [JsonPropertyName("isBan")]
            public bool IsBan { get; set; }
        }
    }
}
