using MongoDB.Bson;
using MongoDB.Driver;
using NovinTamas.Framework.Application.Exceptions;
using NovinTamas.TaskManager.Domain.Common;
using NovinTamas.TaskManager.Domain.Enums;
using NovinTamas.TaskManager.Domain.Models.Tasks;
using NovinTamas.TaskManager.Infrastructure.Persistance.Common;
using NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments;
using NovinTamas.TaskManager.Infrastructure.Persistance.MongoDocuments.TaskMongo;
using System.Text.RegularExpressions;

namespace NovinTamas.TaskManager.Infrastructure.Persistance.Repositories
{
    public class TaskItemRepository : ITaskItemRepository
    {
        private const double BoardOrderGap = 1024d;

        private readonly IMongoCollection<TaskDocument> _collection;

        private static readonly FilterDefinitionBuilder<TaskDocument> F = Builders<TaskDocument>.Filter;

        private static readonly IReadOnlyDictionary<string, string> SortFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["createdAt"] = nameof(BaseDocument.CreatedAt),
            ["updatedAt"] = nameof(BaseDocument.UpdatedAt),
            ["dueDate"] = nameof(TaskDocument.DueDate),
            ["priority"] = nameof(TaskDocument.Priority),
            ["status"] = nameof(TaskDocument.Status),
            ["title"] = nameof(TaskDocument.Title),
            ["code"] = nameof(TaskDocument.Code),
            ["progress"] = nameof(TaskDocument.Progress),
            ["boardOrder"] = nameof(TaskDocument.BoardOrder)
        };

        private static readonly IReadOnlyCollection<QuerySortOption> DefaultSorts = new[]
        {
            new QuerySortOption { Field = "priority", Descending = true },
            new QuerySortOption { Field = "createdAt", Descending = true }
        };

        public TaskItemRepository(IMongoDatabase mongoDatabase)
        {
            _collection = mongoDatabase.GetCollection<TaskDocument>("Tasks");
            _ = CreateIndexesAsync();
        }

        private async Task CreateIndexesAsync()
        {
            var models = new[]
            {
                new CreateIndexModel<TaskDocument>(
                    Builders<TaskDocument>.IndexKeys.Ascending(x => x.CompanyId).Ascending(x => x.Status).Ascending(x => x.BoardOrder),
                    new CreateIndexOptions { Name = "idx_tasks_company_status_order" }),
                new CreateIndexModel<TaskDocument>(
                    Builders<TaskDocument>.IndexKeys.Ascending(x => x.CompanyId).Ascending(x => x.AssigneeIds),
                    new CreateIndexOptions { Name = "idx_tasks_company_assignees" }),
                new CreateIndexModel<TaskDocument>(
                    Builders<TaskDocument>.IndexKeys.Ascending(x => x.CompanyId).Ascending(x => x.ProjectId),
                    new CreateIndexOptions { Name = "idx_tasks_company_project" }),
                new CreateIndexModel<TaskDocument>(
                    Builders<TaskDocument>.IndexKeys.Ascending(x => x.CompanyId).Ascending(x => x.DueDate),
                    new CreateIndexOptions { Name = "idx_tasks_company_duedate" }),
                new CreateIndexModel<TaskDocument>(
                    Builders<TaskDocument>.IndexKeys.Ascending(x => x.CompanyId).Descending(x => x.CreatedAt),
                    new CreateIndexOptions { Name = "idx_tasks_company_createdat" })
            };

            try { await _collection.Indexes.CreateManyAsync(models); } catch { }
        }

        private static FilterDefinition<TaskDocument> ById(string companyId, string id) =>
            F.And(F.Eq(x => x.CompanyId, companyId), F.Eq(x => x.Id, ObjectId.Parse(id)));

        public async Task<TaskItem?> GetByIdAsync(string companyId, string id)
        {
            if (!ObjectId.TryParse(id, out _)) return null;

            var doc = await _collection.Find(ById(companyId, id)).FirstOrDefaultAsync();
            return doc == null ? null : TaskMapper.ToDomain(doc);
        }

        public async Task<string> AddAsync(TaskItem task)
        {
            var counters = _collection.Database.GetCollection<CounterDocument>("Counters");
            var counter = await counters.FindOneAndUpdateAsync<CounterDocument>(
                Builders<CounterDocument>.Filter.Eq(c => c.Id, $"TaskCode:{task.CompanyId}"),
                Builders<CounterDocument>.Update.Inc(c => c.Sequence, 1L),
                new FindOneAndUpdateOptions<CounterDocument, CounterDocument>
                {
                    ReturnDocument = ReturnDocument.After,
                    IsUpsert = true
                });

            task.Code = $"T-{counter.Sequence}";

            if (task.BoardOrder == 0)
                task.BoardOrder = await GetNextBoardOrderAsync(task.CompanyId, task.Status);

            var doc = TaskMapper.ToDocument(task);
            await _collection.InsertOneAsync(doc);
            return doc.Id.ToString();
        }

