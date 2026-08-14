namespace NovinTamas.TaskManager.Domain.Models.Permissions
{
    /// <summary>
    /// دسترسی‌های یک پرسنل در ماژول وظایف. نبودِ رکورد یعنی مقادیر پیش‌فرض
    /// (فقط ساخت وظیفه برای خودش)، پس لازم نیست برای هر کارمند رکورد ساخته شود.
    /// </summary>
    public class MemberPermission : BaseEntity<string>
    {
        public const bool DefaultCanCreateTask = true;
        public const bool DefaultCanCreateForOthers = false;
        public const bool DefaultCanAssignToOthers = false;
        public const bool DefaultCanViewOthersTasks = false;

        public string CompanyId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public bool CanCreateTask { get; set; } = DefaultCanCreateTask;
        public bool CanCreateForOthers { get; set; } = DefaultCanCreateForOthers;
        public bool CanAssignToOthers { get; set; } = DefaultCanAssignToOthers;
        public bool CanViewOthersTasks { get; set; } = DefaultCanViewOthersTasks;
    }
}
