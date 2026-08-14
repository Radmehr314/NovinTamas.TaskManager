namespace NovinTamas.TaskManager.Domain.Common
{
    public class TaskStatistics
    {
        public int TotalCount { get; set; }
        public int TodoCount { get; set; }
        public int InProgressCount { get; set; }
        public int InReviewCount { get; set; }
        public int DoneCount { get; set; }
        public int CancelledCount { get; set; }
        public int OverdueCount { get; set; }
        public int DueTodayCount { get; set; }
        public int UnassignedCount { get; set; }
        public int CompletedThisWeekCount { get; set; }
    }

    public class TaskPriorityStatistics
    {
        public int LowCount { get; set; }
        public int MediumCount { get; set; }
        public int HighCount { get; set; }
        public int UrgentCount { get; set; }
    }

    // آمار هر پرسنل برای صفحه‌ی «بار کاری تیم» در پنل مدیر
    public class MemberWorkload
    {
        public string UserId { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int OpenCount { get; set; }
        public int DoneCount { get; set; }
        public int OverdueCount { get; set; }
    }
}