        public async Task UpdateAsync(TaskItem task)
        {
            task.UpdatedAt = DateTime.UtcNow;
            var doc = TaskMapper.ToDocument(task);
            var result = await _collection.ReplaceOneAsync(ById(task.CompanyId, task.Id!), doc);

            if (result.MatchedCount == 0)
                throw new NotFoundException("وظیفه یافت نشد.");
        }

        public async Task DeleteAsync(string companyId, string id)
        {
            await _collection.DeleteOneAsync(ById(companyId, id));
        }

        public async Task<PagedResult<TaskItem>> GetPagedAsync(string companyId, QueryOptions options, TaskSearchCriteria? criteria, string? currentUserId)
        {
            var filter = BuildFilter(companyId, criteria, currentUserId);
            var totalCount = await _collection.CountDocumentsAsync(filter);
            var docs = await QueryHelper.ApplyPagingAndSortingAsync(_collection, filter, options, SortFields, DefaultSorts);

            return new PagedResult<TaskItem>
            {
                Items = docs.Select(TaskMapper.ToDomain).ToList(),
                TotalCount = totalCount,
                Skip = options.Skip,
                Limit = options.Limit
            };
        }

        public async Task<List<TaskItem>> GetBoardAsync(string companyId, TaskSearchCriteria? criteria, string? currentUserId, int perColumnLimit)
        {
            // برد همه‌ی ستون‌ها را با یک کوئری می‌گیرد و برش هر ستون در لایه‌ی بالاتر انجام می‌شود؛
            // سقف کلی جلوی برگرداندن هزاران کارت به مرورگر را می‌گیرد.
            var filter = BuildFilter(companyId, criteria, currentUserId);
            var docs = await _collection
                .Find(filter)
                .Sort(Builders<TaskDocument>.Sort.Ascending(x => x.BoardOrder).Descending(x => x.CreatedAt))
                .Limit(Math.Max(perColumnLimit, 1) * 5)
                .ToListAsync();

            return docs.Select(TaskMapper.ToDomain).ToList();
        }

        public async Task ChangeStatusAsync(string companyId, string id, TaskState status, double? boardOrder, DateTime? completedAt)
        {
            var update = Builders<TaskDocument>.Update
                .Set(x => x.Status, status)
                .Set(x => x.CompletedAt, completedAt)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            if (boardOrder.HasValue)
                update = update.Set(x => x.BoardOrder, boardOrder.Value);

            if (status == TaskState.Done)
                update = update.Set(x => x.Progress, 100);

            var result = await _collection.UpdateOneAsync(ById(companyId, id), update);

            if (result.MatchedCount == 0)
                throw new NotFoundException("وظیفه یافت نشد.");
        }

        public async Task ChangePriorityAsync(string companyId, string id, TaskPriority priority)
        {
            var update = Builders<TaskDocument>.Update
                .Set(x => x.Priority, priority)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(ById(companyId, id), update);

            if (result.MatchedCount == 0)
                throw new NotFoundException("وظیفه یافت نشد.");
        }

        public async Task ChangeAssigneesAsync(string companyId, string id, List<string> assigneeIds)
        {
            var update = Builders<TaskDocument>.Update
                .Set(x => x.AssigneeIds, assigneeIds)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(ById(companyId, id), update);

            if (result.MatchedCount == 0)
                throw new NotFoundException("وظیفه یافت نشد.");
        }

        public async Task SetArchivedAsync(string companyId, string id, bool isArchived)
        {
            var update = Builders<TaskDocument>.Update
                .Set(x => x.IsArchived, isArchived)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(ById(companyId, id), update);

            if (result.MatchedCount == 0)
                throw new NotFoundException("وظیفه یافت نشد.");
        }

        public async Task SetChecklistAsync(string companyId, string id, List<ChecklistItem> checklist, int progress)
        {
            var docs = checklist.Select(x => new ChecklistItemDocument
            {
                Id = x.Id,
                Title = x.Title,
                IsDone = x.IsDone,
                DoneByUserId = x.DoneByUserId,
                DoneAt = x.DoneAt
            }).ToList();

            var update = Builders<TaskDocument>.Update
                .Set(x => x.Checklist, docs)
                .Set(x => x.Progress, progress)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(ById(companyId, id), update);

            if (result.MatchedCount == 0)
                throw new NotFoundException("وظیفه یافت نشد.");
        }

