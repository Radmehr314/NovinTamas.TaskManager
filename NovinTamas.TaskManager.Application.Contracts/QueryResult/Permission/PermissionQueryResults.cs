namespace NovinTamas.TaskManager.Application.Contracts.QueryResult.Permission
{
    public class MemberPermissionResult
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool CanCreateTask { get; set; }
        public bool CanCreateForOthers { get; set; }
        public bool CanAssignToOthers { get; set; }
        public bool CanViewOthersTasks { get; set; }
        public bool CanAssignToManager { get; set; }
        public int AssignedTaskCount { get; set; }
    }
}
