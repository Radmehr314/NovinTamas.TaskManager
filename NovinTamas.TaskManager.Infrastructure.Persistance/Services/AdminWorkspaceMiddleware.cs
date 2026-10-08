using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Services
{
    /// <summary>
    /// ادمین‌های پنل مدیریت، وظایف را در فضای کاری شرکت خودمان (کد تنظیم‌شده در AdminWorkspace:CompanyCode)
    /// مدیریت می‌کنند؛ این middleware شناسه‌ی آن شرکت را پیدا کرده و برای UserInfoService در درخواست می‌گذارد.
    /// </summary>
    public class AdminWorkspaceMiddleware
    {
        public const string CompanyIdItemKey = "AdminWorkspaceCompanyId";
        private const string RequiredPermission = "manage_tasks";

        private readonly RequestDelegate _next;

        public AdminWorkspaceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration, IMemoryCache cache, IHttpClientService httpClient)
        {
            var principal = context.User;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value ?? principal.FindFirst("role")?.Value;

            if (principal.Identity?.IsAuthenticated != true || role != "admin")
            {
                await _next(context);
                return;
            }

            if (!principal.HasClaim("permission", RequiredPermission))
            {
                await WriteErrorAsync(context, StatusCodes.Status403Forbidden, "ERR_403", "دسترسی مدیریت وظایف ندارید.");
                return;
            }

            var companyCode = configuration["AdminWorkspace:CompanyCode"];
            if (string.IsNullOrWhiteSpace(companyCode))
            {
                await WriteErrorAsync(context, StatusCodes.Status503ServiceUnavailable, "ERR_503", "فضای کاری وظایف ادمین تنظیم نشده است.");
                return;
            }

            var cacheKey = $"admin-workspace:{companyCode}";
            if (!cache.TryGetValue<string>(cacheKey, out var companyId) || string.IsNullOrWhiteSpace(companyId))
            {
                try
                {
                    var companies = await httpClient.PostAsync<object, List<CompanyLookup>>(
                        "userprofile", "/api/Company/Admin/GetCompaniesByCodes", new { Codes = new[] { companyCode } });
                    companyId = companies?.FirstOrDefault(x => x.CompanyCode == companyCode)?.Id;
                }
                catch
                {
                    companyId = null;
                }

                if (string.IsNullOrWhiteSpace(companyId))
                {
                    await WriteErrorAsync(context, StatusCodes.Status503ServiceUnavailable, "ERR_503", $"شرکت با کد {companyCode} یافت نشد.");
                    return;
                }

                cache.Set(cacheKey, companyId, TimeSpan.FromHours(6));
            }

            context.Items[CompanyIdItemKey] = companyId;
            await _next(context);
        }

        private static async Task WriteErrorAsync(HttpContext context, int statusCode, string code, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { ErrorCode = code, ErrorMessage = message }));
        }

        private class CompanyLookup
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("companyCode")]
            public string CompanyCode { get; set; } = string.Empty;
        }
    }
}