        public async Task<double> GetNextBoardOrderAsync(string companyId, TaskState status)
        {
            var filter = F.And(F.Eq(x => x.CompanyId, companyId), F.Eq(x => x.Status, status));
            var last = await _collection
                .Find(filter)
                .Sort(Builders<TaskDocument>.Sort.Descending(x => x.BoardOrder))
                .Limit(1)
                .FirstOrDefaultAsync();

            return (last?.BoardOrder ?? 0) + BoardOrderGap;
        }

        public async Task<TaskStatistics> GetStatisticsAsync(string companyId, string? assigneeId)
        {
            var now = DateTime.UtcNow;
            var todayEnd = now.Date.AddDays(1);
            var weekStart = now.Date.AddDays(-7);

            var scope = BaseScope(companyId, assigneeId);
            var openScope = F.And(scope, F.Nin(x => x.Status, new[] { TaskState.Done, TaskState.Cancelled }));

            var counts = await Task.WhenAll(
                _collection.CountDocumentsAsync(scope),
                _collection.CountDocumentsAsync(F.And(scope, F.Eq(x => x.Status, TaskState.Todo))),
                _collection.CountDocumentsAsync(F.And(scope, F.Eq(x => x.Status, TaskState.InProgress))),
                _collection.CountDocumentsAsync(F.And(scope, F.Eq(x => x.Status, TaskState.InReview))),
                _collection.CountDocumentsAsync(F.And(scope, F.Eq(x => x.Status, TaskState.Done))),
                _collection.CountDocumentsAsync(F.And(scope, F.Eq(x => x.Status, TaskState.Cancelled))),
                _collection.CountDocumentsAsync(F.And(openScope, F.Lt(x => x.DueDate, now))),
                _collection.CountDocumentsAsync(F.And(openScope, F.Gte(x => x.DueDate, now), F.Lt(x => x.DueDate, todayEnd))),
                _collection.CountDocumentsAsync(F.And(scope, F.Size(x => x.AssigneeIds, 0))),
                _collection.CountDocumentsAsync(F.And(scope, F.Eq(x => x.Status, TaskState.Done), F.Gte(x => x.CompletedAt, weekStart)))
            );

            return new TaskStatistics
            {
                TotalCount = (int)counts[0],
                TodoCount = (int)counts[1],
                InProgressCount = (int)counts[2],
                InReviewCount = (int)counts[3],
                DoneCount = (int)counts[4],
                CancelledCount = (int)counts[5],
                OverdueCount = (int)counts[6],
                DueTodayCount = (int)counts[7],
                UnassignedCount = (int)counts[8],
                CompletedThisWeekCount = (int)counts[9]
            };
        }

        public async Task<TaskPriorityStatistics> GetPriorityStatisticsAsync(string companyId, string? assigneeId)
        {
            var scope = F.And(BaseScope(companyId, assigneeId), F.Nin(x => x.Status, new[] { TaskState.Done, TaskState.Cancelled }));

            var counts = await Task.WhenAll(
                _collection.CountDocumentsAsync(F.And(scope, F.Eq(x => x.Priority, TaskPriority.Low))),
                _collection.CountDocumentsAsync(F.And(scope, F.Eq(x => x.Priority, TaskPriority.Medium))),
                _collection.CountDocumentsAsync(F.And(scope, F.Eq(x => x.Priority, TaskPriority.High))),
                _collection.CountDocumentsAsync(F.And(scope, F.Eq(x => x.Priority, TaskPriority.Urgent)))
            );

            return new TaskPriorityStatistics
            {
                LowCount = (int)counts[0],
                MediumCount = (int)counts[1],
                HighCount = (int)counts[2],
                UrgentCount = (int)counts[3]
            };
        }

        public async Task<List<MemberWorkload>> GetWorkloadAsync(string companyId)
        {
            var now = DateTime.UtcNow;
            var filter = F.And(F.Eq(x => x.CompanyId, companyId), F.Eq(x => x.IsArchived, false));

            var docs = await _collection
                .Find(filter)
                .Project(x => new { x.AssigneeIds, x.Status, x.DueDate })
                .ToListAsync();

            var workloads = new Dictionary<string, MemberWorkload>();

            foreach (var doc in docs)
            {
                foreach (var userId in doc.AssigneeIds ?? new List<string>())
                {
                    if (!workloads.TryGetValue(userId, out var workload))
                    {
                        workload = new MemberWorkload { UserId = userId };
                        workloads[userId] = workload;
                    }

                    workload.TotalCount++;

                    if (doc.Status == TaskState.Done)
                        workload.DoneCount++;
                    else if (doc.Status != TaskState.Cancelled)
                    {
                        workload.OpenCount++;
                        if (doc.DueDate.HasValue && doc.DueDate.Value < now)
                            workload.OverdueCount++;
                    }
                }
            }

            return workloads.Values.OrderByDescending(x => x.OpenCount).ToList();
        }

