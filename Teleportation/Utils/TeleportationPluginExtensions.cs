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
        public static void StartPlayerCombat(this TeleportationPlugin pluginInstance, CSteamID steamID)
        {
            if (pluginInstance.CombatPlayers.TryGetValue(steamID, out Timer timer))
            {
                timer.Enabled = false;
                timer.Start();
            }
            else
            {
                timer = new Timer(pluginInstance.Configuration.Instance.CombatDuration * 1000);
                pluginInstance.CombatPlayers.Add(steamID, timer);
                timer.AutoReset = false;
                timer.Elapsed += (sender, e) =>
                {
                    TaskDispatcher.QueueOnMainThread(() => pluginInstance.StopPlayerCombat(steamID));
                };
                timer.Start();

                pluginInstance.SendMessageToPlayer(steamID, "CombatStart");
                UnturnedChat.Say(steamID, pluginInstance.Translate("CombatStart"), pluginInstance.MessageColor);                
            }
        }

        public static void StopPlayerCombat(this TeleportationPlugin plugin, CSteamID steamID)
        {
            if (plugin.CombatPlayers.TryGetValue(steamID, out Timer timer))
            {
                timer.Dispose();
                plugin.CombatPlayers.Remove(steamID);
                plugin.SendMessageToPlayer(steamID, "CombatExpire");
            }
        }

        public static void StartPlayerRaid(this TeleportationPlugin pluginInstance, CSteamID steamID)
        {
            if (pluginInstance.RaidPlayers.TryGetValue(steamID, out Timer timer))
            {
                if (timer.Enabled)
                {
                    timer.Enabled = false;
                }   

                timer.Start();
            }
            else
            {
                timer = new Timer(pluginInstance.Configuration.Instance.RaidDuration * 1000);
                pluginInstance.RaidPlayers.Add(steamID, timer);
                timer.AutoReset = false;
                timer.Elapsed += (sender, e) =>
                {
                    TaskDispatcher.QueueOnMainThread(() => pluginInstance.StopPlayerRaid(steamID));                    
                };
                timer.Start();

                pluginInstance.SendMessageToPlayer(steamID, "RaidStart");
            }
        }

        public static void StopPlayerRaid(this TeleportationPlugin pluginInstance, CSteamID steamID)
        {
            if (pluginInstance.RaidPlayers.TryGetValue(steamID, out Timer timer))
            {
                timer.Dispose();
                pluginInstance.RaidPlayers.Remove(steamID);
                pluginInstance.SendMessageToPlayer(steamID, "RaidExpire");
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

                float height = LevelGround.getHeight(point);
                if (height > point.y)
                {
                    return true;
                }
            }

            return false;
        }
    }
}