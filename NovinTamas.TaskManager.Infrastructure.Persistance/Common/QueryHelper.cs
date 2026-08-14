using MongoDB.Driver;
using NovinTamas.TaskManager.Domain.Common;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Common
{
    public static class QueryHelper
    {
        public static async Task<List<TDocument>> ApplyPagingAndSortingAsync<TDocument>(
            IMongoCollection<TDocument> collection,
            FilterDefinition<TDocument> filter,
            QueryOptions options,
            IReadOnlyDictionary<string, string>? allowedSortFields,
            IReadOnlyCollection<QuerySortOption>? defaultSorts = null)
        {
            var query = collection.Find(filter);
            var sortDefinition = BuildSortDefinition<TDocument>(options, allowedSortFields, defaultSorts);

            if (sortDefinition != null)
                query = query.Sort(sortDefinition);

            var limit = options.Limit <= 0 ? 50 : options.Limit;
            return await query.Skip(Math.Max(options.Skip, 0)).Limit(limit).ToListAsync();
        }

        private static SortDefinition<TDocument>? BuildSortDefinition<TDocument>(
            QueryOptions options,
            IReadOnlyDictionary<string, string>? allowedSortFields,
            IReadOnlyCollection<QuerySortOption>? defaultSorts)
        {
            var sorts = (options.Sorts ?? new List<QuerySortOption>())
                .Where(sort => !string.IsNullOrWhiteSpace(sort.Field))
                .ToList();

            if (sorts.Count == 0 && !string.IsNullOrWhiteSpace(options.SortBy))
                sorts.Add(new QuerySortOption { Field = options.SortBy, Descending = options.Descending });

            if (sorts.Count == 0 && defaultSorts != null)
                sorts.AddRange(defaultSorts.Where(sort => !string.IsNullOrWhiteSpace(sort.Field)));

            if (sorts.Count == 0)
                return null;

            return Builders<TDocument>.Sort.Combine(sorts.Select(sort =>
            {
                var field = ResolveSortField(sort.Field!, allowedSortFields);
                return sort.Descending
                    ? Builders<TDocument>.Sort.Descending(field)
                    : Builders<TDocument>.Sort.Ascending(field);
            }));
        }

        private static string ResolveSortField(string field, IReadOnlyDictionary<string, string>? allowedSortFields)
        {
            var normalized = field.Trim();

            if (allowedSortFields == null)
                return char.ToUpperInvariant(normalized[0]) + normalized[1..];

            // فیلد ناشناخته به‌جای throw به مرتب‌سازی پیش‌فرض برمی‌گردد تا کلاینت با 500 مواجه نشود
            return allowedSortFields.TryGetValue(normalized, out var documentField)
                ? documentField
                : allowedSortFields.Values.First();
        }
    }
}
