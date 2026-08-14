namespace NovinTamas.TaskManager.Application.Contracts.QueryResult.Project
{
    public class GetProjectQueryResult
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = string.Empty;
        public List<string> MemberIds { get; set; } = new();
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TaskCount { get; set; }
        public int DoneCount { get; set; }
    }
}
