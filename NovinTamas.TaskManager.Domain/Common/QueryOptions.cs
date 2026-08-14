namespace NovinTamas.TaskManager.Domain.Common
{
    public class QueryOptions
    {
        public int Skip { get; set; } = 0;
        public int Limit { get; set; } = 50;
        public string? SortBy { get; set; }
        public bool Descending { get; set; } = true;
        public List<QuerySortOption> Sorts { get; set; } = new();
    }

    public class QuerySortOption
    {
        public string? Field { get; set; }
        public bool Descending { get; set; } = true;
    }
}
