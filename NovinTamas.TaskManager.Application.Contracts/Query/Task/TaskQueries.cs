using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Domain.Common;

namespace NovinTamas.TaskManager.Application.Contracts.Query.Task
{
    public class GetPagedTasksQuery : IQuery
    {
        public QueryOptions Options { get; set; } = new();
        public TaskSearchCriteria? Criteria { get; set; }
    }

    public class GetTaskBoardQuery : IQuery
    {
        public TaskSearchCriteria? Criteria { get; set; }
        public int PerColumnLimit { get; set; } = 100;
    }

    public class GetTaskByIdQuery : IQuery
    {
        public string Id { get; set; } = string.Empty;
    }

    public class GetTaskHistoryQuery : IQuery
    {
        public string TaskId { get; set; } = string.Empty;
    }

    public class GetTaskStatisticsQuery : IQuery
    {
        // اگر true باشد فقط وظایف ارجاع‌شده به کاربر جاری شمرده می‌شود (پنل پرسنل)
        public bool OnlyMine { get; set; }
    }

    public class GetTeamWorkloadQuery : IQuery
    {
    }

    public class GetUpcomingTasksQuery : IQuery
    {
        public bool OnlyMine { get; set; }
        public int Days { get; set; } = 7;
        public int Limit { get; set; } = 10;
    }

    public class GetTaskConfigQuery : IQuery
    {
    }
}
