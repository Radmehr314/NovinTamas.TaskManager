using NovinTamas.Framework.Application;

namespace NovinTamas.TaskManager.Application.Contracts.Query.Project
{
    public class GetProjectsQuery : IQuery
    {
        public bool IncludeArchived { get; set; }
    }

    public class GetProjectByIdQuery : IQuery
    {
        public string Id { get; set; } = string.Empty;
    }
}
