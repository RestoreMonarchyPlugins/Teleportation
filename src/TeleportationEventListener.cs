using System.Threading.Tasks;
using RestoreMonarchy.TeleportationPlugin.Requests;
using Rocket.API.Eventing;
using Rocket.Core.Eventing;
using Rocket.Core.Player.Events;

namespace RestoreMonarchy.TeleportationPlugin
{
    public class TeleportationEventListener : IEventListener<PlayerDisconnectedEvent>
    {
        private readonly ITeleportationRequestManager requestsManager;

        public TeleportationEventListener(ITeleportationRequestManager requestsManager)
        {
            this.requestsManager = requestsManager;
        }

        [EventHandler]
        public async Task HandleEventAsync(IEventEmitter emitter, PlayerDisconnectedEvent @event)
        {
            requestsManager.Clear(@event.Player.User);
        }
    }
}