using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Rocket.API.Scheduling;
using Rocket.API.User;
using Rocket.Core.Scheduling;
using Rocket.Core.User;
using Rocket.Unturned.Player;

namespace RestoreMonarchy.TeleportationPlugin.Requests
{
    public class TeleportationRequestManager : ITeleportationRequestManager
    {
        private readonly ITaskScheduler taskScheduler;
        private readonly List<PlayerTeleportRequest> pendingRequests = new List<PlayerTeleportRequest>();
        private TeleportationPlugin pluginInstance;

        public TeleportationRequestManager(ITaskScheduler taskScheduler)
        {
            this.taskScheduler = taskScheduler;
        }

        public void AddRequest(IUser sender, IUser target)
        {
            pendingRequests.Add(new PlayerTeleportRequest { Sender = sender, Receiver = target });
        }

        public IEnumerable<PlayerTeleportRequest> GetRequestsForSender(IUser user)
        {
            return pendingRequests.Where(d => d.Sender.Id == user.Id);
        }

        public IEnumerable<PlayerTeleportRequest> GetRequestsForReceiver(IUser user)
        {
            return pendingRequests.Where(d => d.Receiver.Id == user.Id);
        }

        public void Clear(IUser sender = null)
        {
            if (sender == null)
            {
                pendingRequests.Clear();
                return;
            }

            pendingRequests.RemoveAll(d => d.Sender.Id == sender.Id || d.Receiver.Id == sender.Id);
        }

        public void Process(PlayerTeleportRequest request)
        {
            taskScheduler.ScheduleDelayed(pluginInstance, async () => await ProcessUnturnedTeleportation(request), $"Teleportation Task {request.Sender.DisplayName}->{request.Receiver.DisplayName}", TimeSpan.FromSeconds(pluginInstance.ConfigurationInstance.TeleportationDelay), true);
        }

        public void Remove(PlayerTeleportRequest request)
        {
            pendingRequests.RemoveAll(x => x.Receiver.Id == request.Receiver.Id && x.Sender.Id == request.Sender.Id);
        }

        public void SetPluginInstance(TeleportationPlugin pluginInstance)
        {
            this.pluginInstance = pluginInstance;
        }

        public async Task ProcessUnturnedTeleportation(PlayerTeleportRequest request)
        {
            if (pluginInstance == null)
            {
                return;
            }

            var sender = request.Sender;
            var target = request.Receiver;

            UnturnedUser uTarget = (UnturnedUser)target;
            UnturnedUser uSender = (UnturnedUser)sender;

            if (!uTarget.Player.IsOnline || uTarget.Player.Entity.Dead)
            {
                await target.SendMessageAsync(await pluginInstance.Translations.GetAsync("TP_Dead", sender.DisplayName), Color.Orange);
                return;
            }

            await uTarget.Player.Entity.TeleportAsync(uSender.Player.Entity.Position, 0);
            await sender.SendMessageAsync(await pluginInstance.Translations.GetAsync("TP_Teleport", target.DisplayName), Color.Orange);
            await target.SendMessageAsync(await pluginInstance.Translations.GetAsync("TP_Teleported", sender.DisplayName), Color.Orange);

            pendingRequests.RemoveAll(x => x.Receiver.Id == target.Id && x.Sender.Id == sender.Id);
        }
    }
}