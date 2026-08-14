using Autofac;
using Microsoft.Extensions.Configuration;
using NovinTamas.Framework.Application;
using NovinTamas.TaskManager.Application.CommandHandlers;
using NovinTamas.TaskManager.Application.Contracts.Contracts;
using NovinTamas.TaskManager.Application.Mapper;
using NovinTamas.TaskManager.Application.QueryHandlers;
using NovinTamas.TaskManager.Domain;
using NovinTamas.TaskManager.Domain.Models.OutboxMessages;
using NovinTamas.TaskManager.Infrastructure.Persistance;
using NovinTamas.TaskManager.Infrastructure.Persistance.Repositories;
using NovinTamas.TaskManager.Infrastructure.Persistance.Services;

namespace NovinTamas.TaskManager.Infrastructure.Config
{
    public class AutofacModule : Module
    {
        private readonly IConfiguration _cfg;

        public AutofacModule(IConfiguration cfg)
        {
            _cfg = cfg;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(TaskCommandHandler).Assembly)
                .As(type => type.GetInterfaces()
                    .Where(interfaceType => interfaceType.IsClosedTypeOf(typeof(ICommandHandler<>))))
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(typeof(TaskQueryHandler).Assembly)
                .As(type => type.GetInterfaces()
                    .Where(interfaceType => interfaceType.IsClosedTypeOf(typeof(IQueryHandler<,>))))
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(typeof(TaskItemRepository).Assembly)
                .Where(t => t.Name.EndsWith("Repository"))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();
            builder.RegisterType<UserInfoService>().As<IUserInfoService>().InstancePerLifetimeScope();
            builder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
            builder.RegisterType<FileStorageService>().As<IFileStorageService>().InstancePerLifetimeScope();
            builder.RegisterType<TaskActivityWriter>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<PermissionResolver>().As<IPermissionResolver>().InstancePerLifetimeScope();

            builder.RegisterType<AutofacCommandBus>().As<ICommandBus>().InstancePerLifetimeScope();
            builder.RegisterType<AutofacQueryBus>().As<IQueryBus>().InstancePerLifetimeScope();

            builder.RegisterType<OutboxMessageRepository>()
                .As<IOutboxMessageRepository>()
                .InstancePerLifetimeScope();

            builder.Register(ctx =>
            {
                var cfg = ctx.Resolve<IConfiguration>();

                return new EventPublisher(
                    cfg["Rabbit:Host"] ?? "localhost",
                    int.TryParse(cfg["Rabbit:Port"], out var port) ? port : 5672,
                    cfg["Rabbit:User"] ?? "guest",
                    cfg["Rabbit:Pass"] ?? "guest",
                    cfg["Rabbit:NotificationExchange"] ?? "novintamas.notifications");
            })
                .As<IEventPublisher>()
                .SingleInstance();

            builder.RegisterType<SessionRevocationCache>()
                .As<ISessionRevocationCache>()
                .SingleInstance();
        }
    }
}
