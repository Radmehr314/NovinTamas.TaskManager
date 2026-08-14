namespace NovinTamas.TaskManager.Domain.Common
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public long TotalCount { get; set; }
        public int Skip { get; set; }
        public int Limit { get; set; }

        public int TotalPages => Limit <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / Limit);
        public bool HasNext => Limit > 0 && Skip + Limit < TotalCount;
    }
}
