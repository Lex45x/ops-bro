using OpsBro.Domain.Bind;
using OpsBro.Domain.Settings;

namespace OpsBro.Domain
{
    public class DomainModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            
            builder.RegisterType<JsonBindingService>().AsImplementedInterfaces();

            builder.RegisterType<SettingsSource>()
                .SingleInstance()
                .AutoActivate()
                .AsImplementedInterfaces();
        }
    }
}