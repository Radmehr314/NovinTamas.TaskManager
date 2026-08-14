using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Application.Contracts.Commands.Notification;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Domain;

namespace NovinTamas.TaskManager.Application.CommandHandlers
{
    public class NotificationCommandHandler : ICommandHandler<MarkNotificationsAsReadCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserInfoService _userInfoService;

        public NotificationCommandHandler(IUnitOfWork unitOfWork, IUserInfoService userInfoService)
        {
            _unitOfWork = unitOfWork;
            _userInfoService = userInfoService;
        }

        public async Task<CommandResult> Handle(MarkNotificationsAsReadCommand command)
        {
            var user = _userInfoService.GetCurrentUser();

            if (command.All || command.Ids.Count == 0)
                await _unitOfWork.NotificationRepository.MarkAllAsReadAsync(user.CompanyId, user.UserId);
            else
                await _unitOfWork.NotificationRepository.MarkAsReadAsync(user.CompanyId, user.UserId, command.Ids);

            return new CommandResult();
        }
    }
}
