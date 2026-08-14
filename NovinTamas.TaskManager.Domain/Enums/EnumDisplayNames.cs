namespace NovinTamas.TaskManager.Domain.Enums;

public static class EnumDisplayNames
{
    public static string ToPersian(this TaskState state) => state switch
    {
        TaskState.Todo => "در انتظار شروع",
        TaskState.InProgress => "در حال انجام",
        TaskState.InReview => "در انتظار بررسی",
        TaskState.Done => "انجام شده",
        TaskState.Cancelled => "لغو شده",
        _ => state.ToString()
    };

    public static string ToPersian(this TaskPriority priority) => priority switch
    {
        TaskPriority.Low => "کم",
        TaskPriority.Medium => "متوسط",
        TaskPriority.High => "زیاد",
        TaskPriority.Urgent => "فوری",
        _ => priority.ToString()
    };

    public static string ToPersian(this HistoryAction action) => action switch
    {
        HistoryAction.Created => "ایجاد وظیفه",
        HistoryAction.Updated => "ویرایش وظیفه",
        HistoryAction.StatusChanged => "تغییر وضعیت",
        HistoryAction.Assigned => "تغییر مسئولین",
        HistoryAction.CommentAdded => "ثبت نظر",
        HistoryAction.PriorityChanged => "تغییر اولویت",
        HistoryAction.DueDateChanged => "تغییر مهلت انجام",
        HistoryAction.ChecklistChanged => "تغییر چک‌لیست",
        HistoryAction.ProgressChanged => "تغییر پیشرفت",
        HistoryAction.Archived => "بایگانی",
        HistoryAction.Restored => "خروج از بایگانی",
        HistoryAction.Deleted => "حذف وظیفه",
        HistoryAction.AttachmentAdded => "افزودن پیوست",
        _ => action.ToString()
    };

    public static string ToPersian(this NotificationType type) => type switch
    {
        NotificationType.TaskAssigned => "ارجاع وظیفه",
        NotificationType.TaskUnassigned => "حذف از وظیفه",
        NotificationType.StatusChanged => "تغییر وضعیت",
        NotificationType.PriorityChanged => "تغییر اولویت",
        NotificationType.CommentAdded => "نظر جدید",
        NotificationType.DueDateChanged => "تغییر مهلت",
        NotificationType.TaskCompleted => "تکمیل وظیفه",
        NotificationType.TaskDueSoon => "نزدیک شدن مهلت",
        NotificationType.TaskOverdue => "گذشتن مهلت",
        NotificationType.Mentioned => "اشاره به شما",
        _ => type.ToString()
    };
}
