using NovinTamas.Framework.Application;

namespace NovinTamas.TaskManager.Application.Contracts.Commands.Notification
{
    public class MarkNotificationsAsReadCommand : ICommand
    {
        public List<string> Ids { get; set; } = new();
        public bool All { get; set; }
    }
}
