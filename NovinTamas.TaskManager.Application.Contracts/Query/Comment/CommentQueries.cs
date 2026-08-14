using NovinTamas.Framework.Application;

namespace NovinTamas.TaskManager.Application.Contracts.Query.Comment
{
    public class GetTaskCommentsQuery : IQuery
    {
        public string TaskId { get; set; } = string.Empty;
    }
}
