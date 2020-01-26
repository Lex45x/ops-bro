using OpsBro.Domain.Bind;
using OpsBro.Domain.Settings;

namespace OpsBro.Domain
{
    public class DomainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BindingService>().AsImplementedInterfaces();

            builder.RegisterType<SettingsSource>()
                .SingleInstance()
                .AutoActivate()
                .AsImplementedInterfaces();

            builder.RegisterGeneric(typeof(InlineValidator<>))
                .As(typeof(IValidator<>));

            builder.RegisterAssemblyTypes(ThisAssembly)
                .AsClosedTypesOf(typeof(ICommandHandler<>))
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(ThisAssembly)
                .AsClosedTypesOf(typeof(IQueryHandler<,>))
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(ThisAssembly)
                .AsClosedTypesOf(typeof(IEventSubscriber<>))
                .AsImplementedInterfaces()
                .WithAttributeFiltering();

            builder.RegisterAssemblyTypes(ThisAssembly)
                .AsClosedTypesOf(typeof(IValidator<>))
                .AsImplementedInterfaces();
        }
    }
}