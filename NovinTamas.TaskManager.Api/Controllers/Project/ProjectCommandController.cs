using Microsoft.AspNetCore.Mvc;
using NovinTamas.Framework.Api;
using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Application.Contracts.Commands.Project;

namespace NovinTamas.TaskManager.Api.Controllers.Project
{
    public class ProjectCommandController : BaseCommandController
    {
        public ProjectCommandController(ICommandBus bus) : base(bus)
        {
        }

        [HttpPost("CreateProject")]
        public async Task<ActionResult<CommandResult>> CreateProject([FromBody] CreateProjectCommand command)
            => Ok(await Bus.Dispatch(command));

        [HttpPut("UpdateProject")]
        public async Task<ActionResult<CommandResult>> UpdateProject([FromBody] UpdateProjectCommand command)
            => Ok(await Bus.Dispatch(command));

        [HttpPatch("ArchiveProject")]
        public async Task<ActionResult<CommandResult>> ArchiveProject([FromBody] ArchiveProjectCommand command)
            => Ok(await Bus.Dispatch(command));

        [HttpDelete("DeleteProject")]
        public async Task<ActionResult<CommandResult>> DeleteProject([FromBody] DeleteProjectCommand command)
            => Ok(await Bus.Dispatch(command));
    }
}
