using NovinTamas.Framework.Application;
using NovinTamas.Framework.Application.Exceptions;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Application.Contracts.Query.Project;
using NovinTamas.TaskManager.Application.Contracts.QueryResult.Project;
using NovinTamas.TaskManager.Domain;
using NovinTamas.TaskManager.Domain.Common;
using NovinTamas.TaskManager.Domain.Enums;

namespace NovinTamas.TaskManager.Application.QueryHandlers
{
    public class ProjectQueryHandler :
        IQueryHandler<GetProjectsQuery, List<GetProjectQueryResult>>,
        IQueryHandler<GetProjectByIdQuery, GetProjectQueryResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserInfoService _userInfoService;

        public ProjectQueryHandler(IUnitOfWork unitOfWork, IUserInfoService userInfoService)
        {
            _unitOfWork = unitOfWork;
            _userInfoService = userInfoService;
        }

        public async Task<List<GetProjectQueryResult>> Handle(GetProjectsQuery query)
        {
            var user = _userInfoService.GetCurrentUser();
            var projects = await _unitOfWork.ProjectRepository.GetByCompanyIdAsync(user.CompanyId, query.IncludeArchived);

            if (projects.Count == 0)
                return new List<GetProjectQueryResult>();

            // شمارش وظایف هر پروژه با یک کوئری، به‌جای یک کوئری به ازای هر پروژه
            var paged = await _unitOfWork.TaskRepository.GetPagedAsync(
                user.CompanyId,
                new QueryOptions { Skip = 0, Limit = 2000, SortBy = "createdAt", Descending = true },
                new TaskSearchCriteria(),
                user.UserId);

            var grouped = paged.Items
                .Where(x => !string.IsNullOrWhiteSpace(x.ProjectId))
                .GroupBy(x => x.ProjectId!)
                .ToDictionary(x => x.Key, x => (Total: x.Count(), Done: x.Count(t => t.Status == TaskState.Done)));

            return projects.Select(project =>
            {
                grouped.TryGetValue(project.Id!, out var counts);

                return new GetProjectQueryResult
                {
                    Id = project.Id!,
                    Name = project.Name,
                    Description = project.Description,
                    Color = project.Color,
                    MemberIds = project.MemberIds,
                    IsArchived = project.IsArchived,
                    CreatedAt = project.CreatedAt,
                    TaskCount = counts.Total,
                    DoneCount = counts.Done
                };
            }).ToList();
        }

        public async Task<GetProjectQueryResult> Handle(GetProjectByIdQuery query)
        {
            var user = _userInfoService.GetCurrentUser();

            var project = await _unitOfWork.ProjectRepository.GetByIdAsync(user.CompanyId, query.Id)
                          ?? throw new NotFoundException("پروژه یافت نشد.");

            return new GetProjectQueryResult
            {
                Id = project.Id!,
                Name = project.Name,
                Description = project.Description,
                Color = project.Color,
                MemberIds = project.MemberIds,
                IsArchived = project.IsArchived,
                CreatedAt = project.CreatedAt,
                TaskCount = await _unitOfWork.TaskRepository.CountByProjectAsync(user.CompanyId, project.Id!)
            };
        }
    }
}
