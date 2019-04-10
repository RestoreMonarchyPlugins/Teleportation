using Rocket.API.User;

namespace RestoreMonarchy.TeleportationPlugin.Requests
{
    public class PlayerTeleportRequest
    {
        public IUser Receiver { get; set; }
        public IUser Sender { get; set; }
    }
}
