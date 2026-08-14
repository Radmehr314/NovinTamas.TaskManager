using NovinTamas.TaskManager.Domain.Models.Comments;
using NovinTamas.TaskManager.Domain.Models.Histories;
using NovinTamas.TaskManager.Domain.Models.Notifications;
using NovinTamas.TaskManager.Domain.Models.Permissions;
using NovinTamas.TaskManager.Domain.Models.Projects;
using NovinTamas.TaskManager.Domain.Models.Tasks;

namespace NovinTamas.TaskManager.Domain
{
    public interface IUnitOfWork
    {
        IProjectRepository ProjectRepository { get; }
        ITaskItemRepository TaskRepository { get; }
        ITaskCommentRepository CommentRepository { get; }
        ITaskHistoryRepository HistoryRepository { get; }
        ITaskNotificationRepository NotificationRepository { get; }
        IMemberPermissionRepository PermissionRepository { get; }
    }
}
