namespace NovinTamas.TaskManager.Application.Contracts.QueryResult.Notification
{
    public class GetNotificationQueryResult
    {
        public string Id { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GetUnreadNotificationsCountQueryResult
    {
        public long Count { get; set; }
    }
}
