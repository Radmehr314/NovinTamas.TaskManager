namespace NovinTamas.TaskManager.Domain.Enums
{
    public enum NotificationType
    {
        TaskAssigned = 1,
        TaskUnassigned = 2,
        StatusChanged = 3,
        PriorityChanged = 4,
        CommentAdded = 5,
        DueDateChanged = 6,
        TaskCompleted = 7,
        TaskDueSoon = 8,
        TaskOverdue = 9,
        Mentioned = 10
    }
}
