namespace NovinTamas.TaskManager.Domain.Models.Comments
{
    public class TaskComment : BaseEntity<string>
    {
        public TaskComment(string companyId, string taskId, string userId, string role, string content)
        {
            CompanyId = companyId;
            TaskId = taskId;
            UserId = userId;
            Role = role;
            Content = content;
            CreatedAt = DateTime.UtcNow;
        }

        public string CompanyId { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Files { get; set; } = new();
    }
}
