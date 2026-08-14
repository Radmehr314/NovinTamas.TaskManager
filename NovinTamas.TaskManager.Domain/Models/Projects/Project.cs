namespace NovinTamas.TaskManager.Domain.Models.Projects
{
    public class Project : BaseEntity<string>
    {
        public Project(string companyId, string name)
        {
            CompanyId = companyId;
            Name = name;
            CreatedAt = DateTime.UtcNow;
        }

        public string CompanyId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#5A6ACF";
        public string CreatedByUserId { get; set; } = string.Empty;
        public List<string> MemberIds { get; set; } = new();
        public bool IsArchived { get; set; }
    }
}
