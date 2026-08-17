using NovinTamas.Framework.Application;
using NovinTamas.Framework.Application.Exceptions;
using NovinTamas.TaskManager.Application.Contracts.Commands.Task;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Application.Mapper;
using NovinTamas.TaskManager.Domain;
using NovinTamas.TaskManager.Domain.Enums;
using NovinTamas.TaskManager.Domain.Models.Histories;
using NovinTamas.TaskManager.Domain.Models.Tasks;

namespace NovinTamas.TaskManager.Application.CommandHandlers
{
    public class TaskCommandHandler :
        ICommandHandler<CreateTaskCommand>,
        ICommandHandler<UpdateTaskCommand>,
        ICommandHandler<ChangeTaskStatusCommand>,
        ICommandHandler<ChangeTaskPriorityCommand>,
        ICommandHandler<AssignTaskCommand>,
        ICommandHandler<SetTaskProgressCommand>,
        ICommandHandler<AddChecklistItemCommand>,
        ICommandHandler<ToggleChecklistItemCommand>,
        ICommandHandler<DeleteChecklistItemCommand>,
        ICommandHandler<ArchiveTaskCommand>,
        ICommandHandler<DeleteTaskCommand>
    {
        private const double BoardOrderGap = 1024d;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserInfoService _userInfoService;
        private readonly IUserService _userService;
        private readonly IPermissionResolver _permissions;
        private readonly TaskActivityWriter _activity;

        public TaskCommandHandler(
            IUnitOfWork unitOfWork,
            IUserInfoService userInfoService,
            IUserService userService,
            IPermissionResolver permissions,
            TaskActivityWriter activity)
        {
            _unitOfWork = unitOfWork;
            _userInfoService = userInfoService;
            _userService = userService;
            _permissions = permissions;
            _activity = activity;
        }

        public async Task<CommandResult> Handle(CreateTaskCommand command)
        {
            var user = _userInfoService.GetCurrentUser();

            if (string.IsNullOrWhiteSpace(command.Title))
                throw new ArgumentException("عنوان وظیفه الزامی است.");

            var assignees = await ValidateAssigneesAsync(user, command.AssigneeIds);
            await EnsureCanCreateAsync(user, assignees);
            await ValidateProjectAsync(user, command.ProjectId);

            // وظیفه‌ی بدون مسئولی که پرسنل ساخته در «وظایف من» خودش دیده نمی‌شود،
            // پس خودش مسئول پیش‌فرض می‌شود. برای مدیر شرکت بی‌مسئول ماندن مجاز است.
            if (!user.IsCompanyOwner && assignees.Count == 0)
                assignees.Add(user.UserId);

            var task = new TaskItem(user.CompanyId, command.Title.Trim())
            {
                Description = command.Description,
                ProjectId = string.IsNullOrWhiteSpace(command.ProjectId) ? null : command.ProjectId,
                CreatedByUserId = user.UserId,
                CreatedByUserName = await _userService.GetDisplayNameAsync(user.CompanyId, user.UserId, user.Role),
                CreatedByRole = user.Role,
                AssigneeIds = assignees,
                Priority = command.Priority,
                Status = command.Status ?? TaskState.Todo,
                StartDate = command.StartDate,
                DueDate = command.DueDate,
                EstimatedHours = command.EstimatedHours,
                Labels = NormalizeLabels(command.Labels),
                Files = command.Files ?? new List<string>(),
                Checklist = command.ChecklistTitles
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => new ChecklistItem { Title = x.Trim() })
                    .ToList()
            };

            var taskId = await _unitOfWork.TaskRepository.AddAsync(task);
            task.Id = taskId;

            await _activity.WriteHistoryAsync(user, taskId, HistoryAction.Created, new Dictionary<string, ChangeDetail>
            {
                ["Title"] = new() { FieldName = "عنوان", NewValue = task.Title },
                ["Status"] = new() { FieldName = "وضعیت", NewValue = task.Status.ToPersian() },
                ["Priority"] = new() { FieldName = "اولویت", NewValue = task.Priority.ToPersian() }
            }, command.IpAddress, command.UserAgent);

            await _activity.NotifyAsync(user, task, assignees, NotificationType.TaskAssigned,
                $"وظیفه‌ی «{task.Title}» به شما ارجاع شد");

            await _activity.PublishAsync("task.created", "taskmanager.task.created", new
            {
                taskId,
                companyId = user.CompanyId,
                title = task.Title,
                assigneeIds = assignees
            });

            return new CommandResult { Id = taskId };
        }

