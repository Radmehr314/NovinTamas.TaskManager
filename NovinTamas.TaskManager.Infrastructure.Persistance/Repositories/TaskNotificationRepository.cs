using MongoDB.Bson;
using MongoDB.Driver;
using NovinTamas.TaskManager.Domain.Common;
using NovinTamas.TaskManager.Domain.Models.Notifications;
using NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments.NotificationMongo;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Repositories
{
    public class TaskNotificationRepository : ITaskNotificationRepository
    {
        private readonly IMongoCollection<NotificationDocument> _collection;

        private static readonly FilterDefinitionBuilder<NotificationDocument> F = Builders<NotificationDocument>.Filter;

        public TaskNotificationRepository(IMongoDatabase mongoDatabase)
        {
            _collection = mongoDatabase.GetCollection<NotificationDocument>("TaskNotifications");
            _ = CreateIndexesAsync();
        }

        private async Task CreateIndexesAsync()
        {
            try
            {
                await _collection.Indexes.CreateOneAsync(new CreateIndexModel<NotificationDocument>(
                    Builders<NotificationDocument>.IndexKeys
                        .Ascending(x => x.CompanyId)
                        .Ascending(x => x.ReceiverUserId)
                        .Ascending(x => x.IsRead)
                        .Descending(x => x.CreatedAt),
                    new CreateIndexOptions { Name = "idx_tasknotifications_receiver" }));
            }
            catch { }
        }

        public async Task<string> AddAsync(TaskNotification notification)
        {
            var doc = NotificationMapper.ToDocument(notification);
            await _collection.InsertOneAsync(doc);
            return doc.Id.ToString();
        }

        public async Task AddManyAsync(IEnumerable<TaskNotification> notifications)
        {
            var docs = notifications.Select(NotificationMapper.ToDocument).ToList();

            if (docs.Count == 0) return;

            await _collection.InsertManyAsync(docs);
        }

        public async Task<PagedResult<TaskNotification>> GetByUserPagedAsync(string companyId, string userId, QueryOptions options, bool onlyUnread)
        {
            var filter = F.And(F.Eq(x => x.CompanyId, companyId), F.Eq(x => x.ReceiverUserId, userId));

            if (onlyUnread)
                filter = F.And(filter, F.Eq(x => x.IsRead, false));

            var totalCount = await _collection.CountDocumentsAsync(filter);
            var limit = options.Limit <= 0 ? 20 : options.Limit;

            var docs = await _collection
                .Find(filter)
                .Sort(Builders<NotificationDocument>.Sort.Descending(x => x.CreatedAt))
                .Skip(Math.Max(options.Skip, 0))
                .Limit(limit)
                .ToListAsync();

            return new PagedResult<TaskNotification>
            {
                Items = docs.Select(NotificationMapper.ToDomain).ToList(),
                TotalCount = totalCount,
                Skip = options.Skip,
                Limit = limit
            };
        }

        public async Task<long> GetUnreadCountAsync(string companyId, string userId)
        {
            var filter = F.And(
                F.Eq(x => x.CompanyId, companyId),
                F.Eq(x => x.ReceiverUserId, userId),
                F.Eq(x => x.IsRead, false));

            return await _collection.CountDocumentsAsync(filter);
        }

        public async Task MarkAsReadAsync(string companyId, string userId, List<string> ids)
        {
            var objectIds = ids
                .Where(id => ObjectId.TryParse(id, out _))
                .Select(ObjectId.Parse)
                .ToList();

            if (objectIds.Count == 0) return;

            var filter = F.And(
                F.Eq(x => x.CompanyId, companyId),
                F.Eq(x => x.ReceiverUserId, userId),
                F.In(x => x.Id, objectIds));

            await _collection.UpdateManyAsync(filter, BuildReadUpdate());
        }

        public async Task MarkAllAsReadAsync(string companyId, string userId)
        {
            var filter = F.And(
                F.Eq(x => x.CompanyId, companyId),
                F.Eq(x => x.ReceiverUserId, userId),
                F.Eq(x => x.IsRead, false));

            await _collection.UpdateManyAsync(filter, BuildReadUpdate());
        }

        public async Task DeleteByTaskIdAsync(string companyId, string taskId)
        {
            await _collection.DeleteManyAsync(F.And(F.Eq(x => x.CompanyId, companyId), F.Eq(x => x.TaskId, taskId)));
        }

        private static UpdateDefinition<NotificationDocument> BuildReadUpdate() =>
            Builders<NotificationDocument>.Update
                .Set(x => x.IsRead, true)
                .Set(x => x.ReadAt, DateTime.UtcNow);
    }
}
