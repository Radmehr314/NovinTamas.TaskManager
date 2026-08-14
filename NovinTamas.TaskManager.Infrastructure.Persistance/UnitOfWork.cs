using NovinTamas.TaskManager.Domain;
using NovinTamas.TaskManager.Domain.Models.Comments;
using NovinTamas.TaskManager.Domain.Models.Histories;
using NovinTamas.TaskManager.Domain.Models.Notifications;
using NovinTamas.TaskManager.Domain.Models.Permissions;
using NovinTamas.TaskManager.Domain.Models.Projects;
using NovinTamas.TaskManager.Domain.Models.Tasks;

namespace NovinTamas.TaskManager.Infrastructure.Persistance
{
    public class UnitOfWork : IUnitOfWork
    {
        public IProjectRepository ProjectRepository { get; }
        public ITaskItemRepository TaskRepository { get; }
        public ITaskCommentRepository CommentRepository { get; }
        public ITaskHistoryRepository HistoryRepository { get; }
        public ITaskNotificationRepository NotificationRepository { get; }
        public IMemberPermissionRepository PermissionRepository { get; }

        public UnitOfWork(
            IProjectRepository projectRepository,
            ITaskItemRepository taskRepository,
            ITaskCommentRepository commentRepository,
            ITaskHistoryRepository historyRepository,
            ITaskNotificationRepository notificationRepository,
            IMemberPermissionRepository permissionRepository)
        {
            ProjectRepository = projectRepository;
            TaskRepository = taskRepository;
            CommentRepository = commentRepository;
            HistoryRepository = historyRepository;
            NotificationRepository = notificationRepository;
            PermissionRepository = permissionRepository;
        }
    }
}
