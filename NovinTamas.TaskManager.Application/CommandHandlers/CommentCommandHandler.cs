using NovinTamas.Framework.Application;
using NovinTamas.Framework.Application.Exceptions;
using NovinTamas.TaskManager.Application.Contracts.Commands.Comment;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Application.Mapper;
using NovinTamas.TaskManager.Domain;
using NovinTamas.TaskManager.Domain.Enums;
using NovinTamas.TaskManager.Domain.Models.Comments;

namespace NovinTamas.TaskManager.Application.CommandHandlers
{
    public class CommentCommandHandler :
        ICommandHandler<AddTaskCommentCommand>,
        ICommandHandler<DeleteTaskCommentCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserInfoService _userInfoService;
        private readonly IUserService _userService;
        private readonly IPermissionResolver _permissions;
        private readonly TaskActivityWriter _activity;

        public CommentCommandHandler(
            IUnitOfWork unitOfWork,
            IUserInfoService userInfoService,
            IUserService userService,
            IPermissionResolver permissions,
            TaskActivityWriter activity)
        {
            _unitOfWork = unitOfWork;
            _userInfoService = userInfoService;
            _userService = userService;
            _permissions = permissions;
            _activity = activity;
        }

        public async Task<CommandResult> Handle(AddTaskCommentCommand command)
        {
            var user = _userInfoService.GetCurrentUser();

            if (string.IsNullOrWhiteSpace(command.Content) && (command.Files?.Count ?? 0) == 0)
                throw new ArgumentException("متن نظر الزامی است.");

            var task = await _unitOfWork.TaskRepository.GetByIdAsync(user.CompanyId, command.TaskId)
                       ?? throw new NotFoundException("وظیفه یافت نشد.");

            TaskAccess.EnsureView(user, await _permissions.GetCurrentAsync(), task);

            var comment = new TaskComment(user.CompanyId, task.Id!, user.UserId, user.Role, command.Content?.Trim() ?? string.Empty)
            {
                UserName = await _userService.GetDisplayNameAsync(user.CompanyId, user.UserId, user.Role),
                Files = command.Files ?? new List<string>()
            };

            var commentId = await _unitOfWork.CommentRepository.AddAsync(comment);

            await _activity.NotifyAsync(user, task, task.AssigneeIds.Append(task.CreatedByUserId),
                NotificationType.CommentAdded, $"نظر جدید روی «{task.Title}»");

            return new CommandResult { Id = commentId };
        }

        public async Task<CommandResult> Handle(DeleteTaskCommentCommand command)
        {
            var user = _userInfoService.GetCurrentUser();

            var comment = await _unitOfWork.CommentRepository.GetByIdAsync(user.CompanyId, command.Id)
                          ?? throw new NotFoundException("نظر یافت نشد.");

            if (!user.IsCompanyOwner && comment.UserId != user.UserId)
                throw new UserAccessException("اجازه‌ی حذف این نظر را ندارید.");

            await _unitOfWork.CommentRepository.DeleteAsync(user.CompanyId, command.Id);
            return new CommandResult { Id = command.Id };
        }
    }
}
