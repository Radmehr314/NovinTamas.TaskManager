namespace NovinTamas.TaskManager.Domain.Enums
{
    // نام TaskStatus عمداً استفاده نشده چون با System.Threading.Tasks.TaskStatus تداخل دارد
    public enum TaskState
    {
        Todo = 1,
        InProgress = 2,
        InReview = 3,
        Done = 4,
        Cancelled = 5
    }
}
