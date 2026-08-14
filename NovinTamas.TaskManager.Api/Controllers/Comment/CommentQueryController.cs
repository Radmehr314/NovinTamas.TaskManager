using Microsoft.AspNetCore.Mvc;
using NovinTamas.Framework.Api;
using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Application.Contracts.Query.Comment;
using NovinTamas.TaskManager.Application.Contracts.QueryResult.Comment;

namespace NovinTamas.TaskManager.Api.Controllers.Comment
{
    public class CommentQueryController : BaseQueryController
    {
        public CommentQueryController(IQueryBus bus) : base(bus)
        {
        }

        [HttpPost("GetComments")]
        public async Task<ActionResult<List<GetTaskCommentsQueryResult>>> GetComments([FromBody] GetTaskCommentsQuery query)
            => Ok(await Bus.Dispatch<GetTaskCommentsQuery, List<GetTaskCommentsQueryResult>>(query));
    }
}
