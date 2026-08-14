namespace NovinTamas.TaskManager.Application.Contracts.Messages.Session
{
    // شکل پیامی که IAM موقع revoke شدن یک ActiveSession روی RabbitMQ پابلیش می‌کنه
    public class SessionRevokedEvent
    {
        public string Jti { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
