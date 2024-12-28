using RestoreMonarchy.Teleportation.Utils;
using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace RestoreMonarchy.Teleportation.Commands
{
    public class TPACommand : IRocketCommand
    {
        private TeleportationPlugin pluginInstance => TeleportationPlugin.Instance;

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 1)
            {
                pluginInstance.SendMessageToPlayer(caller, "TPAHelp");
                return;
            }

            UnturnedPlayer player = (UnturnedPlayer)caller;

            string cmd = command[0].ToLower();

            if (cmd == "accept" || cmd == "a")
            {
                pluginInstance.AcceptTPARequest(player);
            }
            else if (cmd == "cancel" || cmd == "c")
            {
                pluginInstance.CancelTPARequest(player);
            }
            else if (cmd == "deny" || cmd == "d")
            {
                pluginInstance.DenyTPARequest(player);
            }
            else
            {
                UnturnedPlayer target = UnturnedPlayer.FromName(cmd);
                if (target != null)
                {
                    pluginInstance.SendTPARequest(player, target);
                }
                else
                {
                    pluginInstance.SendMessageToPlayer(caller, "TargetNotFound");
                }
            }
        }

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "tpa";

        public string Help => "Teleportation request command";

        public string Syntax => "(player/accept/deny/cancel)";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();
    }
}
