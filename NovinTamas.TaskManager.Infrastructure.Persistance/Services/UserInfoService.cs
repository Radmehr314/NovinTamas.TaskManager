using Microsoft.AspNetCore.Http;
using NovinTamas.Framework.Application.Exceptions;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using System.Security.Claims;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Services
{
    public class UserInfoService : IUserInfoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserInfoService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // توکن قبلاً توسط JwtBearer اعتبارسنجی شده، پس اینجا فقط claimها خوانده می‌شوند
        // و دوباره امضا/انقضا چک نمی‌شود.
        public CurrentUser GetCurrentUser()
        {
            var principal = _httpContextAccessor.HttpContext?.User;

            var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? principal?.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                throw new UserAccessException("دسترسی نامعتبر است.");

            var role = principal?.FindFirst(ClaimTypes.Role)?.Value
                       ?? principal?.FindFirst("role")?.Value
                       ?? "user";

            // مدیر شرکت خودش مستاجر است؛ پرسنل شناسه‌ی شرکتش را در claim جدا دارد.
            var companyId = principal?.FindFirst("companyId")?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                companyId = userId;

            return new CurrentUser(userId, role, companyId);
        }
    }
}
