using Rocket.API.Commands;
using Rocket.API.I18N;
using Rocket.API.Permissions;
using Rocket.API.Player;
using Rocket.API.User;
using Rocket.Core.Commands;
using Rocket.Core.User;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleportationPlugin.Models;
using System.Drawing;
using System;
using Rocket.API.Scheduling;
using Rocket.Core.Scheduling;
using Rocket.UnityEngine.Extensions;

namespace TeleportationPlugin.Commands
{
    public class HomeCommand
    {
        private readonly IUserManager userManager;
        private readonly ITranslationCollection translations;
        private readonly IPermissionProvider permissionProvider;
        private readonly Rocket.API.Logging.ILogger logger;
        private readonly ITaskScheduler taskScheduler;
        private readonly TeleportationPlugin Instance;
        private readonly Database Database;

        public HomeCommand(IUserManager userManager, ITranslationCollection translations, IPermissionProvider permissionProvider, Rocket.API.Logging.ILogger logger, ITaskScheduler taskScheduler, Database Database, TeleportationPlugin instance)
        {
            this.userManager = userManager;
            this.translations = translations;
            this.permissionProvider = permissionProvider;
            this.logger = logger;
            this.taskScheduler = taskScheduler;
            this.Database = Database;
            this.Instance = instance;
        }

        [Command(Summary = "Teleports to your bed", Name = "home")]
        [CommandAlias("bed")]
        [CommandUser(typeof(IPlayerUser))]
        public async Task Home(ICommandContext context, IUser sender)
        {
            UnturnedUser uSender = (UnturnedUser)sender;
            UnityEngine.Vector3 position;
            string bedName;

            if (context.Parameters.Length > 0)
            {
                bedName = await context.Parameters.GetAsync<string>(0);
                string uBed = Database.GetBed(sender.Id, bedName);
                if (uBed == null)
                {
                    await sender.SendMessageAsync(await translations.GetAsync("Home_NotFound", bedName), Color.Orange);
                    return;
                }
                else
                {
                    position = UTools.StringToVector3(uBed);
                }
            }
            else
            {
                if (!BarricadeManager.tryGetBed(uSender.CSteamID, out position, out byte angle))
                {
                    await sender.SendMessageAsync(await translations.GetAsync("Home_NotHave"), Color.Orange);
                    return;
                }
            }

            await sender.SendMessageAsync(await translations.GetAsync("Home_Delay", Instance.ConfigurationInstance.TeleportationDelay), Color.Orange);

            taskScheduler.ScheduleDelayed(Instance, async () => await UTeleportation(uSender, position), "Home Teleportation Task", 
                TimeSpan.FromSeconds(Instance.ConfigurationInstance.TeleportationDelay), true);
        }

        public async Task UTeleportation(UnturnedUser uSender, UnityEngine.Vector3 position)
        {
            await uSender.Player.Entity.TeleportAsync(position.ToSystemVector(), 0);
            await uSender.Player.User.SendMessageAsync(await translations.GetAsync("Home_Success"), Color.Orange);
        }

        [Command(Summary = "List of your beds", Name = "homes")]
        [CommandAlias("beds")]
        [CommandUser(typeof(IPlayerUser))]
        public async Task Homes(IUser sender)
        {
            var beds = Database.GetAllBeds(sender.Id);
            if (beds.Count() == 0)
            {
                await sender.SendMessageAsync(await translations.GetAsync("Home_NotHave"), Color.Orange);
                return;
            }

            StringBuilder result = new StringBuilder(await translations.GetAsync("Home_List"));

            foreach (var bed in beds)
            {
                result.Append($" {bed.BedName},");
            }
            await sender.SendMessageAsync(result.ToString().TrimEnd(',', ' '), Color.Orange);
        }

        [Command(Summary = "Renames your bed", Name = "renamehome")]
        [CommandAlias("renamebed")]
        [CommandUser(typeof(IPlayerUser))]
        public async Task RenameHome(IUser sender, string oldName, string newName)
        {
            UnturnedUser uSender = (UnturnedUser)sender;
            if (Database.RenameBed(uSender, oldName, newName, await translations.GetAsync("Home_NotFound", oldName), await translations.GetAsync("Home_Exist", newName)))
            {
                await sender.SendMessageAsync(await translations.GetAsync("Home_Rename", oldName, newName), Color.Orange);
            }
            
        }

    }
}
