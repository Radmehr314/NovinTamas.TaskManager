using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Application.Contracts.Query.Permission;
using NovinTamas.TaskManager.Application.Contracts.QueryResult.Permission;
using NovinTamas.TaskManager.Application.Mapper;
using NovinTamas.TaskManager.Domain;
using NovinTamas.TaskManager.Domain.Models.Permissions;

namespace NovinTamas.TaskManager.Application.QueryHandlers
{
    public class PermissionQueryHandler : IQueryHandler<GetMemberPermissionsQuery, List<MemberPermissionResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserInfoService _userInfoService;
        private readonly IUserService _userService;

        public PermissionQueryHandler(IUnitOfWork unitOfWork, IUserInfoService userInfoService, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userInfoService = userInfoService;
            _userService = userService;
        }

        public async Task<List<MemberPermissionResult>> Handle(GetMemberPermissionsQuery query)
        {
            var user = _userInfoService.GetCurrentUser();
            TaskAccess.EnsureCompanyOwner(user);

            var members = await _userService.GetCompanyMembersAsync(user.CompanyId);
            var stored = (await _unitOfWork.PermissionRepository.GetByCompanyIdAsync(user.CompanyId))
                .ToDictionary(x => x.UserId);

            var workload = (await _unitOfWork.TaskRepository.GetWorkloadAsync(user.CompanyId))
                .ToDictionary(x => x.UserId, x => x.TotalCount);

            // پرسنلی که رکورد ندارد با مقادیر پیش‌فرض برگردانده می‌شود تا لیست کامل باشد
            return members.Select(member =>
            {
                stored.TryGetValue(member.Id, out var record);

                return new MemberPermissionResult
                {
                    UserId = member.Id,
                    FullName = member.FullName,
                    CanCreateTask = record?.CanCreateTask ?? MemberPermission.DefaultCanCreateTask,
                    CanCreateForOthers = record?.CanCreateForOthers ?? MemberPermission.DefaultCanCreateForOthers,
                    CanAssignToOthers = record?.CanAssignToOthers ?? MemberPermission.DefaultCanAssignToOthers,
                    CanViewOthersTasks = record?.CanViewOthersTasks ?? MemberPermission.DefaultCanViewOthersTasks,
                    CanAssignToManager = record?.CanAssignToManager ?? MemberPermission.DefaultCanAssignToManager,
                    AssignedTaskCount = workload.TryGetValue(member.Id, out var count) ? count : 0
                };
            }).ToList();
        }
    }
}
