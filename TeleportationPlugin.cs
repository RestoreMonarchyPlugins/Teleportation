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
        public Dictionary<CSteamID, Timer> CombatPlayers { get; set; }
        public Dictionary<CSteamID, Timer> RaidPlayers { get; set; }
        public Dictionary<CSteamID, DateTime> Cooldowns { get; set; }
        public Color MessageColor { get; set; }

        protected override void Load()
        {
            Instance = this;
            MessageColor = UnturnedChat.GetColorFromName(Configuration.Instance.MessageColor, Color.green);
            TPRequests = new List<TPARequest>();
            CombatPlayers = new Dictionary<CSteamID, Timer>();
            RaidPlayers = new Dictionary<CSteamID, Timer>();
            Cooldowns = new Dictionary<CSteamID, DateTime>();

            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            DamageTool.damagePlayerRequested += DamagePlayerRequested;
            UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;
            BarricadeManager.onDamageBarricadeRequested += OnBarricadeDamaged;
            StructureManager.onDamageStructureRequested += OnStructureDamaged;

            Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded!", ConsoleColor.Yellow);
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
            DamageTool.damagePlayerRequested -= DamagePlayerRequested;
            UnturnedPlayerEvents.OnPlayerDeath -= OnPlayerDeath;
            BarricadeManager.onDamageBarricadeRequested -= OnBarricadeDamaged;
            StructureManager.onDamageStructureRequested -= OnStructureDamaged;

            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
        }

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            this.ClearPlayerRequests(player.CSteamID);
            this.StopPlayerCombat(player.CSteamID);
            this.StopPlayerRaid(player.CSteamID);
        }

        private void DamagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            var killerPlayer = PlayerTool.getPlayer(parameters.killer);
            if (!parameters.player.life.isDead && killerPlayer != null && killerPlayer != parameters.player && !Configuration.Instance.AllowCombat)
            {
                this.StartPlayerCombat(parameters.killer);
                this.StartPlayerCombat(parameters.player.channel.owner.playerID.steamID);
            }
        }

        private void OnStructureDamaged(CSteamID instigatorSteamID, Transform structureTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            SteamPlayer steamPlayer;
            if (!Configuration.Instance.AllowRaid && (steamPlayer = PlayerTool.getSteamPlayer(instigatorSteamID)) != null)
            {
                if (StructureManager.tryGetInfo(structureTransform, out _, out _, out ushort index, out StructureRegion region))
                {
                    // return if structure owner is instigator
                    if (region.structures[index].owner == instigatorSteamID.m_SteamID || region.structures[index].group == steamPlayer.player.quests.groupID.m_SteamID)
                    {
                        return;
                    }

                    // return if structure owner is offline
                    if (!Provider.clients.Exists(x => x.playerID.steamID.m_SteamID == region.structures[index].owner || x.player.quests.groupID.m_SteamID == region.structures[index].group))
                    {
                        return;
                    }
                    
                    this.StartPlayerRaid(instigatorSteamID);
                }
            }
        }

        private void OnBarricadeDamaged(CSteamID instigatorSteamID, Transform barricadeTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            SteamPlayer steamPlayer;
            if (!Configuration.Instance.AllowRaid && (steamPlayer = PlayerTool.getSteamPlayer(instigatorSteamID)) != null)
            {
                if (BarricadeManager.tryGetInfo(barricadeTransform, out _, out _, out _, out ushort index, out BarricadeRegion region))
                {
                    // return if barricade owner is instigator
                    if (region.barricades[index].owner == instigatorSteamID.m_SteamID || region.barricades[index].group == steamPlayer.player.quests.groupID.m_SteamID)
                    {
                        return;
                    }

                    // return if barricade owner is offline
                    if (!Provider.clients.Exists(x => x.playerID.steamID.m_SteamID == region.barricades[index].owner || x.player.quests.groupID.m_SteamID == region.barricades[index].group))
                    {
                        return;
                    }

                    this.StartPlayerRaid(instigatorSteamID);
                }
            }
        }

        private void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            this.StopPlayerCombat(player.CSteamID);
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
            { "TPAWhileCombat", "Teleportation canceled because {0} is in combat mode" },
            { "TPAWhileCombatYou", "Teleportation canceled because you are in combat mode" },
            { "TPAWhileRaid", "Teleportation canceled because {0} is in raid mode" },
            { "TPAWhileRaidYou", "Teleportation canceled because you are in raid mode" },
            { "TPADead", "Teleportation canceled because you or {0} is dead" },
            { "TPACave", "Teleportation canceled because {0} is in cave" },
            { "TPACaveYou", "Teleportation canceled because you are in cave" },
            { "TPANoSentRequest", "You did not send any TPA request" },
            { "TPACanceled", "Successfully canceled TPA request to {0}" },
            { "TPADenied", "Successfully denied TPA request from {0}" },
            { "TPASuccess", "You have been teleported to {0}" },
            { "TPAYourself", "You cannot send TPA request to yourself" },
            { "TPAVehicle", "Teleportation canceled because {0} is in vehicle" },
            { "TPAVehicleYou", "Teleportation canceled because you are in vehicle" }
        };
    }
}
