using NovinTamas.Framework.Application.Exceptions;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Domain.Models.Tasks;

namespace NovinTamas.TaskManager.Application.Mapper
{
    // قواعد دسترسی در یک جا جمع شده تا هر handler دوباره اختراعشان نکند.
    public static class TaskAccess
    {
        /// <summary>مدیر شرکت همه را می‌بیند؛ پرسنل فقط وظایف خودش، مگر دسترسی «دیدن وظایف دیگران» داشته باشد.</summary>
        public static bool CanView(CurrentUser user, MemberPermissions perms, TaskItem task) =>
            user.IsCompanyOwner
            || perms.CanViewOthersTasks
            || task.AssigneeIds.Contains(user.UserId)
            || task.CreatedByUserId == user.UserId;

        /// <summary>ویرایش محتوای وظیفه (عنوان، مهلت، پروژه) فقط برای مدیر یا سازنده‌ی وظیفه.</summary>
        public static bool CanEdit(CurrentUser user, TaskItem task) =>
            user.IsCompanyOwner || task.CreatedByUserId == user.UserId;

        /// <summary>تغییر مسئولین؛ علاوه بر سازنده، پرسنلِ دارای دسترسی ارجاع هم می‌تواند.</summary>
        public static bool CanAssign(CurrentUser user, MemberPermissions perms, TaskItem task) =>
            CanEdit(user, task) || (perms.CanAssignToOthers && CanView(user, perms, task));

        public static bool CanDelete(CurrentUser user, TaskItem task) =>
            user.IsCompanyOwner || task.CreatedByUserId == user.UserId;

        public static void EnsureView(CurrentUser user, MemberPermissions perms, TaskItem task)
        {
            if (!CanView(user, perms, task))
                throw new UserAccessException("به این وظیفه دسترسی ندارید.");
        }

        public static void EnsureEdit(CurrentUser user, TaskItem task)
        {
            if (!CanEdit(user, task))
                throw new UserAccessException("اجازه‌ی ویرایش این وظیفه را ندارید.");
        }

        public static void EnsureAssign(CurrentUser user, MemberPermissions perms, TaskItem task)
        {
            if (!CanAssign(user, perms, task))
                throw new UserAccessException("اجازه‌ی ارجاع این وظیفه را ندارید.");
        }

        public static void EnsureDelete(CurrentUser user, TaskItem task)
        {
            if (!CanDelete(user, task))
                throw new UserAccessException("اجازه‌ی حذف این وظیفه را ندارید.");
        }

        public static void EnsureCompanyOwner(CurrentUser user)
        {
            if (!user.IsCompanyOwner)
                throw new UserAccessException("این عملیات فقط برای مدیر شرکت مجاز است.");
        }
    }
}
