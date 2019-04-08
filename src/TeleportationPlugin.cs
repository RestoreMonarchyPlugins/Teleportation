using Rocket.API.DependencyInjection;
using Rocket.API.Eventing;
using Rocket.API.Logging;
using Rocket.API.Permissions;
using Rocket.API.Scheduling;
using Rocket.API.User;
using Rocket.Core.Eventing;
using Rocket.Core.Player.Events;
using Rocket.Core.Plugins;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeleportationPlugin.Commands;
using Harmony;
using System.Reflection;
using TeleportationPlugin.Models;

namespace TeleportationPlugin
{
    public class TeleportationPlugin : Plugin<TeleportationConfiguration>
    {
        private readonly IUserManager userManager;
        private readonly IPermissionProvider permissionProvider;
        private readonly ILogger logger;
        private readonly ITaskScheduler taskScheduler;
        internal static TeleportationPlugin Instance;
        public List<PlayerRequests> requests = new List<PlayerRequests>();
        public Database Database { get; set; }

        public TeleportationPlugin(IDependencyContainer container, IUserManager userManager, IPermissionProvider permissionProvider, ILogger logger, ITaskScheduler taskScheduler) : base(container)
        {
            this.userManager = userManager;
            this.permissionProvider = permissionProvider;
            this.logger = logger;
            this.taskScheduler = taskScheduler;
            Instance = this;
            this.Database = new Database(userManager, permissionProvider, Instance);

        }

        public override Dictionary<string, string> DefaultTranslations => new Dictionary<string, string>
        {
            {"Home_NotHave", "You don't have any bed"},
            {"Home_Driving", "You can't teleport whilte driving"},
            {"Home_NotFound", "{0} could not be found"},
            {"Home_Delay", "You will be teleported to your home in {0} seconds"},
            {"Home_Success", "You have been teleported to your bed"},
            {"Home_List", "Your homes:"},
            {"Home_Exist", "You already have a bed called {0}"},
            {"Home_Rename", "Successfully renamed {0} to {1}"},
            {"TP_NoRequestFrom", "There is no requests to you"},
            {"TP_NoRequestTo", "You didn't send a request to anyone"},
            {"TP_PendingFrom", "You have pending requests from:"},
            {"TP_PendingTo", "You've sent a request to:"},
            {"TP_Self", "You can't send a request to yourself"},
            {"TP_AlreadySent", "You've already sent a request to {0}"},
            {"TP_Limit", "You have sent to many requests, you may cancel them using /tpcancel"},
            {"TP_Sent", "You sent a request to {0}"},
            {"TP_Receive", "{0} sent you request. Respond with /tpadeny or /tpaccept"},
            {"TP_Accept", "You accepted {0} request"},
            {"TP_Accepted", "{0} accepted your request"},
            {"TP_Teleport", "{0} teleported to you"},
            {"TP_Teleported", "You have been teleported to {0}"},
            {"TP_Dead", "Couldn't teleport you, because {0} is dead or left."},
            {"TP_Deny", "You denied the request from {0}"},
            {"TP_Denied", "{0} denied your request"},
            {"TP_NoDeny", "There is no request to you"},
            {"TP_Cancel", "You canceled the request to {0}"}
        };

        protected override async Task OnActivate(bool isFromReload)
        {
            EventBus.AddEventListener(this, new EventListener(ConfigurationInstance, requests));

            TPCommand tpaCommand = new TPCommand(userManager, Translations, permissionProvider, logger, taskScheduler ,Instance, requests);
            HomeCommand homeCommand = new HomeCommand(userManager, Translations, permissionProvider, logger, taskScheduler, Database, Instance);

            if (Instance.ConfigurationInstance.TPEnabled)
                RegisterCommands(tpaCommand);
            if (Instance.ConfigurationInstance.HomeEnabled)
                RegisterCommands(homeCommand);

            HarmonyInstance harmony = HarmonyInstance.Create("pw.cirno.extraconcentratedjuice");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            logger.Log($"{Assembly.GetExecutingAssembly().GetName().Name} has been loaded!", LogLevel.Information);
            logger.Log($"Version: {Assembly.GetExecutingAssembly().GetName().Version}", LogLevel.Information);
            logger.Log($"Made by MCrow", LogLevel.Information);

            string isEnabled = "disabled";

            if (Instance.ConfigurationInstance.TPEnabled)
                isEnabled = "enabled";

            logger.Log($"TP: {isEnabled}", LogLevel.Information);

            if (!Instance.ConfigurationInstance.HomeEnabled)
                isEnabled = "disabled";

            logger.Log($"Home: {isEnabled}", LogLevel.Information);
        }
    }

    public class EventListener : IEventListener<PlayerDisconnectedEvent>
    {
        private readonly TeleportationConfiguration config;
        private readonly List<PlayerRequests> requests;

        public EventListener(TeleportationConfiguration config, List<PlayerRequests> requests)
        {
            this.config = config;
            this.requests = requests;
        }

        [EventHandler]
        public async Task HandleEventAsync(IEventEmitter emitter, PlayerDisconnectedEvent @event)
        {
            requests.RemoveAll(x=> x.Receiver == @event.Player.User);
            requests.RemoveAll(x => x.Sender == @event.Player.User);
        }
    }
}
