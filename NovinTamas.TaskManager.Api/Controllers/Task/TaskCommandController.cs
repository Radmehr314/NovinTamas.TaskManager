using Microsoft.AspNetCore.Mvc;
using NovinTamas.Framework.Api;
using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Application.Contracts.Commands.Task;

namespace NovinTamas.TaskManager.Api.Controllers.Task
{
    public class TaskCommandController : BaseCommandController
    {
        public TaskCommandController(ICommandBus bus) : base(bus)
        {
        }

        [HttpPost("CreateTask")]
        public async Task<ActionResult<CommandResult>> CreateTask([FromBody] CreateTaskCommand command)
            => Ok(await Bus.Dispatch(Audit(command)));

        [HttpPut("UpdateTask")]
        public async Task<ActionResult<CommandResult>> UpdateTask([FromBody] UpdateTaskCommand command)
            => Ok(await Bus.Dispatch(Audit(command)));

        [HttpPatch("ChangeStatus")]
        public async Task<ActionResult<CommandResult>> ChangeStatus([FromBody] ChangeTaskStatusCommand command)
            => Ok(await Bus.Dispatch(Audit(command)));

        [HttpPatch("ChangePriority")]
        public async Task<ActionResult<CommandResult>> ChangePriority([FromBody] ChangeTaskPriorityCommand command)
            => Ok(await Bus.Dispatch(Audit(command)));

        [HttpPatch("Assign")]
        public async Task<ActionResult<CommandResult>> Assign([FromBody] AssignTaskCommand command)
            => Ok(await Bus.Dispatch(Audit(command)));

        [HttpPatch("SetProgress")]
        public async Task<ActionResult<CommandResult>> SetProgress([FromBody] SetTaskProgressCommand command)
            => Ok(await Bus.Dispatch(Audit(command)));

        [HttpPost("AddChecklistItem")]
        public async Task<ActionResult<CommandResult>> AddChecklistItem([FromBody] AddChecklistItemCommand command)
            => Ok(await Bus.Dispatch(Audit(command)));

        [HttpPatch("ToggleChecklistItem")]
        public async Task<ActionResult<CommandResult>> ToggleChecklistItem([FromBody] ToggleChecklistItemCommand command)
            => Ok(await Bus.Dispatch(Audit(command)));

        [HttpDelete("DeleteChecklistItem")]
        public async Task<ActionResult<CommandResult>> DeleteChecklistItem([FromBody] DeleteChecklistItemCommand command)
            => Ok(await Bus.Dispatch(Audit(command)));

        [HttpPatch("Archive")]
        public async Task<ActionResult<CommandResult>> Archive([FromBody] ArchiveTaskCommand command)
            => Ok(await Bus.Dispatch(Audit(command)));

        [HttpDelete("DeleteTask")]
        public async Task<ActionResult<CommandResult>> DeleteTask([FromBody] DeleteTaskCommand command)
            => Ok(await Bus.Dispatch(Audit(command)));

        private T Audit<T>(T command) where T : AuditedCommand
        {
            command.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            command.UserAgent = HttpContext.Request.Headers.UserAgent.ToString();
            return command;
        }
    }
}
