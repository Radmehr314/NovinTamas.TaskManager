namespace NovinTamas.TaskManager.Application.Contracts.Contracts
{
    // هویت درخواست جاری. companyId مرز چندمستاجری سرویس است:
    // برای مدیر شرکت (role=user) خودِ userId است و برای پرسنل (role=employee) از claim جدا می‌آید.
    public record CurrentUser(string UserId, string Role, string CompanyId)
    {
        public bool IsCompanyOwner => Role == "user" || Role == "admin";
        public bool IsEmployee => Role == "employee";
    }

    public interface IUserInfoService
    {
        CurrentUser GetCurrentUser();
    }
}
