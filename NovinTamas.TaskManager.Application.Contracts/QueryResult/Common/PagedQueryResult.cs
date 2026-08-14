namespace NovinTamas.TaskManager.Application.Contracts.QueryResult.Common
{
    public class PagedQueryResult<TItem>
    {
        public List<TItem> Items { get; set; } = new();
        public long TotalCount { get; set; }
        public int Skip { get; set; }
        public int Limit { get; set; }
        public int TotalPages => Limit <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / Limit);
        public bool HasNextPage => Limit > 0 && Skip + Limit < TotalCount;
        public bool HasPreviousPage => Skip > 0 && TotalCount > 0;
    }
}
