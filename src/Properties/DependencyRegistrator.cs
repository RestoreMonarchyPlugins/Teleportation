using RestoreMonarchy.TeleportationPlugin.Requests;
using Rocket.API.DependencyInjection;

namespace RestoreMonarchy.TeleportationPlugin.Properties
{
    public class DependencyRegistrator : IDependencyRegistrator
    {
        public void Register(IDependencyContainer container, IDependencyResolver resolver)
        {
            container.RegisterSingletonType<ITeleportationRequestManager, TeleportationRequestManager>();
        }
    }
}
