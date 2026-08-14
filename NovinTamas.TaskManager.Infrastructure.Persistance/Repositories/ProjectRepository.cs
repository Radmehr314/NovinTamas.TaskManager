using MongoDB.Bson;
using MongoDB.Driver;
using NovinTamas.Framework.Application.Exceptions;
using NovinTamas.TaskManager.Domain.Models.Projects;
using NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments.ProjectMongo;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly IMongoCollection<ProjectDocument> _collection;

        private static readonly FilterDefinitionBuilder<ProjectDocument> F = Builders<ProjectDocument>.Filter;

        public ProjectRepository(IMongoDatabase mongoDatabase)
        {
            _collection = mongoDatabase.GetCollection<ProjectDocument>("Projects");
            _ = CreateIndexesAsync();
        }

        private async Task CreateIndexesAsync()
        {
            try
            {
                await _collection.Indexes.CreateOneAsync(new CreateIndexModel<ProjectDocument>(
                    Builders<ProjectDocument>.IndexKeys.Ascending(x => x.CompanyId).Ascending(x => x.IsArchived),
                    new CreateIndexOptions { Name = "idx_projects_company_archived" }));
            }
            catch { }
        }

        private static FilterDefinition<ProjectDocument> ById(string companyId, string id) =>
            F.And(F.Eq(x => x.CompanyId, companyId), F.Eq(x => x.Id, ObjectId.Parse(id)));

        public async Task<Project?> GetByIdAsync(string companyId, string id)
        {
            if (!ObjectId.TryParse(id, out _)) return null;

            var doc = await _collection.Find(ById(companyId, id)).FirstOrDefaultAsync();
            return doc == null ? null : ProjectMapper.ToDomain(doc);
        }

        public async Task<List<Project>> GetByCompanyIdAsync(string companyId, bool includeArchived = false)
        {
            var filter = includeArchived
                ? F.Eq(x => x.CompanyId, companyId)
                : F.And(F.Eq(x => x.CompanyId, companyId), F.Eq(x => x.IsArchived, false));

            var docs = await _collection
                .Find(filter)
                .Sort(Builders<ProjectDocument>.Sort.Ascending(x => x.Name))
                .ToListAsync();

            return docs.Select(ProjectMapper.ToDomain).ToList();
        }

        public async Task<Dictionary<string, Project>> GetMapByIdsAsync(string companyId, IEnumerable<string> ids)
        {
            var objectIds = ids
                .Where(id => !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _))
                .Select(ObjectId.Parse)
                .Distinct()
                .ToList();

            if (objectIds.Count == 0)
                return new Dictionary<string, Project>();

            var filter = F.And(F.Eq(x => x.CompanyId, companyId), F.In(x => x.Id, objectIds));
            var docs = await _collection.Find(filter).ToListAsync();

            return docs.ToDictionary(x => x.Id.ToString(), ProjectMapper.ToDomain);
        }

        public async Task<string> AddAsync(Project project)
        {
            var doc = ProjectMapper.ToDocument(project);
            await _collection.InsertOneAsync(doc);
            return doc.Id.ToString();
        }

        public async Task UpdateAsync(Project project)
        {
            project.UpdatedAt = DateTime.UtcNow;
            var doc = ProjectMapper.ToDocument(project);
            var result = await _collection.ReplaceOneAsync(ById(project.CompanyId, project.Id!), doc);

            if (result.MatchedCount == 0)
                throw new NotFoundException("پروژه یافت نشد.");
        }

        public async Task SetArchivedAsync(string companyId, string id, bool isArchived)
        {
            var update = Builders<ProjectDocument>.Update
                .Set(x => x.IsArchived, isArchived)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(ById(companyId, id), update);

            if (result.MatchedCount == 0)
                throw new NotFoundException("پروژه یافت نشد.");
        }

        public async Task DeleteAsync(string companyId, string id)
        {
            await _collection.DeleteOneAsync(ById(companyId, id));
        }
    }
}
