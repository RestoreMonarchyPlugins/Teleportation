using RestoreMonarchy.Teleportation.Utils;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace RestoreMonarchy.Teleportation.Commands
{
    public class TPACommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "tpa";

        public string Help => "Teleportation request command";

        public string Syntax => "(player/accept/deny/cancel)";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 1)
            {
                UnturnedChat.Say(caller, TeleportationPlugin.Instance.Translate("TPAHelp"), TeleportationPlugin.Instance.MessageColor);
                return;
            }

            string cmd = command[0].ToLower();

            if (cmd == "accept" || cmd == "a")
            {
                TeleportationPlugin.Instance.AcceptTPARequest((UnturnedPlayer)caller);
            } else if (cmd == "cancel" || cmd == "c")
            {
                TeleportationPlugin.Instance.CancelTPARequest((UnturnedPlayer)caller);
            } else if (cmd == "deny" || cmd == "d")
            {
                TeleportationPlugin.Instance.DenyTPARequest((UnturnedPlayer)caller);
            } else
            {
                var target = UnturnedPlayer.FromName(cmd);
                if (target != null)
                {
                    TeleportationPlugin.Instance.SendTPARequest((UnturnedPlayer)caller, target);
                } else
                {
                    UnturnedChat.Say(caller, TeleportationPlugin.Instance.Translate("TargetNotFound"), TeleportationPlugin.Instance.MessageColor);
                }
            }
        }
    }
}
