namespace NovinTamas.TaskManager.Domain.Models
{
    public class BaseEntity<T>
    {
        public T? Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