        public async Task<CommandResult> Handle(UpdateTaskCommand command)
        {
            var (user, task) = await LoadForEditAsync(command.Id);

            await ValidateProjectAsync(user, command.ProjectId);

            var changes = new Dictionary<string, ChangeDetail>();

            if (!string.IsNullOrWhiteSpace(command.Title) && command.Title.Trim() != task.Title)
            {
                changes["Title"] = new ChangeDetail { FieldName = "عنوان", OldValue = task.Title, NewValue = command.Title.Trim() };
                task.Title = command.Title.Trim();
            }

            if (command.Description != task.Description)
            {
                changes["Description"] = new ChangeDetail { FieldName = "توضیحات", OldValue = "—", NewValue = "—" };
                task.Description = command.Description;
            }

            var newProjectId = string.IsNullOrWhiteSpace(command.ProjectId) ? null : command.ProjectId;

            if (newProjectId != task.ProjectId)
            {
                changes["ProjectId"] = new ChangeDetail { FieldName = "پروژه", OldValue = task.ProjectId ?? "—", NewValue = newProjectId ?? "—" };
                task.ProjectId = newProjectId;
            }

            if (command.Priority != task.Priority)
            {
                changes["Priority"] = new ChangeDetail { FieldName = "اولویت", OldValue = task.Priority.ToPersian(), NewValue = command.Priority.ToPersian() };
                task.Priority = command.Priority;
            }

            if (command.DueDate != task.DueDate)
            {
                changes["DueDate"] = new ChangeDetail
                {
                    FieldName = "مهلت انجام",
                    OldValue = task.DueDate?.ToString("yyyy-MM-dd") ?? "—",
                    NewValue = command.DueDate?.ToString("yyyy-MM-dd") ?? "—"
                };
                task.DueDate = command.DueDate;
            }

            task.StartDate = command.StartDate;
            task.EstimatedHours = command.EstimatedHours;
            task.Labels = NormalizeLabels(command.Labels);
            task.Files = command.Files ?? new List<string>();

            await _unitOfWork.TaskRepository.UpdateAsync(task);

            if (changes.Count > 0)
            {
                await _activity.WriteHistoryAsync(user, task.Id!, HistoryAction.Updated, changes, command.IpAddress, command.UserAgent);
                await _activity.NotifyAsync(user, task, task.AssigneeIds, NotificationType.DueDateChanged,
                    $"وظیفه‌ی «{task.Title}» به‌روزرسانی شد");
            }

            return new CommandResult { Id = task.Id };
        }

        public async Task<CommandResult> Handle(ChangeTaskStatusCommand command)
        {
            var user = _userInfoService.GetCurrentUser();
            var task = await LoadAsync(user, command.Id);

            // برخلاف ویرایش کامل، تغییر وضعیت برای مسئولِ وظیفه هم مجاز است
            await EnsureViewAsync(user, task);

            if (task.Status == command.Status && string.IsNullOrWhiteSpace(command.BeforeTaskId) && string.IsNullOrWhiteSpace(command.AfterTaskId))
                return new CommandResult { Id = task.Id };

            var oldStatus = task.Status;
            var boardOrder = await ResolveBoardOrderAsync(user.CompanyId, command);
            var completedAt = command.Status == TaskState.Done ? DateTime.UtcNow : (DateTime?)null;

            await _unitOfWork.TaskRepository.ChangeStatusAsync(user.CompanyId, task.Id!, command.Status, boardOrder, completedAt);

            if (oldStatus != command.Status)
            {
                await _activity.WriteHistoryAsync(user, task.Id!, HistoryAction.StatusChanged,
                    TaskActivityWriter.Change("Status", "وضعیت", oldStatus.ToPersian(), command.Status.ToPersian()),
                    command.IpAddress, command.UserAgent);

                var receivers = task.AssigneeIds.Append(task.CreatedByUserId);
                var type = command.Status == TaskState.Done ? NotificationType.TaskCompleted : NotificationType.StatusChanged;
                var message = command.Status == TaskState.Done
                    ? $"وظیفه‌ی «{task.Title}» انجام شد"
                    : $"وضعیت «{task.Title}» به {command.Status.ToPersian()} تغییر کرد";

                await _activity.NotifyAsync(user, task, receivers, type, message);

                await _activity.PublishAsync("task.status-changed", "taskmanager.task.status-changed", new
                {
                    taskId = task.Id,
                    companyId = user.CompanyId,
                    title = task.Title,
                    status = command.Status.ToString(),
                    statusName = command.Status.ToPersian()
                });
            }

            return new CommandResult { Id = task.Id };
        }

