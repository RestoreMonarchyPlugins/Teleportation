using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly TeleportationPlugin pluginInstance;
        private readonly List<PlayerTeleportRequest> requests;

        public TeleportCommands(ITranslationCollection translations, IPermissionProvider permissionProvider, ITaskScheduler taskScheduler, TeleportationPlugin pluginInstance, List<PlayerTeleportRequest> requests)
        {
            this.translations = translations;
            this.permissionProvider = permissionProvider;
            this.taskScheduler = taskScheduler;
            this.pluginInstance = pluginInstance;
            this.requests = requests;
        }

        [Command(Summary = "Teleportation requests list", Name = "tplist")]
        [CommandAlias("tpl")]
        [CommandUser(typeof(IPlayerUser))]
        public async Task TpList(IUser sender)
        {
            if (requests.Exists(x => x.Receiver.Id == sender.Id))
            {
                StringBuilder stringList = new StringBuilder(await translations.GetAsync("TP_PendingFrom"));
                requests.FindAll(x => x.Receiver.Id == sender.Id).ForEach(x => stringList.Append($" {x.Sender.DisplayName},"));
                await sender.SendMessageAsync(stringList.ToString().TrimEnd(',', ' '), Color.Orange);
            }
            else
            {
                await sender.SendMessageAsync(await translations.GetAsync("TP_NoRequestFrom"), Color.Orange);
            }

            if (requests.Exists(x => x.Sender.Id == sender.Id))
            {
                StringBuilder stringList = new StringBuilder(await translations.GetAsync("TP_PendingTo"));
                requests.FindAll(x => x.Sender.Id == sender.Id).ForEach(x => stringList.Append($" {x.Receiver.DisplayName},"));
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
            if (requests.Exists(x => x.Sender.Id == sender.Id && x.Receiver.Id == target.User.Id))
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

            if (requests.FindAll(x => x.Sender.Id == sender.Id).Count() == maxLimit)
            {
                await sender.SendMessageAsync(await translations.GetAsync("TP_Limit"));
                return;
            }

            requests.Add(new PlayerTeleportRequest() { Sender = sender, Receiver = target.User});
            await sender.SendMessageAsync(await translations.GetAsync("TP_Sent", target.User.DisplayName), Color.Orange);
            await target.User.SendMessageAsync(await translations.GetAsync("TP_Receive", sender.DisplayName), Color.Orange);
        }

        [Command(Summary = "Accepts teleportation request", Name = "tpaccept")]
        [CommandAlias("tpa")]
        [CommandUser(typeof(IPlayerUser))]
        public async Task TpAccept(ICommandContext context, IUser sender)
        {
            if (!requests.Exists(x => x.Receiver.Id == sender.Id))
            {
                await sender.SendMessageAsync(await translations.GetAsync("TP_NoRequestFrom"), Color.Orange);
                return;
            }

            IUser target;
            if (context.Parameters.Length > 0)
            {
                target = await context.Parameters.GetAsync<IUser>(0);
            }
            else
            {
                target = requests.First(x => x.Receiver.Id == sender.Id).Sender;
                requests.RemoveAll(x => x.Receiver.Id == sender.Id && x.Sender.Id == target.Id);
            }

            await sender.SendMessageAsync(await translations.GetAsync("TP_Accept", target.DisplayName),
                Color.Orange);
            await target.SendMessageAsync(await translations.GetAsync("TP_Accepted", sender.DisplayName),
                Color.Orange);

            taskScheduler.ScheduleDelayed(pluginInstance, async () => await UTeleportation(sender, target),
                "Teleportation Task", TimeSpan.FromSeconds(pluginInstance.ConfigurationInstance.TeleportationDelay), true);
        }

        [Command(Summary = "Denies teleportation request", Name = "tpdeny")]
        [CommandAlias("tpd")]
        [CommandUser(typeof(IPlayerUser))]
        public async Task TpDeny(ICommandContext context, IUser sender)
        {
            if (!requests.Exists(x => x.Receiver.Id == sender.Id))
            {
                await sender.SendMessageAsync(await translations.GetAsync("TP_NoRequestTo"), Color.Orange);
                return;
            }

            var target = context.Parameters.Length > 0
                ? await context.Parameters.GetAsync<IUser>(0)
                : requests.First(x => x.Receiver.Id == sender.Id).Sender;

            requests.RemoveAll(x => x.Receiver.Id == sender.Id && x.Sender.Id == target.Id);
            await sender.SendMessageAsync(await translations.GetAsync("TP_Deny", target.DisplayName), Color.Orange);
            await target.SendMessageAsync(await translations.GetAsync("TP_Denied", sender.DisplayName), Color.Orange);
        }

        [Command(Summary = "Cancels teleportation request", Name = "tpcancel")]
        [CommandAlias("tpc")]
        [CommandUser(typeof(IPlayerUser))]
        public async Task TpCancel(ICommandContext context, IUser sender)
        {
            if (!requests.Exists(x => x.Sender.Id == sender.Id))
            {
                await sender.SendMessageAsync(await translations.GetAsync("TP_NoRequestFrom"), Color.Orange);
                return;
            }

            IUser target;
            if (context.Parameters.Length > 0)
            {
                target = await context.Parameters.GetAsync<IUser>(0);
                requests.RemoveAll(x => x.Sender.Id == sender.Id && x.Receiver.Id == target.Id);
            }
            else
            {
                target = requests.First(x => x.Receiver.Id == sender.Id).Sender;
                requests.RemoveAll(x => x.Sender.Id == sender.Id && x.Receiver.Id == target.Id);
            }

            await sender.SendMessageAsync(await translations.GetAsync("TP_Cancel", target.DisplayName), Color.Orange);
        }

        public async Task UTeleportation(IUser user, IUser target)
        {
            UnturnedUser uTarget = (UnturnedUser)target;
            UnturnedUser uSender = (UnturnedUser)user;

            if (!uTarget.Player.IsOnline || uTarget.Player.Entity.Dead)
            {
                await target.SendMessageAsync(await translations.GetAsync("TP_Dead", user.DisplayName), Color.Orange);
                return;
            }

            uTarget.Player.Entity.Teleport(uSender.Player);
            await user.SendMessageAsync(await translations.GetAsync("TP_Teleport", target.DisplayName),
                Color.Orange);
            await target.SendMessageAsync(await translations.GetAsync("TP_Teleported", user.DisplayName),
                Color.Orange);
            requests.RemoveAll(x => x.Receiver.Id == user.Id && x.Sender.Id == target.Id);
        }
    }
}
