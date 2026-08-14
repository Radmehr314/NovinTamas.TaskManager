namespace NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments.ProjectMongo
{
    public class ProjectDocument : BaseDocument
    {
        public string CompanyId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = string.Empty;
        public string CreatedByUserId { get; set; } = string.Empty;
        public List<string> MemberIds { get; set; } = new();
        public bool IsArchived { get; set; }
    }
}
