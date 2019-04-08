using Rocket.API.Commands;
using Rocket.API.I18N;
using Rocket.API.Logging;
using Rocket.API.Permissions;
using Rocket.API.Player;
using Rocket.API.Scheduling;
using Rocket.API.User;
using Rocket.Core.Commands;
using Rocket.Core.Scheduling;
using Rocket.Core.User;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleportationPlugin.Models;

namespace TeleportationPlugin.Commands
{
    public class TPCommand
    {
        private readonly IUserManager userManager;
        private readonly ITranslationCollection translations;
        private readonly IPermissionProvider permissionProvider;
        private readonly ILogger logger;
        private readonly ITaskScheduler taskScheduler;
        private readonly TeleportationPlugin Instance;
        private readonly List<PlayerRequests> requests;

        public TPCommand(IUserManager userManager, ITranslationCollection translations, IPermissionProvider permissionProvider, ILogger logger, ITaskScheduler taskScheduler, TeleportationPlugin instance, List<PlayerRequests> requests)
        {
            this.userManager = userManager;
            this.translations = translations;
            this.permissionProvider = permissionProvider;
            this.logger = logger;
            this.taskScheduler = taskScheduler;
            this.Instance = instance;
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
            } else
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

            int maxLimit = Instance.ConfigurationInstance.MaxRequestsDefault;

            IEnumerable<IPermissionGroup> userGroups = await permissionProvider.GetGroupsAsync(sender);

            foreach (var item in Instance.ConfigurationInstance.MaxRequestsGroups)
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

            requests.Add(new PlayerRequests() { Sender = sender, Receiver = target.User});
            await sender.SendMessageAsync(await translations.GetAsync("TP_Sent", target.User.DisplayName), Color.Orange);
            await target.User.SendMessageAsync(await translations.GetAsync("TP_Receive", sender.DisplayName), Color.Orange);
        }

        [Command(Summary = "Accepts teleportation request", Name = "tpaccept")]
        [CommandAlias("tpa")]
        [CommandUser(typeof(IPlayerUser))]
        public async Task TpAccept(ICommandContext context, IUser sender)
        {
            IUser target;
            if (requests.Exists(x => x.Receiver.Id == sender.Id))
            {
                if (context.Parameters.Length > 0)
                {
                    target = await context.Parameters.GetAsync<IUser>(0);
                }
                else
                {
                    target = requests.FirstOrDefault(x => x.Receiver.Id == sender.Id).Sender;
                    requests.RemoveAll(x => x.Receiver.Id == sender.Id && x.Sender.Id == target.Id);
                }

                await sender.SendMessageAsync(await translations.GetAsync("TP_Accept", target.DisplayName), Color.Orange);
                await target.SendMessageAsync(await translations.GetAsync("TP_Accepted", sender.DisplayName), Color.Orange);

                taskScheduler.ScheduleDelayed(Instance, async () => await UTeleportation(sender, target), "Teleportation Task", TimeSpan.FromSeconds(Instance.ConfigurationInstance.TeleportationDelay), true);
            }
            else
            {
                await sender.SendMessageAsync(await translations.GetAsync("TP_NoRequestFrom"), Color.Orange);
                return;
            }
        }

        [Command(Summary = "Denies teleportation request", Name = "tpdeny")]
        [CommandAlias("tpd")]
        [CommandUser(typeof(IPlayerUser))]
        public async Task TpDeny(ICommandContext context, IUser sender)
        {
            IUser target;
            if (requests.Exists(x => x.Receiver.Id == sender.Id))
            {
                target = context.Parameters.Length > 0
                    ? await context.Parameters.GetAsync<IUser>(0)
                    : requests.FirstOrDefault(x => x.Receiver.Id == sender.Id).Sender;

                requests.RemoveAll(x => x.Receiver.Id == sender.Id && x.Sender.Id == target.Id);
                await sender.SendMessageAsync(await translations.GetAsync("TP_Deny", target.DisplayName), Color.Orange);
                await target.SendMessageAsync(await translations.GetAsync("TP_Denied", sender.DisplayName), Color.Orange);
            }
            else
            {
                await sender.SendMessageAsync(await translations.GetAsync("TP_NoRequestTo"), Color.Orange);
                return;
            }
        }

        [Command(Summary = "Cancels teleportation request", Name = "tpcancel")]
        [CommandAlias("tpc")]
        [CommandUser(typeof(IPlayerUser))]
        public async Task TpCancel(ICommandContext context, IUser sender)
        {
            IUser target;
            if (requests.Exists(x => x.Sender.Id == sender.Id))
            {
                if (context.Parameters.Length > 0)
                {
                    target = await context.Parameters.GetAsync<IUser>(0);
                    requests.RemoveAll(x => x.Sender == sender && x.Receiver == target);
                }
                else
                {
                    target = requests.FirstOrDefault(x => x.Receiver.Id == sender.Id).Sender;
                    requests.RemoveAll(x => x.Sender == sender && x.Receiver == target);
                }
                await sender.SendMessageAsync(await translations.GetAsync("TP_Cancel", target.DisplayName), Color.Orange);
            }
            else
            {
                await sender.SendMessageAsync(await translations.GetAsync("TP_NoRequestFrom"), Color.Orange);
                return;
            }
        }

        public async Task UTeleportation(IUser user, IUser target)
        {
            UnturnedUser uTarget = (UnturnedUser)target;
            UnturnedUser uSender = (UnturnedUser)user;

            if (uTarget.Player.IsOnline && !uTarget.Player.Entity.Dead)
            {
                uTarget.Player.Entity.Teleport(uSender.Player);
                await user.SendMessageAsync(await translations.GetAsync("TP_Teleport", target.DisplayName), Color.Orange);
                await target.SendMessageAsync(await translations.GetAsync("TP_Teleported", user.DisplayName), Color.Orange);
                requests.RemoveAll(x => x.Receiver.Id == user.Id && x.Sender.Id == target.Id);
            }
            else
            {
                await target.SendMessageAsync(await translations.GetAsync("TP_Dead", user.DisplayName), Color.Orange);
            }
        }
    }
}