        public async Task<CommandResult> Handle(ChangeTaskPriorityCommand command)
        {
            var (user, task) = await LoadForEditAsync(command.Id);

            if (task.Priority == command.Priority)
                return new CommandResult { Id = task.Id };

            await _unitOfWork.TaskRepository.ChangePriorityAsync(user.CompanyId, task.Id!, command.Priority);

            await _activity.WriteHistoryAsync(user, task.Id!, HistoryAction.PriorityChanged,
                TaskActivityWriter.Change("Priority", "اولویت", task.Priority.ToPersian(), command.Priority.ToPersian()),
                command.IpAddress, command.UserAgent);

            await _activity.NotifyAsync(user, task, task.AssigneeIds, NotificationType.PriorityChanged,
                $"اولویت «{task.Title}» به {command.Priority.ToPersian()} تغییر کرد");

            return new CommandResult { Id = task.Id };
        }

        public async Task<CommandResult> Handle(AssignTaskCommand command)
        {
            var user = _userInfoService.GetCurrentUser();
            var task = await LoadAsync(user, command.Id);
            TaskAccess.EnsureAssign(user, await _permissions.GetCurrentAsync(), task);

            var assignees = await ValidateAssigneesAsync(user, command.AssigneeIds);
            var added = assignees.Except(task.AssigneeIds).ToList();
            var removed = task.AssigneeIds.Except(assignees).ToList();

            if (added.Count == 0 && removed.Count == 0)
                return new CommandResult { Id = task.Id };

            await _unitOfWork.TaskRepository.ChangeAssigneesAsync(user.CompanyId, task.Id!, assignees);

            var names = await _userService.GetMemberNamesAsync(user.CompanyId, task.AssigneeIds.Concat(assignees));

            await _activity.WriteHistoryAsync(user, task.Id!, HistoryAction.Assigned,
                TaskActivityWriter.Change("Assignees", "مسئولین",
                    FormatNames(task.AssigneeIds, names),
                    FormatNames(assignees, names)),
                command.IpAddress, command.UserAgent);

            await _activity.NotifyAsync(user, task, added, NotificationType.TaskAssigned,
                $"وظیفه‌ی «{task.Title}» به شما ارجاع شد");

            await _activity.NotifyAsync(user, task, removed, NotificationType.TaskUnassigned,
                $"شما از وظیفه‌ی «{task.Title}» حذف شدید");

            return new CommandResult { Id = task.Id };
        }

        public async Task<CommandResult> Handle(SetTaskProgressCommand command)
        {
            var user = _userInfoService.GetCurrentUser();
            var task = await LoadAsync(user, command.Id);
            await EnsureViewAsync(user, task);

            var progress = Math.Clamp(command.Progress, 0, 100);

            if (progress == task.Progress)
                return new CommandResult { Id = task.Id };

            var oldProgress = task.Progress;
            task.Progress = progress;
            await _unitOfWork.TaskRepository.UpdateAsync(task);

            await _activity.WriteHistoryAsync(user, task.Id!, HistoryAction.ProgressChanged,
                TaskActivityWriter.Change("Progress", "پیشرفت", $"{oldProgress}%", $"{progress}%"),
                command.IpAddress, command.UserAgent);

            return new CommandResult { Id = task.Id };
        }

