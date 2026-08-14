using MongoDB.Driver;
using NovinTamas.TaskManager.Domain.Models.Histories;
using NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments.HistoryMongo;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Repositories
{
    public class TaskHistoryRepository : ITaskHistoryRepository
    {
        private readonly IMongoCollection<HistoryDocument> _collection;

        private static readonly FilterDefinitionBuilder<HistoryDocument> F = Builders<HistoryDocument>.Filter;

        public TaskHistoryRepository(IMongoDatabase mongoDatabase)
        {
            _collection = mongoDatabase.GetCollection<HistoryDocument>("TaskHistories");
            _ = CreateIndexesAsync();
        }

        private async Task CreateIndexesAsync()
        {
            try
            {
                await _collection.Indexes.CreateOneAsync(new CreateIndexModel<HistoryDocument>(
                    Builders<HistoryDocument>.IndexKeys.Ascending(x => x.CompanyId).Ascending(x => x.TaskId).Descending(x => x.CreatedAt),
                    new CreateIndexOptions { Name = "idx_taskhistories_company_task" }));
            }
            catch { }
        }

        public async Task<List<TaskHistory>> GetByTaskIdAsync(string companyId, string taskId)
        {
            var filter = F.And(F.Eq(x => x.CompanyId, companyId), F.Eq(x => x.TaskId, taskId));
            var docs = await _collection
                .Find(filter)
                .Sort(Builders<HistoryDocument>.Sort.Descending(x => x.CreatedAt))
                .ToListAsync();

            return docs.Select(HistoryMapper.ToDomain).ToList();
        }

        public async Task<List<TaskHistory>> GetRecentByCompanyAsync(string companyId, int limit)
        {
            var docs = await _collection
                .Find(F.Eq(x => x.CompanyId, companyId))
                .Sort(Builders<HistoryDocument>.Sort.Descending(x => x.CreatedAt))
                .Limit(limit)
                .ToListAsync();

            return docs.Select(HistoryMapper.ToDomain).ToList();
        }

        public async Task<string> AddAsync(TaskHistory history)
        {
            var doc = HistoryMapper.ToDocument(history);
            await _collection.InsertOneAsync(doc);
            return doc.Id.ToString();
        }

        public async Task DeleteByTaskIdAsync(string companyId, string taskId)
        {
            await _collection.DeleteManyAsync(F.And(F.Eq(x => x.CompanyId, companyId), F.Eq(x => x.TaskId, taskId)));
        }
    }
}
