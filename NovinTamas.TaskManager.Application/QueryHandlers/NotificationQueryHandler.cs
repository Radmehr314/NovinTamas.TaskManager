using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Application.Contracts.Query.Notification;
using NovinTamas.TaskManager.Application.Contracts.QueryResult.Common;
using NovinTamas.TaskManager.Application.Contracts.QueryResult.Notification;
using NovinTamas.TaskManager.Domain;
using NovinTamas.TaskManager.Domain.Common;
using NovinTamas.TaskManager.Domain.Enums;

namespace NovinTamas.TaskManager.Application.QueryHandlers
{
    public class NotificationQueryHandler :
        IQueryHandler<GetMyNotificationsQuery, PagedQueryResult<GetNotificationQueryResult>>,
        IQueryHandler<GetUnreadNotificationsCountQuery, GetUnreadNotificationsCountQueryResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserInfoService _userInfoService;

        public NotificationQueryHandler(IUnitOfWork unitOfWork, IUserInfoService userInfoService)
        {
            _unitOfWork = unitOfWork;
            _userInfoService = userInfoService;
        }

        public async Task<PagedQueryResult<GetNotificationQueryResult>> Handle(GetMyNotificationsQuery query)
        {
            var user = _userInfoService.GetCurrentUser();

            var paged = await _unitOfWork.NotificationRepository.GetByUserPagedAsync(
                user.CompanyId, user.UserId, query.Options ?? new QueryOptions { Limit = 20 }, query.OnlyUnread);

            return new PagedQueryResult<GetNotificationQueryResult>
            {
                Items = paged.Items.Select(x => new GetNotificationQueryResult
                {
                    Id = x.Id!,
                    TaskId = x.TaskId,
                    TaskTitle = x.TaskTitle,
                    SenderName = x.SenderName,
                    Message = x.Message,
                    Type = (int)x.Type,
                    TypeName = x.Type.ToPersian(),
                    IsRead = x.IsRead,
                    CreatedAt = x.CreatedAt
                }).ToList(),
                TotalCount = paged.TotalCount,
                Skip = paged.Skip,
                Limit = paged.Limit
            };
        }

        public async Task<GetUnreadNotificationsCountQueryResult> Handle(GetUnreadNotificationsCountQuery query)
        {
            var user = _userInfoService.GetCurrentUser();

            return new GetUnreadNotificationsCountQueryResult
            {
                Count = await _unitOfWork.NotificationRepository.GetUnreadCountAsync(user.CompanyId, user.UserId)
            };
        }
    }
}
