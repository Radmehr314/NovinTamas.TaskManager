using Microsoft.AspNetCore.Mvc;
using NovinTamas.Framework.Api;
using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Application.Contracts.Query.Task;
using NovinTamas.TaskManager.Application.Contracts.QueryResult.Common;
using NovinTamas.TaskManager.Application.Contracts.QueryResult.Task;

namespace NovinTamas.TaskManager.Api.Controllers.Task
{
    public class TaskQueryController : BaseQueryController
    {
        public TaskQueryController(IQueryBus bus) : base(bus)
        {
        }

        [HttpPost("GetTasks")]
        public async Task<ActionResult<PagedQueryResult<TaskSummaryResult>>> GetTasks([FromBody] GetPagedTasksQuery query)
            => Ok(await Bus.Dispatch<GetPagedTasksQuery, PagedQueryResult<TaskSummaryResult>>(query));

        [HttpPost("GetBoard")]
        public async Task<ActionResult<GetTaskBoardQueryResult>> GetBoard([FromBody] GetTaskBoardQuery query)
            => Ok(await Bus.Dispatch<GetTaskBoardQuery, GetTaskBoardQueryResult>(query));

        [HttpPost("GetTaskById")]
        public async Task<ActionResult<GetTaskByIdQueryResult>> GetTaskById([FromBody] GetTaskByIdQuery query)
            => Ok(await Bus.Dispatch<GetTaskByIdQuery, GetTaskByIdQueryResult>(query));

        [HttpPost("GetHistory")]
        public async Task<ActionResult<List<GetTaskHistoryQueryResult>>> GetHistory([FromBody] GetTaskHistoryQuery query)
            => Ok(await Bus.Dispatch<GetTaskHistoryQuery, List<GetTaskHistoryQueryResult>>(query));

        [HttpPost("GetStatistics")]
        public async Task<ActionResult<GetTaskStatisticsQueryResult>> GetStatistics([FromBody] GetTaskStatisticsQuery query)
            => Ok(await Bus.Dispatch<GetTaskStatisticsQuery, GetTaskStatisticsQueryResult>(query));

        [HttpPost("GetTeamWorkload")]
        public async Task<ActionResult<List<MemberWorkloadResult>>> GetTeamWorkload([FromBody] GetTeamWorkloadQuery query)
            => Ok(await Bus.Dispatch<GetTeamWorkloadQuery, List<MemberWorkloadResult>>(query));

        [HttpPost("GetUpcoming")]
        public async Task<ActionResult<List<TaskSummaryResult>>> GetUpcoming([FromBody] GetUpcomingTasksQuery query)
            => Ok(await Bus.Dispatch<GetUpcomingTasksQuery, List<TaskSummaryResult>>(query));

        [HttpPost("Config")]
        public async Task<ActionResult<GetTaskConfigQueryResult>> Config([FromBody] GetTaskConfigQuery query)
            => Ok(await Bus.Dispatch<GetTaskConfigQuery, GetTaskConfigQueryResult>(query));
    }
}
