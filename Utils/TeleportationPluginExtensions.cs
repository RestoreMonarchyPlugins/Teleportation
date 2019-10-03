using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Steamworks;
using System.Timers;

namespace Teleportation.Utils
{
    public static class TeleportationPluginExtensions
    {
        public static void StartPlayerCombat(this TeleportationPlugin plugin, CSteamID steamID)
        {
            if (plugin.CombatPlayers.TryGetValue(steamID.m_SteamID, out Timer timer))
            {
                if (timer.Enabled)
                    timer.Enabled = false;
                else
                    UnturnedChat.Say(steamID, plugin.Translate("CombatStart"), plugin.MessageColor);

                timer.Start();
            }
            else
            {
                timer = new Timer(plugin.Configuration.Instance.CombatDuration * 1000);
                plugin.CombatPlayers.Add(steamID.m_SteamID, timer);
                timer.AutoReset = false;
                timer.Elapsed += (sender, e) =>
                {
                    UnturnedChat.Say(steamID, plugin.Translate("CombatExpire"), plugin.MessageColor);
                };
                timer.Start();

                UnturnedChat.Say(steamID, plugin.Translate("CombatStart"), plugin.MessageColor);                
            }
        }

        public static void StartPlayerRaid(this TeleportationPlugin plugin, CSteamID steamID)
        {
            if (plugin.RaidPlayers.TryGetValue(steamID.m_SteamID, out Timer timer))
            {
                if (timer.Enabled)
                    timer.Enabled = false;
                else
                    UnturnedChat.Say(steamID, plugin.Translate("RaidStart"), plugin.MessageColor);

                timer.Start();
            }
            else
            {
                timer = new Timer(plugin.Configuration.Instance.RaidDuration * 1000);
                plugin.RaidPlayers.Add(steamID.m_SteamID, timer);
                timer.AutoReset = false;
                timer.Elapsed += (sender, e) =>
                {
                    UnturnedChat.Say(steamID, plugin.Translate("RaidExpire"), plugin.MessageColor);
                };
                timer.Start();

                UnturnedChat.Say(steamID, plugin.Translate("RaidStart"), plugin.MessageColor);
            }
        }
    }
}
