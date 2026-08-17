using Microsoft.AspNetCore.Mvc;
using NovinTamas.Framework.Api;
using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Application.Contracts.Commands.Comment;

namespace NovinTamas.TaskManager.Api.Controllers.Comment
{
    public class CommentCommandController : BaseCommandController
    {
        public CommentCommandController(ICommandBus bus) : base(bus)
        {
        }

        [HttpPost("AddComment")]
        public async Task<ActionResult<CommandResult>> AddComment([FromBody] AddTaskCommentCommand command)
            => Ok(await Bus.Dispatch(command));

        [HttpPut("UpdateComment")]
        public async Task<ActionResult<CommandResult>> UpdateComment([FromBody] UpdateTaskCommentCommand command)
            => Ok(await Bus.Dispatch(command));

        [HttpDelete("DeleteComment")]
        public async Task<ActionResult<CommandResult>> DeleteComment([FromBody] DeleteTaskCommentCommand command)
            => Ok(await Bus.Dispatch(command));
    }
}
