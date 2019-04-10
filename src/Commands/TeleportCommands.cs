using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using RestoreMonarchy.TeleportationPlugin.Requests;
using Rocket.API.Commands;
using Rocket.API.I18N;
using Rocket.API.Permissions;
using Rocket.API.Player;
using Rocket.API.Scheduling;
using Rocket.API.User;
using Rocket.Core.Commands;
using Rocket.Core.Scheduling;
using Rocket.Core.User;
using Rocket.Unturned.Player;

namespace RestoreMonarchy.TeleportationPlugin.Commands
{
    public class TeleportCommands
    {
        private readonly ITranslationCollection translations;
        private readonly IPermissionProvider permissionProvider;
        private readonly ITaskScheduler taskScheduler;
        private readonly ITeleportationRequestManager requestsManager;
        private readonly TeleportationPlugin pluginInstance;

        public TeleportCommands(ITranslationCollection translations, IPermissionProvider permissionProvider, ITaskScheduler taskScheduler, ITeleportationRequestManager requestsManager, TeleportationPlugin pluginInstance)
        {
            this.translations = translations;
            this.permissionProvider = permissionProvider;
            this.taskScheduler = taskScheduler;
            this.requestsManager = requestsManager;
            this.pluginInstance = pluginInstance;
        }

        [Command(Summary = "Teleportation requests list", Name = "tplist")]
        [CommandAlias("tpl")]
        [CommandUser(typeof(IPlayerUser))]
        public async Task TpList(IUser sender)
        {
            var receiverRequests = requestsManager.GetRequestsForSender(sender).ToList();

            if (receiverRequests.Any())
            {
                StringBuilder stringList = new StringBuilder(await translations.GetAsync("TP_PendingFrom"));
                receiverRequests.ForEach(x => stringList.Append($" {x.Sender.DisplayName},"));
                await sender.SendMessageAsync(stringList.ToString().TrimEnd(',', ' '), Color.Orange);
            }
            else
            {
                await sender.SendMessageAsync(await translations.GetAsync("TP_NoRequestFrom"), Color.Orange);
            }

            var senderRequests = requestsManager.GetRequestsForSender(sender).ToList();
            if (senderRequests.Any())
            {
                StringBuilder stringList = new StringBuilder(await translations.GetAsync("TP_PendingTo"));
                senderRequests.ForEach(x => stringList.Append($" {x.Receiver.DisplayName},"));
                await sender.SendMessageAsync(stringList.ToString().TrimEnd(',', ' '), Color.Orange);
            }
            else
            {
                await sender.SendMessageAsync(await translations.GetAsync("TP_NoRequestTo"), Color.Orange);
            }
        }

        [Command(Summary = "Sends request to other player", Name = "tprequest")]
        [CommandAlias("tpr")]
        [CommandUser(typeof(IPlayerUser))]
        public async Task TpRequest(IUser sender, IPlayer target)
        {
            if (target.User.Id == sender.Id)
            {
                await sender.SendMessageAsync(await translations.GetAsync("TP_Self"), Color.Orange);
                return;
            }

            var senderRequests = requestsManager.GetRequestsForSender(sender).ToList();
            if (senderRequests.Any(x => x.Receiver.Id == target.User.Id))
            {
                await sender.SendMessageAsync(await translations.GetAsync("TP_AlreadySent", target.User.DisplayName), Color.Orange);
                return;
            }

            int maxLimit = pluginInstance.ConfigurationInstance.MaxRequestsDefault;

            IEnumerable<IPermissionGroup> userGroups = (await permissionProvider.GetGroupsAsync(sender)).ToList();

            foreach (var item in pluginInstance.ConfigurationInstance.MaxRequestsGroups)
            {
                foreach (var rank in userGroups)
                {
                    if (rank.Id == item.Key && maxLimit < item.Value)
                    {
                        maxLimit = Convert.ToInt32(item.Value);
                    }
                }
            }

            if (senderRequests.Count == maxLimit)
            {
                await sender.SendMessageAsync(await translations.GetAsync("TP_Limit"));
                return;
            }

            requestsManager.AddRequest(sender, target.User);
            await sender.SendMessageAsync(await translations.GetAsync("TP_Sent", target.User.DisplayName), Color.Orange);
            await target.User.SendMessageAsync(await translations.GetAsync("TP_Receive", sender.DisplayName), Color.Orange);
        }

        [Command(Summary = "Accepts teleportation request", Name = "tpaccept")]
        [CommandAlias("tpa")]
        [CommandUser(typeof(IPlayerUser))]
        public async Task TpAccept(ICommandContext context)
        {
            var sender = context.User;

            var receiverRequests = requestsManager.GetRequestsForReceiver(sender).ToList();
            if (!receiverRequests.Any())
            {
                await sender.SendMessageAsync(await translations.GetAsync("TP_NoRequestFrom"), Color.Orange);
                return;
            }

            PlayerTeleportRequest request = await GetRequestFromContextAsync(context, true);
            requestsManager.Process(request);

            var target = request.Receiver;
            await sender.SendMessageAsync(await translations.GetAsync("TP_Accept", target.DisplayName), Color.Orange);
            await target.SendMessageAsync(await translations.GetAsync("TP_Accepted", sender.DisplayName), Color.Orange);
        }

        [Command(Summary = "Denies teleportation request", Name = "tpdeny")]
        [CommandAlias("tpd")]
        [CommandUser(typeof(IPlayerUser))]
        public async Task TpDeny(ICommandContext context)
        {
            var sender = context.User;

            var receiverRequests = requestsManager.GetRequestsForReceiver(sender).ToList();
            if (!receiverRequests.Any())
            {
                await sender.SendMessageAsync(await translations.GetAsync("TP_NoRequestTo"), Color.Orange);
                return;
            }

            PlayerTeleportRequest request = await GetRequestFromContextAsync(context, true);
            requestsManager.Remove(request);

            var target = request.Receiver;
            await sender.SendMessageAsync(await translations.GetAsync("TP_Deny", target.DisplayName), Color.Orange);
            await target.SendMessageAsync(await translations.GetAsync("TP_Denied", sender.DisplayName), Color.Orange);
        }

        [Command(Summary = "Cancels teleportation request", Name = "tpcancel")]
        [CommandAlias("tpc")]
        [CommandUser(typeof(IPlayerUser))]
        public async Task TpCancel(ICommandContext context)
        {
            var sender = context.User;

            var senderRequests = requestsManager.GetRequestsForSender(sender).ToList();
            if (!senderRequests.Any())
            {
                await sender.SendMessageAsync(await translations.GetAsync("TP_NoRequestFrom"), Color.Orange);
                return;
            }

            PlayerTeleportRequest request = await GetRequestFromContextAsync(context, true);
            requestsManager.Remove(request);

            await sender.SendMessageAsync(await translations.GetAsync("TP_Cancel", request.Receiver.DisplayName), Color.Orange);
        }

        private async Task<PlayerTeleportRequest> GetRequestFromContextAsync(ICommandContext context, bool isReceiver)
        {
            var sender = context.User;

            var requests = isReceiver ? requestsManager.GetRequestsForReceiver(sender) : requestsManager.GetRequestsForSender(sender);

            if (context.Parameters.Length > 0)
            {
                var player = await context.Parameters.GetAsync<IUser>(0);
                return requests.First(x => x.Receiver.Id == player.Id);
            }
            else
            {
                return requests.First(x => x.Receiver.Id == sender.Id);
            }
        }
    }
}
