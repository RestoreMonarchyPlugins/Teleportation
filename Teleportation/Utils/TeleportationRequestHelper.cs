using RestoreMonarchy.Teleportation.Models;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Linq;

namespace RestoreMonarchy.Teleportation.Utils
{
    public static class TeleportationRequestHelper
    {
        public static void SendTPARequest(this TeleportationPlugin pluginInstance, UnturnedPlayer sender, UnturnedPlayer target)
        {
            if (sender.Id == target.Id)
            {
                pluginInstance.SendMessageToPlayer(sender, "TPAYourself");
                return;
            }

            if (pluginInstance.Cooldowns.TryGetValue(sender.CSteamID, out DateTime lastUse))
            {
                double secondsElapsed = (DateTime.Now - lastUse).TotalSeconds;
                double timeLeft = Math.Round(pluginInstance.Configuration.Instance.TPACooldown - secondsElapsed);
                if (secondsElapsed < pluginInstance.Configuration.Instance.TPACooldown)
                {
                    pluginInstance.SendMessageToPlayer(sender, "TPACooldown", timeLeft.ToString("N0"));
                    return;
                }
            }

            if (pluginInstance.TPRequests.Exists(x => x.Sender == sender.CSteamID && x.Target == target.CSteamID))
            {
                pluginInstance.SendMessageToPlayer(sender, "TPADuplicate");
                return;
            }

            var request = new TPARequest(sender.CSteamID, target.CSteamID);
            if (!request.Validate())
            {
                return;
            }   

            pluginInstance.TPRequests.Add(request);
            pluginInstance.Cooldowns[sender.CSteamID] = DateTime.Now;

            pluginInstance.SendMessageToPlayer(sender, "TPASent", target.DisplayName);
            pluginInstance.SendMessageToPlayer(target, "TPAReceive", sender.DisplayName);
        }

        public static void AcceptTPARequest(this TeleportationPlugin pluginInstance, UnturnedPlayer caller)
        {
            // Remove all expired TP requests
            pluginInstance.TPRequests.RemoveAll(x => x.IsExpired);

            var request = pluginInstance.TPRequests.FirstOrDefault(x => x.Target == caller.CSteamID);
            if (request == null)
            {
                pluginInstance.SendMessageToPlayer(caller, "TPANoRequest");
                return;
            }

            pluginInstance.SendMessageToPlayer(caller, "TPAAccepted", request.SenderPlayer.CharacterName);
            request.Execute(pluginInstance.Configuration.Instance.TPADelay);
            pluginInstance.TPRequests.Remove(request);
        }

        public static void CancelTPARequest(this TeleportationPlugin pluginInstance, UnturnedPlayer caller)
        {
            var request = pluginInstance.TPRequests.FirstOrDefault(x => x.Sender == caller.CSteamID);
            if (request != null)
            {
                pluginInstance.SendMessageToPlayer(caller, "TPACanceled", request.TargetPlayer.DisplayName);
                pluginInstance.TPRequests.Remove(request);
            } else
            {
                pluginInstance.SendMessageToPlayer(caller, "TPANoSentRequest");
            }
        }

        public static void DenyTPARequest(this TeleportationPlugin pluginInstance, UnturnedPlayer caller)
        {
            var request = pluginInstance.TPRequests.FirstOrDefault(x => x.Target == caller.CSteamID);
            if (request != null)
            {
                pluginInstance.SendMessageToPlayer(caller, "TPADenied", request.SenderPlayer.DisplayName);
                pluginInstance.TPRequests.Remove(request);
            }
            else
            {
                pluginInstance.SendMessageToPlayer(caller, "TPANoRequest");
            }
        }

        public static void ClearPlayerRequests(this TeleportationPlugin plugin, CSteamID steamID)
        {
            plugin.TPRequests.RemoveAll(x => x.Sender == steamID || x.Target == steamID);
        }
    }
}