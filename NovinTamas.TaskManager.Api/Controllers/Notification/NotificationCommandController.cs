using Microsoft.AspNetCore.Mvc;
using NovinTamas.Framework.Api;
using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Application.Contracts.Commands.Notification;

namespace NovinTamas.TaskManager.Api.Controllers.Notification
{
    public class NotificationCommandController : BaseCommandController
    {
        public NotificationCommandController(ICommandBus bus) : base(bus)
        {
        }

        [HttpPatch("MarkAsRead")]
        public async Task<ActionResult<CommandResult>> MarkAsRead([FromBody] MarkNotificationsAsReadCommand command)
            => Ok(await Bus.Dispatch(command));
    }
}
