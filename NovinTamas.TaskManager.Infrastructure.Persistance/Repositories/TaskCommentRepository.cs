using MongoDB.Bson;
using MongoDB.Driver;
using NovinTamas.TaskManager.Domain.Models.Comments;
using NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments.CommentMongo;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Repositories
{
    public class TaskCommentRepository : ITaskCommentRepository
    {
        private readonly IMongoCollection<CommentDocument> _collection;

        private static readonly FilterDefinitionBuilder<CommentDocument> F = Builders<CommentDocument>.Filter;

        public TaskCommentRepository(IMongoDatabase mongoDatabase)
        {
            _collection = mongoDatabase.GetCollection<CommentDocument>("TaskComments");
            _ = CreateIndexesAsync();
        }

        private async Task CreateIndexesAsync()
        {
            try
            {
                await _collection.Indexes.CreateOneAsync(new CreateIndexModel<CommentDocument>(
                    Builders<CommentDocument>.IndexKeys.Ascending(x => x.CompanyId).Ascending(x => x.TaskId).Ascending(x => x.CreatedAt),
                    new CreateIndexOptions { Name = "idx_taskcomments_company_task" }));
            }
            catch { }
        }

        public async Task<TaskComment?> GetByIdAsync(string companyId, string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return null;

            var filter = F.And(F.Eq(x => x.CompanyId, companyId), F.Eq(x => x.Id, objectId));
            var doc = await _collection.Find(filter).FirstOrDefaultAsync();
            return doc == null ? null : CommentMapper.ToDomain(doc);
        }

        public async Task<List<TaskComment>> GetByTaskIdAsync(string companyId, string taskId)
        {
            var filter = F.And(F.Eq(x => x.CompanyId, companyId), F.Eq(x => x.TaskId, taskId));
            var docs = await _collection
                .Find(filter)
                .Sort(Builders<CommentDocument>.Sort.Ascending(x => x.CreatedAt))
                .ToListAsync();

            return docs.Select(CommentMapper.ToDomain).ToList();
        }

        public async Task<Dictionary<string, int>> GetCountsByTaskIdsAsync(string companyId, IEnumerable<string> taskIds)
        {
            var ids = taskIds.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();

            if (ids.Count == 0)
                return new Dictionary<string, int>();

            var filter = F.And(F.Eq(x => x.CompanyId, companyId), F.In(x => x.TaskId, ids));
            var docs = await _collection.Find(filter).Project(x => x.TaskId).ToListAsync();

            return docs.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
        }

        public async Task<string> AddAsync(TaskComment comment)
        {
            var doc = CommentMapper.ToDocument(comment);
            await _collection.InsertOneAsync(doc);
            return doc.Id.ToString();
        }

        public async Task UpdateAsync(TaskComment comment)
        {
            if (!ObjectId.TryParse(comment.Id, out var objectId)) return;

            var update = Builders<CommentDocument>.Update
                .Set(x => x.Content, comment.Content)
                .Set(x => x.Files, comment.Files ?? new List<string>())
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(
                F.And(F.Eq(x => x.CompanyId, comment.CompanyId), F.Eq(x => x.Id, objectId)), update);
        }

        public async Task DeleteAsync(string companyId, string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return;

            await _collection.DeleteOneAsync(F.And(F.Eq(x => x.CompanyId, companyId), F.Eq(x => x.Id, objectId)));
        }

        public async Task DeleteByTaskIdAsync(string companyId, string taskId)
        {
            await _collection.DeleteManyAsync(F.And(F.Eq(x => x.CompanyId, companyId), F.Eq(x => x.TaskId, taskId)));
        }
    }
}
