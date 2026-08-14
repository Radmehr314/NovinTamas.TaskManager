using Microsoft.AspNetCore.Mvc;
using NovinTamas.Framework.Api;
using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Application.Contracts.Query.Project;
using NovinTamas.TaskManager.Application.Contracts.QueryResult.Project;

namespace NovinTamas.TaskManager.Api.Controllers.Project
{
    public class ProjectQueryController : BaseQueryController
    {
        public ProjectQueryController(IQueryBus bus) : base(bus)
        {
        }

        [HttpPost("GetProjects")]
        public async Task<ActionResult<List<GetProjectQueryResult>>> GetProjects([FromBody] GetProjectsQuery query)
            => Ok(await Bus.Dispatch<GetProjectsQuery, List<GetProjectQueryResult>>(query));

        [HttpPost("GetProjectById")]
        public async Task<ActionResult<GetProjectQueryResult>> GetProjectById([FromBody] GetProjectByIdQuery query)
            => Ok(await Bus.Dispatch<GetProjectByIdQuery, GetProjectQueryResult>(query));
    }
}
