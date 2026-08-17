using NovinTamas.Framework.Application;
using NovinTamas.Framework.Application.Exceptions;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Application.Contracts.Query.Task;
using NovinTamas.TaskManager.Application.Contracts.QueryResult.Common;
using NovinTamas.TaskManager.Application.Contracts.QueryResult.Task;
using NovinTamas.TaskManager.Application.Mapper;
using NovinTamas.TaskManager.Domain;
using NovinTamas.TaskManager.Domain.Common;
using NovinTamas.TaskManager.Domain.Enums;
using NovinTamas.TaskManager.Domain.Models.Projects;
using NovinTamas.TaskManager.Domain.Models.Tasks;

namespace NovinTamas.TaskManager.Application.QueryHandlers
{
    public class TaskQueryHandler :
        IQueryHandler<GetPagedTasksQuery, PagedQueryResult<TaskSummaryResult>>,
        IQueryHandler<GetTaskBoardQuery, GetTaskBoardQueryResult>,
        IQueryHandler<GetTaskByIdQuery, GetTaskByIdQueryResult>,
        IQueryHandler<GetTaskHistoryQuery, List<GetTaskHistoryQueryResult>>,
        IQueryHandler<GetTaskStatisticsQuery, GetTaskStatisticsQueryResult>,
        IQueryHandler<GetTeamWorkloadQuery, List<MemberWorkloadResult>>,
        IQueryHandler<GetUpcomingTasksQuery, List<TaskSummaryResult>>,
        IQueryHandler<GetTaskConfigQuery, GetTaskConfigQueryResult>
    {
        private static readonly TaskState[] BoardColumns =
        {
            TaskState.Todo, TaskState.InProgress, TaskState.InReview, TaskState.Done, TaskState.Cancelled
        };

        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserInfoService _userInfoService;
        private readonly IUserService _userService;
        private readonly IPermissionResolver _permissions;

        public TaskQueryHandler(
            IUnitOfWork unitOfWork,
            IUserInfoService userInfoService,
            IUserService userService,
            IPermissionResolver permissions)
        {
            _unitOfWork = unitOfWork;
            _userInfoService = userInfoService;
            _userService = userService;
            _permissions = permissions;
        }

        public async Task<PagedQueryResult<TaskSummaryResult>> Handle(GetPagedTasksQuery query)
        {
            var user = _userInfoService.GetCurrentUser();
            var criteria = ScopeCriteria(user, await _permissions.GetCurrentAsync(), query.Criteria);

            var paged = await _unitOfWork.TaskRepository.GetPagedAsync(user.CompanyId, query.Options ?? new QueryOptions(), criteria, user.UserId);
            var items = await MapAsync(user, paged.Items);

            return new PagedQueryResult<TaskSummaryResult>
            {
                Items = items,
                TotalCount = paged.TotalCount,
                Skip = paged.Skip,
                Limit = paged.Limit
            };
        }

        public async Task<GetTaskBoardQueryResult> Handle(GetTaskBoardQuery query)
        {
            var user = _userInfoService.GetCurrentUser();
            var criteria = ScopeCriteria(user, await _permissions.GetCurrentAsync(), query.Criteria);

            var perColumn = Math.Clamp(query.PerColumnLimit, 10, 200);
            var tasks = await _unitOfWork.TaskRepository.GetBoardAsync(user.CompanyId, criteria, user.UserId, perColumn);
            var mapped = await MapAsync(user, tasks);

            var columns = BoardColumns.Select(state =>
            {
                var columnItems = mapped.Where(x => x.Status == (int)state).ToList();

                return new BoardColumnResult
                {
                    Status = (int)state,
                    StatusName = state.ToPersian(),
                    TotalCount = columnItems.Count,
                    Items = columnItems.Take(perColumn).ToList()
                };
            }).ToList();

            return new GetTaskBoardQueryResult { Columns = columns };
        }

        public async Task<GetTaskByIdQueryResult> Handle(GetTaskByIdQuery query)
        {
            var user = _userInfoService.GetCurrentUser();

            var task = await _unitOfWork.TaskRepository.GetByIdAsync(user.CompanyId, query.Id)
                       ?? throw new NotFoundException("وظیفه یافت نشد.");

            TaskAccess.EnsureView(user, await _permissions.GetCurrentAsync(), task);

            var memberNames = await _userService.GetMemberNamesAsync(user.CompanyId, task.AssigneeIds);
            var projects = await LoadProjectsAsync(user.CompanyId, new[] { task });
            var commentCounts = await _unitOfWork.CommentRepository.GetCountsByTaskIdsAsync(user.CompanyId, new[] { task.Id! });

            return TaskResultMapper.ToDetail(task, user, memberNames, projects, commentCounts, DateTime.UtcNow);
        }

        public async Task<List<GetTaskHistoryQueryResult>> Handle(GetTaskHistoryQuery query)
        {
            var user = _userInfoService.GetCurrentUser();

            var task = await _unitOfWork.TaskRepository.GetByIdAsync(user.CompanyId, query.TaskId)
                       ?? throw new NotFoundException("وظیفه یافت نشد.");

            TaskAccess.EnsureView(user, await _permissions.GetCurrentAsync(), task);

            var histories = await _unitOfWork.HistoryRepository.GetByTaskIdAsync(user.CompanyId, query.TaskId);

            return histories.Select(x => new GetTaskHistoryQueryResult
            {
                Id = x.Id!,
                UserId = x.UserId,
                UserName = x.UserName,
                Action = (int)x.Action,
                ActionName = x.Action.ToPersian(),
                CreatedAt = x.CreatedAt,
                Changes = x.Changes.ToDictionary(c => c.Key, c => new ChangeDetailResult
                {
                    FieldName = c.Value.FieldName,
                    OldValue = c.Value.OldValue,
                    NewValue = c.Value.NewValue
                })
            }).ToList();
        }

        public async Task<GetTaskStatisticsQueryResult> Handle(GetTaskStatisticsQuery query)
        {
            var user = _userInfoService.GetCurrentUser();
            var assigneeId = ResolveScopeUserId(user, await _permissions.GetCurrentAsync(), query.OnlyMine);

            var stats = await _unitOfWork.TaskRepository.GetStatisticsAsync(user.CompanyId, assigneeId);
            var priority = await _unitOfWork.TaskRepository.GetPriorityStatisticsAsync(user.CompanyId, assigneeId);

            return new GetTaskStatisticsQueryResult
            {
                TotalCount = stats.TotalCount,
                TodoCount = stats.TodoCount,
                InProgressCount = stats.InProgressCount,
                InReviewCount = stats.InReviewCount,
                DoneCount = stats.DoneCount,
                CancelledCount = stats.CancelledCount,
                OverdueCount = stats.OverdueCount,
                DueTodayCount = stats.DueTodayCount,
                UnassignedCount = stats.UnassignedCount,
                CompletedThisWeekCount = stats.CompletedThisWeekCount,
                CompletionRate = stats.TotalCount == 0 ? 0 : (int)Math.Round(stats.DoneCount * 100d / stats.TotalCount),
                LowCount = priority.LowCount,
                MediumCount = priority.MediumCount,
                HighCount = priority.HighCount,
                UrgentCount = priority.UrgentCount
            };
        }

        public async Task<List<MemberWorkloadResult>> Handle(GetTeamWorkloadQuery query)
        {
            var user = _userInfoService.GetCurrentUser();
            TaskAccess.EnsureCompanyOwner(user);

            var workloads = await _unitOfWork.TaskRepository.GetWorkloadAsync(user.CompanyId);
            var names = await _userService.GetMemberNamesAsync(user.CompanyId, workloads.Select(x => x.UserId));

            return workloads.Select(x => new MemberWorkloadResult
            {
                UserId = x.UserId,
                FullName = names.TryGetValue(x.UserId, out var name) && !string.IsNullOrWhiteSpace(name) ? name : "کاربر حذف‌شده",
                TotalCount = x.TotalCount,
                OpenCount = x.OpenCount,
                DoneCount = x.DoneCount,
                OverdueCount = x.OverdueCount
            }).ToList();
        }

        public async Task<List<TaskSummaryResult>> Handle(GetUpcomingTasksQuery query)
        {
            var user = _userInfoService.GetCurrentUser();
            var assigneeId = ResolveScopeUserId(user, await _permissions.GetCurrentAsync(), query.OnlyMine);

            var tasks = await _unitOfWork.TaskRepository.GetUpcomingAsync(
                user.CompanyId, assigneeId, Math.Clamp(query.Days, 1, 60), Math.Clamp(query.Limit, 1, 50));

            return await MapAsync(user, tasks);
        }

        public async Task<GetTaskConfigQueryResult> Handle(GetTaskConfigQuery query)
        {
            var user = _userInfoService.GetCurrentUser();

            var members = await _userService.GetCompanyMembersAsync(user.CompanyId);
            var labels = await GetUsedLabelsAsync(user.CompanyId);
            var perms = await _permissions.GetCurrentAsync();

            // پرسنلِ دارای دسترسی «ارجاع به مدیر»، مدیر شرکت را هم در لیست مسئولین می‌بیند
            var assignables = members
                .Where(x => !x.IsBan)
                .Select(x => new AssigneeSummary { Id = x.Id, FullName = x.FullName })
                .ToList();

            if (!user.IsCompanyOwner && perms.CanAssignToManager)
            {
                var managerName = await _userService.GetCompanyNameAsync(user.CompanyId);

                assignables.Insert(0, new AssigneeSummary
                {
                    Id = user.CompanyId,
                    FullName = string.IsNullOrWhiteSpace(managerName) ? "مدیر شرکت" : $"{managerName} (مدیر)"
                });
            }

            return new GetTaskConfigQueryResult
            {
                CanCreateTask = perms.CanCreateTask,
                CanCreateForOthers = perms.CanCreateForOthers,
                CanAssignToOthers = perms.CanAssignToOthers,
                CanViewOthersTasks = perms.CanViewOthersTasks,
                CanAssignToManager = perms.CanAssignToManager,
                Status = Enum.GetValues<TaskState>()
                    .Select(x => new ConfigItemResult { Value = (int)x, Name = x.ToPersian() })
                    .ToList(),
                Priority = Enum.GetValues<TaskPriority>()
                    .Select(x => new ConfigItemResult { Value = (int)x, Name = x.ToPersian() })
                    .ToList(),
                Members = assignables,
                Labels = labels,
                IsCompanyOwner = user.IsCompanyOwner,
                CurrentUserId = user.UserId,
                ManagerUserId = user.CompanyId
            };
        }

        #region Internal

        // پرسنلِ بدون دسترسیِ «دیدن وظایف دیگران» فقط وظایف خودش را می‌بیند؛
        // این قید در سرور اعمال می‌شود نه در کلاینت.
        private static TaskSearchCriteria ScopeCriteria(CurrentUser user, MemberPermissions perms, TaskSearchCriteria? criteria)
        {
            var scoped = criteria ?? new TaskSearchCriteria();

            if (!user.IsCompanyOwner && !perms.CanViewOthersTasks)
            {
                scoped.OnlyAssignedToMe = true;
                scoped.AssigneeId = null;
            }

            return scoped;
        }

        private static string? ResolveScopeUserId(CurrentUser user, MemberPermissions perms, bool onlyMine) =>
            user.IsCompanyOwner || perms.CanViewOthersTasks
                ? (onlyMine ? user.UserId : null)
                : user.UserId;

        private async Task<List<TaskSummaryResult>> MapAsync(CurrentUser user, List<TaskItem> tasks)
        {
            if (tasks.Count == 0)
                return new List<TaskSummaryResult>();

            var memberNames = await _userService.GetMemberNamesAsync(user.CompanyId, tasks.SelectMany(x => x.AssigneeIds));
            var projects = await LoadProjectsAsync(user.CompanyId, tasks);
            var commentCounts = await _unitOfWork.CommentRepository.GetCountsByTaskIdsAsync(user.CompanyId, tasks.Select(x => x.Id!));
            var now = DateTime.UtcNow;

            return tasks.Select(x => TaskResultMapper.ToSummary(x, memberNames, projects, commentCounts, now)).ToList();
        }

        private async Task<Dictionary<string, Project>> LoadProjectsAsync(string companyId, IEnumerable<TaskItem> tasks)
        {
            var ids = tasks
                .Select(x => x.ProjectId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .Distinct()
                .ToList();

            return ids.Count == 0
                ? new Dictionary<string, Project>()
                : await _unitOfWork.ProjectRepository.GetMapByIdsAsync(companyId, ids);
        }

        // برچسب‌ها آزادند و جایی تعریف نمی‌شوند؛ لیست پیشنهاد از روی وظایف موجود ساخته می‌شود.
        private async Task<List<string>> GetUsedLabelsAsync(string companyId)
        {
            var paged = await _unitOfWork.TaskRepository.GetPagedAsync(
                companyId,
                new QueryOptions { Skip = 0, Limit = 200, SortBy = "createdAt", Descending = true },
                new TaskSearchCriteria { IncludeArchived = true },
                null);

            return paged.Items
                .SelectMany(x => x.Labels)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .GroupBy(x => x)
                .OrderByDescending(x => x.Count())
                .Select(x => x.Key)
                .Take(30)
                .ToList();
        }

        #endregion
    }
}
