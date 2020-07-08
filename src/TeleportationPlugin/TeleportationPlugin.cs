using System;
using OpenMod.Core.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

[assembly: PluginMetadata("TeleportationPlugin", Author = "MCrow", DisplayName = "Teleportation")]
namespace RestoreMonarchy.TeleportationPlugin
{
    public class TeleportationPlugin : OpenModUniversalPlugin
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<TeleportationPlugin> logger;
        private readonly IConfiguration configuration;

        public TeleportationPlugin(IServiceProvider serviceProvider, ILogger<TeleportationPlugin> logger, IConfiguration configuration) : base (serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.configuration = configuration;
        }

        public Dictionary<string, DateTime> Cooldowns { get; private set; }

        protected override async Task OnLoadAsync()
        {
            Cooldowns = new Dictionary<string, DateTime>();

        }

        protected override async Task OnUnloadAsync()
        {

        }
    }
}
