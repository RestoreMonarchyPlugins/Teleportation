using System.Collections.Generic;
using Rocket.API.User;

namespace RestoreMonarchy.TeleportationPlugin.Requests
{
    public interface ITeleportationRequestManager
    {
        void AddRequest(IUser from, IUser to);

        IEnumerable<PlayerTeleportRequest> GetRequestsForSender(IUser user);

        IEnumerable<PlayerTeleportRequest> GetRequestsForReceiver(IUser user);

        void Clear(IUser user = null);

        void Process(PlayerTeleportRequest request);

        void Remove(PlayerTeleportRequest request);
    }
}