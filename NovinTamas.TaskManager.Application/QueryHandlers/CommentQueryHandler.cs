using NovinTamas.Framework.Application;
using NovinTamas.Framework.Application.Exceptions;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Application.Contracts.Query.Comment;
using NovinTamas.TaskManager.Application.Contracts.QueryResult.Comment;
using NovinTamas.TaskManager.Application.Mapper;
using NovinTamas.TaskManager.Domain;

namespace NovinTamas.TaskManager.Application.QueryHandlers
{
    public class CommentQueryHandler : IQueryHandler<GetTaskCommentsQuery, List<GetTaskCommentsQueryResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserInfoService _userInfoService;
        private readonly IPermissionResolver _permissions;

        public CommentQueryHandler(IUnitOfWork unitOfWork, IUserInfoService userInfoService, IPermissionResolver permissions)
        {
            _unitOfWork = unitOfWork;
            _userInfoService = userInfoService;
            _permissions = permissions;
        }

        public async Task<List<GetTaskCommentsQueryResult>> Handle(GetTaskCommentsQuery query)
        {
            var user = _userInfoService.GetCurrentUser();

            var task = await _unitOfWork.TaskRepository.GetByIdAsync(user.CompanyId, query.TaskId)
                       ?? throw new NotFoundException("وظیفه یافت نشد.");

            TaskAccess.EnsureView(user, await _permissions.GetCurrentAsync(), task);

            var comments = await _unitOfWork.CommentRepository.GetByTaskIdAsync(user.CompanyId, query.TaskId);

            return comments.Select(x => new GetTaskCommentsQueryResult
            {
                Id = x.Id!,
                TaskId = x.TaskId,
                UserId = x.UserId,
                UserName = x.UserName,
                Role = x.Role,
                Content = x.Content,
                Files = x.Files,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                CanEdit = x.UserId == user.UserId,
                CanDelete = user.IsCompanyOwner || x.UserId == user.UserId
            }).ToList();
        }
    }
}
