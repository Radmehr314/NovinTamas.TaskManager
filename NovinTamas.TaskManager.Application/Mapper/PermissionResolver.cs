using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Domain;

namespace NovinTamas.TaskManager.Application.Mapper
{
    public class PermissionResolver : IPermissionResolver
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserInfoService _userInfoService;

        private MemberPermissions? _cached;

        public PermissionResolver(IUnitOfWork unitOfWork, IUserInfoService userInfoService)
        {
            _unitOfWork = unitOfWork;
            _userInfoService = userInfoService;
        }

        public async Task<MemberPermissions> GetCurrentAsync()
        {
            // این سرویس scoped است، پس در طول یک request فقط یک بار به دیتابیس می‌زند
            if (_cached != null) return _cached;

            var user = _userInfoService.GetCurrentUser();

            if (user.IsCompanyOwner)
                return _cached = MemberPermissions.Owner;

            var record = await _unitOfWork.PermissionRepository.GetAsync(user.CompanyId, user.UserId);

            return _cached = record == null
                ? MemberPermissions.Default
                : new MemberPermissions(
                    record.CanCreateTask,
                    record.CanCreateForOthers,
                    record.CanAssignToOthers,
                    record.CanViewOthersTasks,
                    record.CanAssignToManager);
        }
    }
}
