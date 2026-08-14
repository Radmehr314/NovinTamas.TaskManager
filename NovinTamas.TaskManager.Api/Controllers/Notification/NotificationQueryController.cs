using Microsoft.AspNetCore.Mvc;
using NovinTamas.Framework.Api;
using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Application.Contracts.Query.Notification;
using NovinTamas.TaskManager.Application.Contracts.QueryResult.Common;
using NovinTamas.TaskManager.Application.Contracts.QueryResult.Notification;

namespace NovinTamas.TaskManager.Api.Controllers.Notification
{
    public class NotificationQueryController : BaseQueryController
    {
        public NotificationQueryController(IQueryBus bus) : base(bus)
        {
        }

        [HttpPost("GetMyNotifications")]
        public async Task<ActionResult<PagedQueryResult<GetNotificationQueryResult>>> GetMyNotifications([FromBody] GetMyNotificationsQuery query)
            => Ok(await Bus.Dispatch<GetMyNotificationsQuery, PagedQueryResult<GetNotificationQueryResult>>(query));

        [HttpPost("GetUnreadCount")]
        public async Task<ActionResult<GetUnreadNotificationsCountQueryResult>> GetUnreadCount([FromBody] GetUnreadNotificationsCountQuery query)
            => Ok(await Bus.Dispatch<GetUnreadNotificationsCountQuery, GetUnreadNotificationsCountQueryResult>(query));
    }
}
