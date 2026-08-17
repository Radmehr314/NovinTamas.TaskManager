using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Application.Contracts.Commands.Permission;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Application.Mapper;
using NovinTamas.TaskManager.Domain;
using NovinTamas.TaskManager.Domain.Models.Permissions;

namespace NovinTamas.TaskManager.Application.CommandHandlers
{
    public class PermissionCommandHandler : ICommandHandler<SetMemberPermissionsCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserInfoService _userInfoService;
        private readonly IUserService _userService;

        public PermissionCommandHandler(IUnitOfWork unitOfWork, IUserInfoService userInfoService, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userInfoService = userInfoService;
            _userService = userService;
        }

        public async Task<CommandResult> Handle(SetMemberPermissionsCommand command)
        {
            var user = _userInfoService.GetCurrentUser();
            TaskAccess.EnsureCompanyOwner(user);

            if (string.IsNullOrWhiteSpace(command.UserId))
                throw new ArgumentException("شناسه‌ی پرسنل الزامی است.");

            // فقط پرسنل همین شرکت؛ وگرنه مدیر می‌توانست برای کاربر شرکت دیگری رکورد بسازد
            var members = await _userService.GetCompanyMembersAsync(user.CompanyId);

            if (members.All(x => x.Id != command.UserId))
                throw new ArgumentException("این کاربر جزو پرسنل شرکت نیست.");

            await _unitOfWork.PermissionRepository.UpsertAsync(new MemberPermission
            {
                CompanyId = user.CompanyId,
                UserId = command.UserId,
                CanCreateTask = command.CanCreateTask,
                CanCreateForOthers = command.CanCreateForOthers,
                CanAssignToOthers = command.CanAssignToOthers,
                CanViewOthersTasks = command.CanViewOthersTasks,
                CanAssignToManager = command.CanAssignToManager
            });

            return new CommandResult { Id = command.UserId };
        }
    }
}
