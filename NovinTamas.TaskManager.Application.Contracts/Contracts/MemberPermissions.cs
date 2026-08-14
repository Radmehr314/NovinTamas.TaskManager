namespace NovinTamas.TaskManager.Application.Contracts.Contracts
{
    /// <summary>
    /// دسترسی‌های مؤثر کاربر جاری. مدیر شرکت همیشه همه را دارد؛
    /// برای پرسنل از رکورد ذخیره‌شده یا مقادیر پیش‌فرض ساخته می‌شود.
    /// </summary>
    public record MemberPermissions(
        bool CanCreateTask,
        bool CanCreateForOthers,
        bool CanAssignToOthers,
        bool CanViewOthersTasks)
    {
        public static MemberPermissions Owner => new(true, true, true, true);

        public static MemberPermissions Default => new(true, false, false, false);
    }

    public interface IPermissionResolver
    {
        /// <summary>دسترسی‌های کاربر جاری؛ نتیجه در طول یک request کش می‌شود.</summary>
        Task<MemberPermissions> GetCurrentAsync();
    }
}
