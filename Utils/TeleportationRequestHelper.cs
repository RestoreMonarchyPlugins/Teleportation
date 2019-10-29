using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Linq;
using RestoreMonarchy.Teleportation.Models;

namespace RestoreMonarchy.Teleportation.Utils
{
    public static class TeleportationRequestHelper
    {
        public static void SendTPARequest(this TeleportationPlugin plugin, UnturnedPlayer sender, UnturnedPlayer target)
        {
            if (sender.Id == target.Id)
            {
                UnturnedChat.Say(sender, plugin.Translate("TPAYourself"), plugin.MessageColor);
                return;
            }

            if (plugin.Cooldowns.TryGetValue(sender.CSteamID, out DateTime lastUse))
            {
                double secondsElapsed = (DateTime.Now - lastUse).TotalSeconds;
                double timeLeft = Math.Round(plugin.Configuration.Instance.TPACooldown - secondsElapsed);
                if (secondsElapsed < plugin.Configuration.Instance.TPACooldown)
                {
                    UnturnedChat.Say(sender, plugin.Translate("TPACooldown", timeLeft), plugin.MessageColor);
                    return;
                }
            }

            if (plugin.TPRequests.Exists(x => x.Sender == sender.CSteamID && x.Target == target.CSteamID))
            {
                UnturnedChat.Say(sender, plugin.Translate("TPADuplicate"), plugin.MessageColor);
                return;
            }

            plugin.TPRequests.Add(new TPARequest(sender.CSteamID, target.CSteamID));
            plugin.Cooldowns[sender.CSteamID] = DateTime.Now;
            UnturnedChat.Say(sender, plugin.Translate("TPASent", target.DisplayName), plugin.MessageColor);
            UnturnedChat.Say(target, plugin.Translate("TPAReceive", sender.DisplayName), plugin.MessageColor);
        }

        public static void AcceptTPARequest(this TeleportationPlugin plugin, UnturnedPlayer caller)
        {
            var request = plugin.TPRequests.FirstOrDefault(x => x.Target == caller.CSteamID);
            if (request == null)
            {
                UnturnedChat.Say(caller, plugin.Translate("TPANoRequest"), plugin.MessageColor);
                return;
            }

            UnturnedChat.Say(caller, plugin.Translate("TPAAccepted", UnturnedPlayer.FromCSteamID(request.Target).DisplayName), plugin.MessageColor);
            request.Execute(plugin.Configuration.Instance.TPADelay);
            plugin.TPRequests.Remove(request);
        }

        public static void CancelTPARequest(this TeleportationPlugin plugin, UnturnedPlayer caller)
        {
            var request = plugin.TPRequests.FirstOrDefault(x => x.Sender == caller.CSteamID);
            if (request != null)
            {
                UnturnedChat.Say(caller, plugin.Translate("TPACanceled", UnturnedPlayer.FromCSteamID(request.Target).DisplayName), plugin.MessageColor);
                plugin.TPRequests.Remove(request);
            } else
            {
                UnturnedChat.Say(caller, plugin.Translate("TPANoSentRequest"), plugin.MessageColor);
            }
        }

        public static void DenyTPARequest(this TeleportationPlugin plugin, UnturnedPlayer caller)
        {
            var request = plugin.TPRequests.FirstOrDefault(x => x.Target == caller.CSteamID);
            if (request != null)
            {
                UnturnedChat.Say(caller, plugin.Translate("TPADenied", UnturnedPlayer.FromCSteamID(request.Sender).DisplayName), plugin.MessageColor);
                plugin.TPRequests.Remove(request);
            }
            else
            {
                UnturnedChat.Say(caller, plugin.Translate("TPANoRequest"), plugin.MessageColor);
            }
        }
    }
}
