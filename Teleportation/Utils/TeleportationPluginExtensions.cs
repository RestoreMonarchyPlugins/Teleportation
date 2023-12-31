using Rocket.Core.Utils;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Timers;
using UnityEngine;

namespace RestoreMonarchy.Teleportation.Utils
{
    public static class TeleportationPluginExtensions
    {
        public static void StartPlayerCombat(this TeleportationPlugin plugin, CSteamID steamID)
        {
            if (plugin.CombatPlayers.TryGetValue(steamID, out Timer timer))
            {
                timer.Enabled = false;
                timer.Start();
            }
            else
            {
                timer = new Timer(plugin.Configuration.Instance.CombatDuration * 1000);
                plugin.CombatPlayers.Add(steamID, timer);
                timer.AutoReset = false;
                timer.Elapsed += (sender, e) =>
                {
                    TaskDispatcher.QueueOnMainThread(() => plugin.StopPlayerCombat(steamID));
                };
                timer.Start();

                UnturnedChat.Say(steamID, plugin.Translate("CombatStart"), plugin.MessageColor);                
            }
        }

        public static void StopPlayerCombat(this TeleportationPlugin plugin, CSteamID steamID)
        {
            if (plugin.CombatPlayers.TryGetValue(steamID, out Timer timer))
            {
                timer.Dispose();
                plugin.CombatPlayers.Remove(steamID);
                UnturnedChat.Say(steamID, plugin.Translate("CombatExpire"), plugin.MessageColor);                
            }
        }

        public static void StartPlayerRaid(this TeleportationPlugin plugin, CSteamID steamID)
        {
            if (plugin.RaidPlayers.TryGetValue(steamID, out Timer timer))
            {
                if (timer.Enabled)
                    timer.Enabled = false;

                timer.Start();
            }
            else
            {
                timer = new Timer(plugin.Configuration.Instance.RaidDuration * 1000);
                plugin.RaidPlayers.Add(steamID, timer);
                timer.AutoReset = false;
                timer.Elapsed += (sender, e) =>
                {
                    TaskDispatcher.QueueOnMainThread(() => plugin.StopPlayerRaid(steamID));                    
                };
                timer.Start();

                UnturnedChat.Say(steamID, plugin.Translate("RaidStart"), plugin.MessageColor);
            }
        }

        public static void StopPlayerRaid(this TeleportationPlugin plugin, CSteamID steamID)
        {
            if (plugin.RaidPlayers.TryGetValue(steamID, out Timer timer))
            {
                timer.Dispose();
                plugin.RaidPlayers.Remove(steamID);
                UnturnedChat.Say(steamID, plugin.Translate("RaidExpire"), plugin.MessageColor);
            }
        }

        public static bool IsPlayerInRaid(this TeleportationPlugin plugin, CSteamID steamID)
        {
            if (!plugin.Configuration.Instance.AllowRaid)
            {
                if (plugin.RaidPlayers.TryGetValue(steamID, out Timer timer) && timer.Enabled)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsPlayerInCombat(this TeleportationPlugin plugin, CSteamID steamID)
        {
            if (!plugin.Configuration.Instance.AllowCombat)
            {
                if (plugin.CombatPlayers.TryGetValue(steamID, out Timer timer) && timer.Enabled)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsPlayerInCave(this TeleportationPlugin plugin, UnturnedPlayer player)
        {
            if (!plugin.Configuration.Instance.AllowCave)
            {
                Vector3 point = Vector3.zero;
                UndergroundAllowlist.AdjustPosition(ref point, 0.5f, 1f);
                if (point != player.Position)
                {
                    return true;
                }
            }
            return false;
        }
    }
}