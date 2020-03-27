using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Timers;
using RestoreMonarchy.Teleportation.Models;
using RestoreMonarchy.Teleportation.Utils;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;
using Rocket.Unturned;
using Rocket.Unturned.Events;

namespace RestoreMonarchy.Teleportation
{
    public class TeleportationPlugin : RocketPlugin<TeleportationConfiguration>
    {
        public static TeleportationPlugin Instance { get; private set; }
        public List<TPARequest> TPRequests { get; set; }
        public Dictionary<ulong, Timer> CombatPlayers { get; set; }
        public Dictionary<ulong, Timer> RaidPlayers { get; set; }
        public Dictionary<CSteamID, DateTime> Cooldowns { get; set; }
        public Color MessageColor { get; set; }

        protected override void Load()
        {
            Instance = this;
            MessageColor = UnturnedChat.GetColorFromName(Configuration.Instance.MessageColor, Color.green);
            TPRequests = new List<TPARequest>();
            CombatPlayers = new Dictionary<ulong, Timer>();
            RaidPlayers = new Dictionary<ulong, Timer>();
            Cooldowns = new Dictionary<CSteamID, DateTime>();

            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            DamageTool.playerDamaged += OnPlayerDamaged;
            UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;
            BarricadeManager.onDamageBarricadeRequested += OnBuildingDamaged;
            StructureManager.onDamageStructureRequested += OnBuildingDamaged;
            Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded!", ConsoleColor.Yellow);
        }

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            this.ClearPlayerRequests(player.CSteamID);
        }

        protected override void Unload()
        {
            foreach (var combatPlayer in CombatPlayers)
                combatPlayer.Value.Dispose();
            foreach (var raidPlayer in RaidPlayers)
                raidPlayer.Value.Dispose();

            TPRequests = null;
            CombatPlayers = null;
            RaidPlayers = null;
            Cooldowns = null;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            DamageTool.playerDamaged -= OnPlayerDamaged;
            UnturnedPlayerEvents.OnPlayerDeath -= OnPlayerDeath;
            BarricadeManager.onDamageBarricadeRequested -= OnBuildingDamaged;
            StructureManager.onDamageStructureRequested -= OnBuildingDamaged;
            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
        }

        private void OnBuildingDamaged(CSteamID instigatorSteamID, Transform barricadeTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            SteamPlayer steamPlayer;
            if ((steamPlayer = PlayerTool.getSteamPlayer(instigatorSteamID)) != null)
            {
                var player = UnturnedPlayer.FromSteamPlayer(steamPlayer);

                if (player != null)
                    this.StartPlayerRaid(instigatorSteamID);
            }
        }

        private void OnPlayerDamaged(Player player, ref EDeathCause cause, ref ELimb limb, ref CSteamID killer, ref Vector3 direction, ref float damage, ref float times, ref bool canDamage)
        {
            var killerPlayer = PlayerTool.getSteamPlayer(killer);

            if (killerPlayer != null)
            {
                this.StartPlayerCombat(killer);
                this.StartPlayerCombat(player.channel.owner.playerID.steamID);
            }
        }

        private void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            if (CombatPlayers.TryGetValue(player.CSteamID.m_SteamID, out Timer timer) && timer.Enabled)
            {
                timer.Dispose();
                CombatPlayers.Remove(player.CSteamID.m_SteamID);
            }
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "TargetNotFound", "Failed to find a target" },
            { "CombatStart", "Combat mode started" },
            { "CombatExpire", "Combat mode expired" },
            { "RaidStart", "Raid mode started" },
            { "RaidExpire", "Raid mode expired" },
            { "TPAHelp", "Use: /tpa <player/accept/deny/cancel>" },
            { "TPACooldown", "You have to wait {0} before you can send request again" },
            { "TPADuplicate", "You already sent a teleportation request to that player" },
            { "TPASent", "Successfully sent TPA request to {0}" },
            { "TPAReceive", "You received TPA request from {0}" },
            { "TPANoRequest", "There is no TPA requests to you" },
            { "TPAAccepted", "Successfully accepted TPA request from {0}" },
            { "TPADelay", "You will be teleported to {0} in {1} seconds" },
            { "TPAWhileCombat", "Teleportation canceled because you or {0} is in combat mode" },
            { "TPAWhileRaid", "Teleportation canceled because you or {0} is in raid mode" },
            { "TPADead", "Teleportation canceled because you or {0} is dead" },
            { "TPACave", "Teleportation canceled because {0} is in cave" },
            { "TPANoSentRequest", "You did not send any TPA request" },
            { "TPACanceled", "Successfully canceled TPA request to {0}" },
            { "TPADenied", "Successfully denied TPA request from {0}" },
            { "TPASuccess", "You have been teleported to {0}" },
            { "TPAYourself", "You cannot send TPA request to yourself" }
        };
    }
}