        public async Task<CommandResult> Handle(AddChecklistItemCommand command)
        {
            var user = _userInfoService.GetCurrentUser();
            var task = await LoadAsync(user, command.TaskId);
            await EnsureViewAsync(user, task);

            if (string.IsNullOrWhiteSpace(command.Title))
                throw new ArgumentException("عنوان آیتم چک‌لیست الزامی است.");

            task.Checklist.Add(new ChecklistItem { Title = command.Title.Trim() });
            await SaveChecklistAsync(user, task);

            await _activity.WriteHistoryAsync(user, task.Id!, HistoryAction.ChecklistChanged,
                TaskActivityWriter.Change("Checklist", "چک‌لیست", "—", $"افزودن «{command.Title.Trim()}»"),
                command.IpAddress, command.UserAgent);

            return new CommandResult { Id = task.Id };
        }

        public async Task<CommandResult> Handle(ToggleChecklistItemCommand command)
        {
            var user = _userInfoService.GetCurrentUser();
            var task = await LoadAsync(user, command.TaskId);
            await EnsureViewAsync(user, task);

            var item = task.Checklist.FirstOrDefault(x => x.Id == command.ItemId)
                       ?? throw new NotFoundException("آیتم چک‌لیست یافت نشد.");

            item.IsDone = command.IsDone;
            item.DoneByUserId = command.IsDone ? user.UserId : null;
            item.DoneAt = command.IsDone ? DateTime.UtcNow : null;

            await SaveChecklistAsync(user, task);

            return new CommandResult { Id = task.Id };
        }

        public async Task<CommandResult> Handle(DeleteChecklistItemCommand command)
        {
            var user = _userInfoService.GetCurrentUser();
            var task = await LoadAsync(user, command.TaskId);
            await EnsureViewAsync(user, task);

            task.Checklist.RemoveAll(x => x.Id == command.ItemId);
            await SaveChecklistAsync(user, task);

            return new CommandResult { Id = task.Id };
        }

        public async Task<CommandResult> Handle(ArchiveTaskCommand command)
        {
            var (user, task) = await LoadForEditAsync(command.Id);

            await _unitOfWork.TaskRepository.SetArchivedAsync(user.CompanyId, task.Id!, command.IsArchived);

            await _activity.WriteHistoryAsync(user, task.Id!,
                command.IsArchived ? HistoryAction.Archived : HistoryAction.Restored,
                new Dictionary<string, ChangeDetail>(), command.IpAddress, command.UserAgent);

            return new CommandResult { Id = task.Id };
        }

        public async Task<CommandResult> Handle(DeleteTaskCommand command)
        {
            var user = _userInfoService.GetCurrentUser();
            var task = await LoadAsync(user, command.Id);
            TaskAccess.EnsureDelete(user, task);

            await _unitOfWork.TaskRepository.DeleteAsync(user.CompanyId, task.Id!);
            await _unitOfWork.CommentRepository.DeleteByTaskIdAsync(user.CompanyId, task.Id!);
            await _unitOfWork.HistoryRepository.DeleteByTaskIdAsync(user.CompanyId, task.Id!);
            await _unitOfWork.NotificationRepository.DeleteByTaskIdAsync(user.CompanyId, task.Id!);

            return new CommandResult { Id = task.Id };
        }

        #region Internal

        private async Task<TaskItem> LoadAsync(CurrentUser user, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("شناسه‌ی وظیفه الزامی است.");

            return await _unitOfWork.TaskRepository.GetByIdAsync(user.CompanyId, id)
                   ?? throw new NotFoundException("وظیفه یافت نشد.");
        }

        private async Task<(CurrentUser user, TaskItem task)> LoadForEditAsync(string id)
        {
            var user = _userInfoService.GetCurrentUser();
            var task = await LoadAsync(user, id);
            TaskAccess.EnsureEdit(user, task);
            return (user, task);
        }

        private async Task EnsureViewAsync(CurrentUser user, TaskItem task)
        {
            TaskAccess.EnsureView(user, await _permissions.GetCurrentAsync(), task);
        }