        public async Task<List<TaskItem>> GetUpcomingAsync(string companyId, string? assigneeId, int days, int limit)
        {
            var now = DateTime.UtcNow;
            var until = now.AddDays(days);

            var filter = F.And(
                BaseScope(companyId, assigneeId),
                F.Nin(x => x.Status, new[] { TaskState.Done, TaskState.Cancelled }),
                F.Ne(x => x.DueDate, null),
                F.Lte(x => x.DueDate, until));

            var docs = await _collection
                .Find(filter)
                .Sort(Builders<TaskDocument>.Sort.Ascending(x => x.DueDate))
                .Limit(limit)
                .ToListAsync();

            return docs.Select(TaskMapper.ToDomain).ToList();
        }

        public async Task<int> CountByProjectAsync(string companyId, string projectId)
        {
            var filter = F.And(F.Eq(x => x.CompanyId, companyId), F.Eq(x => x.ProjectId, projectId));
            return (int)await _collection.CountDocumentsAsync(filter);
        }

        private static FilterDefinition<TaskDocument> BaseScope(string companyId, string? assigneeId)
        {
            var filter = F.And(F.Eq(x => x.CompanyId, companyId), F.Eq(x => x.IsArchived, false));

            return string.IsNullOrWhiteSpace(assigneeId)
                ? filter
                : F.And(filter, F.AnyEq(x => x.AssigneeIds, assigneeId));
        }

        private static FilterDefinition<TaskDocument> BuildFilter(string companyId, TaskSearchCriteria? criteria, string? currentUserId)
        {
            var filters = new List<FilterDefinition<TaskDocument>> { F.Eq(x => x.CompanyId, companyId) };

            if (criteria == null)
            {
                filters.Add(F.Eq(x => x.IsArchived, false));
                return F.And(filters);
            }

            if (!criteria.IncludeArchived)
                filters.Add(F.Eq(x => x.IsArchived, false));

            if (criteria.Status.HasValue)
                filters.Add(F.Eq(x => x.Status, criteria.Status.Value));

            if (criteria.Priority.HasValue)
                filters.Add(F.Eq(x => x.Priority, criteria.Priority.Value));

            if (!string.IsNullOrWhiteSpace(criteria.ProjectId))
                filters.Add(F.Eq(x => x.ProjectId, criteria.ProjectId));

            if (criteria.OnlyAssignedToMe && !string.IsNullOrWhiteSpace(currentUserId))
                filters.Add(F.AnyEq(x => x.AssigneeIds, currentUserId));
            else if (!string.IsNullOrWhiteSpace(criteria.AssigneeId))
                filters.Add(F.AnyEq(x => x.AssigneeIds, criteria.AssigneeId));

            if (!string.IsNullOrWhiteSpace(criteria.CreatedByUserId))
                filters.Add(F.Eq(x => x.CreatedByUserId, criteria.CreatedByUserId));

            if (criteria.IsAssigned.HasValue)
            {
                filters.Add(criteria.IsAssigned.Value
                    ? F.SizeGt(x => x.AssigneeIds, 0)
                    : F.Size(x => x.AssigneeIds, 0));
            }

            if (criteria.OnlyOverdue)
            {
                filters.Add(F.Lt(x => x.DueDate, DateTime.UtcNow));
                filters.Add(F.Nin(x => x.Status, new[] { TaskState.Done, TaskState.Cancelled }));
            }

            if (criteria.DueFrom.HasValue)
                filters.Add(F.Gte(x => x.DueDate, criteria.DueFrom.Value));

            if (criteria.DueTo.HasValue)
                filters.Add(F.Lte(x => x.DueDate, criteria.DueTo.Value));

            if (criteria.CreatedFrom.HasValue)
                filters.Add(F.Gte(x => x.CreatedAt, criteria.CreatedFrom.Value));

            if (criteria.CreatedTo.HasValue)
                filters.Add(F.Lte(x => x.CreatedAt, criteria.CreatedTo.Value));

            if (criteria.Labels is { Count: > 0 })
                filters.Add(F.AnyIn(x => x.Labels, criteria.Labels));

            if (!string.IsNullOrWhiteSpace(criteria.SearchText))
            {
                var regex = new BsonRegularExpression(Regex.Escape(criteria.SearchText.Trim()), "i");
                filters.Add(F.Or(
                    F.Regex(x => x.Title, regex),
                    F.Regex(x => x.Description, regex),
                    F.Regex(x => x.Code, regex)));
            }

            return F.And(filters);
        }
    }
}
