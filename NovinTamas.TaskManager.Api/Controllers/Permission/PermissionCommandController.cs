using Microsoft.AspNetCore.Mvc;
using NovinTamas.Framework.Api;
using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Application.Contracts.Commands.Permission;

namespace NovinTamas.TaskManager.Api.Controllers.Permission
{
    public class PermissionCommandController : BaseCommandController
    {
        public PermissionCommandController(ICommandBus bus) : base(bus)
        {
        }

        [HttpPut("SetMemberPermissions")]
        public async Task<ActionResult<CommandResult>> SetMemberPermissions([FromBody] SetMemberPermissionsCommand command)
            => Ok(await Bus.Dispatch(command));
    }
}
