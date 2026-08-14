using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Domain.Common;

namespace NovinTamas.TaskManager.Application.Contracts.Query.Notification
{
    public class GetMyNotificationsQuery : IQuery
    {
        public QueryOptions Options { get; set; } = new();
        public bool OnlyUnread { get; set; }
    }

    public class GetUnreadNotificationsCountQuery : IQuery
    {
    }
}
