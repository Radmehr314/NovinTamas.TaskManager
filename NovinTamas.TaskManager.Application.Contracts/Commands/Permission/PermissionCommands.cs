using NovinTamas.Framework.Application;

namespace NovinTamas.TaskManager.Application.Contracts.Commands.Permission
{
    public class SetMemberPermissionsCommand : ICommand
    {
        public string UserId { get; set; } = string.Empty;
        public bool CanCreateTask { get; set; }
        public bool CanCreateForOthers { get; set; }
        public bool CanAssignToOthers { get; set; }
        public bool CanViewOthersTasks { get; set; }
        public bool CanAssignToManager { get; set; }
    }
}
