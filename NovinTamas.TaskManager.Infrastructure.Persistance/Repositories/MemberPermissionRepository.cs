using MongoDB.Driver;
using NovinTamas.TaskManager.Domain.Models.Permissions;
using NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments.PermissionMongo;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Repositories
{
    public class MemberPermissionRepository : IMemberPermissionRepository
    {
        private readonly IMongoCollection<PermissionDocument> _collection;

        private static readonly FilterDefinitionBuilder<PermissionDocument> F = Builders<PermissionDocument>.Filter;

        public MemberPermissionRepository(IMongoDatabase mongoDatabase)
        {
            _collection = mongoDatabase.GetCollection<PermissionDocument>("MemberPermissions");
            _ = CreateIndexesAsync();
        }

        private async Task CreateIndexesAsync()
        {
            try
            {
                // یک رکورد دسترسی به ازای هر کاربر در هر شرکت
                await _collection.Indexes.CreateOneAsync(new CreateIndexModel<PermissionDocument>(
                    Builders<PermissionDocument>.IndexKeys.Ascending(x => x.CompanyId).Ascending(x => x.UserId),
                    new CreateIndexOptions { Name = "idx_permissions_company_user", Unique = true }));
            }
            catch { }
        }

        public async Task<MemberPermission?> GetAsync(string companyId, string userId)
        {
            var filter = F.And(F.Eq(x => x.CompanyId, companyId), F.Eq(x => x.UserId, userId));
            var doc = await _collection.Find(filter).FirstOrDefaultAsync();
            return doc == null ? null : PermissionMapper.ToDomain(doc);
        }

        public async Task<List<MemberPermission>> GetByCompanyIdAsync(string companyId)
        {
            var docs = await _collection.Find(F.Eq(x => x.CompanyId, companyId)).ToListAsync();
            return docs.Select(PermissionMapper.ToDomain).ToList();
        }

        public async Task UpsertAsync(MemberPermission permission)
        {
            var filter = F.And(
                F.Eq(x => x.CompanyId, permission.CompanyId),
                F.Eq(x => x.UserId, permission.UserId));

            var update = Builders<PermissionDocument>.Update
                .SetOnInsert(x => x.CompanyId, permission.CompanyId)
                .SetOnInsert(x => x.UserId, permission.UserId)
                .SetOnInsert(x => x.CreatedAt, DateTime.UtcNow)
                .Set(x => x.CanCreateTask, permission.CanCreateTask)
                .Set(x => x.CanCreateForOthers, permission.CanCreateForOthers)
                .Set(x => x.CanAssignToOthers, permission.CanAssignToOthers)
                .Set(x => x.CanViewOthersTasks, permission.CanViewOthersTasks)
                .Set(x => x.CanAssignToManager, permission.CanAssignToManager)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
        }

        public async Task DeleteAsync(string companyId, string userId)
        {
            await _collection.DeleteOneAsync(F.And(F.Eq(x => x.CompanyId, companyId), F.Eq(x => x.UserId, userId)));
        }
    }
}