        // ساخت وظیفه برای خود و برای دیگران دو دسترسی جداگانه‌اند؛ مدیر شرکت هر دو را دارد.
        private async Task EnsureCanCreateAsync(CurrentUser user, List<string> assignees)
        {
            if (user.IsCompanyOwner) return;

            var perms = await _permissions.GetCurrentAsync();

            if (!perms.CanCreateTask)
                throw new UserAccessException("اجازه‌ی ایجاد وظیفه را ندارید.");

            // مدیر با دسترسی «ارجاع به مدیر» جدا حساب می‌شود و «ایجاد برای دیگران» را لازم ندارد
            var hasOthers = assignees.Any(id =>
                id != user.UserId && !(perms.CanAssignToManager && id == user.CompanyId));

            if (hasOthers && !perms.CanCreateForOthers)
                throw new UserAccessException("اجازه‌ی ایجاد وظیفه برای دیگران را ندارید.");
        }

        private async Task SaveChecklistAsync(CurrentUser user, TaskItem task)
        {
            await _unitOfWork.TaskRepository.SetChecklistAsync(user.CompanyId, task.Id!, task.Checklist, task.ResolveProgress());
        }

        // فقط پرسنل همان شرکت (یا خود مدیر) می‌توانند مسئول یک وظیفه باشند
        private async Task<List<string>> ValidateAssigneesAsync(CurrentUser user, List<string>? assigneeIds)
        {
            var requested = (assigneeIds ?? new List<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            if (requested.Count == 0)
                return new List<string>();

            var members = await _userService.GetCompanyMembersAsync(user.CompanyId);
            var allowed = members.Select(x => x.Id).Append(user.CompanyId).ToHashSet();

            var invalid = requested.Where(x => !allowed.Contains(x)).ToList();

            if (invalid.Count > 0)
                throw new ArgumentException("یک یا چند کاربر انتخاب‌شده جزو پرسنل این شرکت نیستند.");

            // ارجاع به مدیر شرکت دسترسی جداگانه دارد؛ شناسه‌ی مدیر همان companyId است.
            if (!user.IsCompanyOwner && requested.Contains(user.CompanyId))
            {
                var perms = await _permissions.GetCurrentAsync();

                if (!perms.CanAssignToManager)
                    throw new UserAccessException("اجازه‌ی ارجاع وظیفه به مدیر را ندارید.");
            }

            return requested;
        }

        private async Task ValidateProjectAsync(CurrentUser user, string? projectId)
        {
            if (string.IsNullOrWhiteSpace(projectId)) return;

            var project = await _unitOfWork.ProjectRepository.GetByIdAsync(user.CompanyId, projectId);

            if (project == null)
                throw new NotFoundException("پروژه یافت نشد.");
        }

        // با گرفتن کارت بالایی و پایینیِ محل رها شدن، ترتیب جدید وسط آن دو محاسبه می‌شود
        private async Task<double?> ResolveBoardOrderAsync(string companyId, ChangeTaskStatusCommand command)
        {
            var before = await GetOrderAsync(companyId, command.BeforeTaskId);
            var after = await GetOrderAsync(companyId, command.AfterTaskId);

            if (before.HasValue && after.HasValue)
                return (before.Value + after.Value) / 2;

            if (before.HasValue)
                return before.Value + BoardOrderGap;

            if (after.HasValue)
                return after.Value - BoardOrderGap;

            return await _unitOfWork.TaskRepository.GetNextBoardOrderAsync(companyId, command.Status);
        }

        private async Task<double?> GetOrderAsync(string companyId, string? taskId)
        {
            if (string.IsNullOrWhiteSpace(taskId)) return null;

            var task = await _unitOfWork.TaskRepository.GetByIdAsync(companyId, taskId);
            return task?.BoardOrder;
        }

        private static List<string> NormalizeLabels(List<string>? labels) =>
            (labels ?? new List<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct()
                .Take(10)
                .ToList();

        private static string FormatNames(IEnumerable<string> ids, IReadOnlyDictionary<string, string> names)
        {
            var list = ids.Select(id => names.TryGetValue(id, out var name) ? name : id).ToList();
            return list.Count == 0 ? "—" : string.Join("، ", list);
        }

        #endregion
    }
}
