using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Timers;

namespace Teleportation.Utils
{
    public static class TeleportationPluginExtensions
    {
        public static void StartPlayerCombat(this TeleportationPlugin plugin, UnturnedPlayer player)
        {
            if (plugin.CombatPlayers.TryGetValue(player.CSteamID.m_SteamID, out Timer timer))
            {
                if (timer.Enabled)
                    timer.Enabled = false;
                else
                    UnturnedChat.Say(player.CSteamID, plugin.Translate("CombatStart"), plugin.MessageColor);

                timer.Start();
            }
            else
            {
                timer = new Timer(plugin.Configuration.Instance.CombatDuration * 1000);
                plugin.CombatPlayers.Add(player.CSteamID.m_SteamID, timer);
                timer.AutoReset = false;
                timer.Elapsed += (sender, e) =>
                {
                    UnturnedChat.Say(player.CSteamID, plugin.Translate("CombatExpire"), plugin.MessageColor);
                };
                timer.Start();

                UnturnedChat.Say(player.CSteamID, plugin.Translate("CombatStart"), plugin.MessageColor);                
            }
        }

        public static void StartPlayerRaid(this TeleportationPlugin plugin, UnturnedPlayer player)
        {
            if (plugin.RaidPlayers.TryGetValue(player.CSteamID.m_SteamID, out Timer timer))
            {
                if (timer.Enabled)
                    timer.Enabled = false;
                else
                    UnturnedChat.Say(player.CSteamID, plugin.Translate("RaidStart"), plugin.MessageColor);

                timer.Start();
            }
            else
            {
                timer = new Timer(plugin.Configuration.Instance.RaidDuration * 1000);
                plugin.RaidPlayers.Add(player.CSteamID.m_SteamID, timer);
                timer.AutoReset = false;
                timer.Elapsed += (sender, e) =>
                {
                    UnturnedChat.Say(player.CSteamID, plugin.Translate("RaidExpire"), plugin.MessageColor);
                };
                timer.Start();

                UnturnedChat.Say(player.CSteamID, plugin.Translate("RaidStart"), plugin.MessageColor);
            }
        }
    }
}
