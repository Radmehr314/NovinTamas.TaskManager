using NovinTamas.Framework.Application;

namespace NovinTamas.TaskManager.Application.Contracts.Commands.Project
{
    public class CreateProjectCommand : ICommand
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public List<string> MemberIds { get; set; } = new();
    }

    public class UpdateProjectCommand : ICommand
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public List<string> MemberIds { get; set; } = new();
    }

    public class ArchiveProjectCommand : ICommand
    {
        public string Id { get; set; } = string.Empty;
        public bool IsArchived { get; set; } = true;
    }

    public class DeleteProjectCommand : ICommand
    {
        public string Id { get; set; } = string.Empty;
    }
}
