using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Linq;
using RestoreMonarchy.Teleportation.Models;
using Steamworks;
using SDG.Unturned;
using RestoreMonarchy.Teleportation.Models.Configs;

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

            var request = new TPARequest(sender.CSteamID, target.CSteamID);
            if (!request.Validate())
                return;

            plugin.TPRequests.Add(request);
            plugin.Cooldowns[sender.CSteamID] = DateTime.Now;

            UnturnedChat.Say(sender, plugin.Translate("TPASent", target.DisplayName), plugin.MessageColor);
            UnturnedChat.Say(target, plugin.Translate("TPAReceive", sender.DisplayName), plugin.MessageColor);
        }

        public static void AcceptTPARequest(this TeleportationPlugin plugin, UnturnedPlayer caller)
        {
            // Remove all expired TP requests
            plugin.TPRequests.RemoveAll(x => x.IsExpired);

            var request = plugin.TPRequests.FirstOrDefault(x => x.Target == caller.CSteamID);
            if (request == null)
            {
                UnturnedChat.Say(caller, plugin.Translate("TPANoRequest"), plugin.MessageColor);
                return;
            }

            UnturnedChat.Say(caller, plugin.Translate("TPAAccepted", request.SenderPlayer.CharacterName), plugin.MessageColor);

            double delay = plugin.Configuration.Instance.TPADelay;
            CSteamID groupID = request.SenderPlayer.Player.quests.groupID;            
            if (groupID != CSteamID.Nil)
            {
                GroupInfo groupInfo = GroupManager.getGroupInfo(groupID);

                GroupTPADelay groupTPADelay = plugin.Configuration.Instance.GroupTPADelays
                    .OrderBy(x => x.MaxMembers)
                    .Where(x => x.MaxMembers <= groupInfo.members)
                    .FirstOrDefault();

                if (groupTPADelay != null)
                {
                    plugin.LogDebug($"Set TPA delay of player {request.SenderPlayer.CharacterName} to group delay of max {groupTPADelay.MaxMembers} members");
                    delay = groupTPADelay.TPADelay;
                }
            }

            request.Execute(delay);
            plugin.TPRequests.Remove(request);
        }

        public static void CancelTPARequest(this TeleportationPlugin plugin, UnturnedPlayer caller)
        {
            var request = plugin.TPRequests.FirstOrDefault(x => x.Sender == caller.CSteamID);
            if (request != null)
            {
                UnturnedChat.Say(caller, plugin.Translate("TPACanceled", request.TargetPlayer.DisplayName), plugin.MessageColor);
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
                UnturnedChat.Say(caller, plugin.Translate("TPADenied", request.SenderPlayer.DisplayName), plugin.MessageColor);
                plugin.TPRequests.Remove(request);
            }
            else
            {
                UnturnedChat.Say(caller, plugin.Translate("TPANoRequest"), plugin.MessageColor);
            }
        }

        public static void ClearPlayerRequests(this TeleportationPlugin plugin, CSteamID steamID)
        {
            plugin.TPRequests.RemoveAll(x => x.Sender == steamID || x.Target == steamID);
        }
    }
}
