using Rocket.Core.Utils;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Steamworks;
using RestoreMonarchy.Teleportation.Utils;
using UnityEngine;

namespace RestoreMonarchy.Teleportation.Models
{
    public class TPARequest : ITeleport
    {
        public TPARequest(CSteamID sender, CSteamID target)
        {
            Sender = sender;
            Target = target;
        }

        public TPARequest() { }

        public CSteamID Sender { get; set; }
        public CSteamID Target { get; set; }

        private TeleportationPlugin pluginInstance = TeleportationPlugin.Instance;
        private bool cancel = false;

        public void Execute(double delay)
        {
            var sender = UnturnedPlayer.FromCSteamID(Sender);
            var target = UnturnedPlayer.FromCSteamID(Target);
            
            if (pluginInstance.Configuration.Instance.CancelOnMove)
            {
                var comp = pluginInstance.TryAddComponent<MovementDetector>();
                if (comp != null)
                    comp.Initialize(this, sender, target);
            }

            if (delay > 0)
            {
                UnturnedChat.Say(Sender, pluginInstance.Translate("TPADelay", target.DisplayName, delay), pluginInstance.MessageColor);
            }

            TaskDispatcher.QueueOnMainThread(() =>
            {
                if (!cancel)
                {
                    if (pluginInstance.IsPlayerInCombat(sender.CSteamID))
                    {
                        UnturnedChat.Say(Sender, pluginInstance.Translate("TPAWhileCombat", target.DisplayName), pluginInstance.MessageColor);
                        UnturnedChat.Say(Target, pluginInstance.Translate("TPAWhileCombat", sender.DisplayName), pluginInstance.MessageColor);
                        return;
                    }
                    else if (pluginInstance.IsPlayerInRaid(sender.CSteamID))
                    {
                        UnturnedChat.Say(Sender, pluginInstance.Translate("TPAWhileRaid", target.DisplayName), pluginInstance.MessageColor);
                        UnturnedChat.Say(Target, pluginInstance.Translate("TPAWhileRaid", sender.DisplayName), pluginInstance.MessageColor);
                        return;
                    }
                    else if (sender.Dead || target.Dead)
                    {
                        UnturnedChat.Say(Sender, pluginInstance.Translate("TPADead", target.DisplayName), pluginInstance.MessageColor);
                        UnturnedChat.Say(Target, pluginInstance.Translate("TPADead", sender.DisplayName), pluginInstance.MessageColor);
                    }
                    else if (pluginInstance.IsPlayerInCave(target))
                    {
                        UnturnedChat.Say(Sender, pluginInstance.Translate("TPACave", target.DisplayName), pluginInstance.MessageColor);
                        UnturnedChat.Say(Target, pluginInstance.Translate("TPACave", target.DisplayName), pluginInstance.MessageColor);
                    }
                    else
                    {
                        sender.Teleport(target);
                        UnturnedChat.Say(Sender, pluginInstance.Translate("TPASuccess", target.DisplayName), pluginInstance.MessageColor);
                    }
                }
            }, (float)delay);
        }

        public void Cancel(string message)
        {            
            cancel = true;
            UnturnedChat.Say(Sender, message, pluginInstance.MessageColor);
            UnturnedChat.Say(Target, message, pluginInstance.MessageColor);
        }
    }
}
