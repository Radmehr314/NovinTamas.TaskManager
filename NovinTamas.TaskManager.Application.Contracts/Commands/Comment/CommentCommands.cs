using NovinTamas.Framework.Application;

namespace NovinTamas.TaskManager.Application.Contracts.Commands.Comment
{
    public class AddTaskCommentCommand : ICommand
    {
        public string TaskId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Files { get; set; } = new();
    }

    public class DeleteTaskCommentCommand : ICommand
    {
        public string Id { get; set; } = string.Empty;
    }
}
