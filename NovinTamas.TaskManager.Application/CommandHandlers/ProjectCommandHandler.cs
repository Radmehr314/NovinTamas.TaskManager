using NovinTamas.Framework.Application;
using NovinTamas.Framework.Application.Exceptions;
using NovinTamas.TaskManager.Application.Contracts.Commands.Project;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Application.Mapper;
using NovinTamas.TaskManager.Domain;
using NovinTamas.TaskManager.Domain.Models.Projects;

namespace NovinTamas.TaskManager.Application.CommandHandlers
{
    public class ProjectCommandHandler :
        ICommandHandler<CreateProjectCommand>,
        ICommandHandler<UpdateProjectCommand>,
        ICommandHandler<ArchiveProjectCommand>,
        ICommandHandler<DeleteProjectCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserInfoService _userInfoService;

        public ProjectCommandHandler(IUnitOfWork unitOfWork, IUserInfoService userInfoService)
        {
            _unitOfWork = unitOfWork;
            _userInfoService = userInfoService;
        }

        public async Task<CommandResult> Handle(CreateProjectCommand command)
        {
            var user = _userInfoService.GetCurrentUser();
            TaskAccess.EnsureCompanyOwner(user);

            if (string.IsNullOrWhiteSpace(command.Name))
                throw new ArgumentException("نام پروژه الزامی است.");

            var project = new Project(user.CompanyId, command.Name.Trim())
            {
                Description = command.Description,
                Color = string.IsNullOrWhiteSpace(command.Color) ? "#5A6ACF" : command.Color,
                CreatedByUserId = user.UserId,
                MemberIds = command.MemberIds?.Distinct().ToList() ?? new List<string>()
            };

            var id = await _unitOfWork.ProjectRepository.AddAsync(project);
            return new CommandResult { Id = id };
        }

        public async Task<CommandResult> Handle(UpdateProjectCommand command)
        {
            var user = _userInfoService.GetCurrentUser();
            TaskAccess.EnsureCompanyOwner(user);

            var project = await _unitOfWork.ProjectRepository.GetByIdAsync(user.CompanyId, command.Id)
                          ?? throw new NotFoundException("پروژه یافت نشد.");

            if (string.IsNullOrWhiteSpace(command.Name))
                throw new ArgumentException("نام پروژه الزامی است.");

            project.Name = command.Name.Trim();
            project.Description = command.Description;
            project.Color = string.IsNullOrWhiteSpace(command.Color) ? project.Color : command.Color;
            project.MemberIds = command.MemberIds?.Distinct().ToList() ?? new List<string>();

            await _unitOfWork.ProjectRepository.UpdateAsync(project);
            return new CommandResult { Id = project.Id };
        }

        public async Task<CommandResult> Handle(ArchiveProjectCommand command)
        {
            var user = _userInfoService.GetCurrentUser();
            TaskAccess.EnsureCompanyOwner(user);

            await _unitOfWork.ProjectRepository.SetArchivedAsync(user.CompanyId, command.Id, command.IsArchived);
            return new CommandResult { Id = command.Id };
        }

        public async Task<CommandResult> Handle(DeleteProjectCommand command)
        {
            var user = _userInfoService.GetCurrentUser();
            TaskAccess.EnsureCompanyOwner(user);

            // حذف پروژه‌ای که وظیفه دارد وظایف را بی‌صاحب می‌کند، پس جلویش گرفته می‌شود
            var taskCount = await _unitOfWork.TaskRepository.CountByProjectAsync(user.CompanyId, command.Id);

            if (taskCount > 0)
                throw new ArgumentException($"این پروژه {taskCount} وظیفه دارد. ابتدا وظایف را منتقل یا حذف کنید، یا پروژه را بایگانی کنید.");

            await _unitOfWork.ProjectRepository.DeleteAsync(user.CompanyId, command.Id);
            return new CommandResult { Id = command.Id };
        }
    }
}
