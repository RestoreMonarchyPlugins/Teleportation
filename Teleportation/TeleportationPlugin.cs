using RestoreMonarchy.Teleportation.Components;
using RestoreMonarchy.Teleportation.Models;
using RestoreMonarchy.Teleportation.Utils;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

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

        public MovementDetectorComponent MovementDetector { get; set; }

        protected override void Load()
        {
            Instance = this;
            MessageColor = UnturnedChat.GetColorFromName(Configuration.Instance.MessageColor, Color.green);
            
            TPRequests = new List<TPARequest>();
            CombatPlayers = new Dictionary<CSteamID, Timer>();
            RaidPlayers = new Dictionary<CSteamID, Timer>();
            Cooldowns = new Dictionary<CSteamID, DateTime>();

            MovementDetector = gameObject.AddComponent<MovementDetectorComponent>();

            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            DamageTool.damagePlayerRequested += DamagePlayerRequested;
            UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;
            BarricadeManager.onDamageBarricadeRequested += OnBarricadeDamaged;
            StructureManager.onDamageStructureRequested += OnStructureDamaged;

            Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded!", ConsoleColor.Yellow);
            Logger.Log("Check out more Unturned plugins at restoremonarchy.com");
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

            Destroy(MovementDetector);

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
                StructureDrop drop = StructureManager.FindStructureByRootTransform(structureTransform);
                StructureData data = drop.GetServersideData();
                // return if structure owner is instigator
                if (data.owner == instigatorSteamID.m_SteamID || data.group == steamPlayer.player.quests.groupID.m_SteamID)
                {
                    return;
                }

                // return if structure owner is offline
                if (!Provider.clients.Exists(x => x.playerID.steamID.m_SteamID == data.owner || x.player.quests.groupID.m_SteamID == data.group))
                {
                    return;
                }

                this.StartPlayerRaid(instigatorSteamID);
            }
        }

        private void OnBarricadeDamaged(CSteamID instigatorSteamID, Transform barricadeTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            SteamPlayer steamPlayer;
            if (!Configuration.Instance.AllowRaid && (steamPlayer = PlayerTool.getSteamPlayer(instigatorSteamID)) != null)
            {
                BarricadeDrop drop = BarricadeManager.FindBarricadeByRootTransform(barricadeTransform);
                BarricadeData data = drop.GetServersideData();
                // return if barricade owner is instigator
                if (data.owner == instigatorSteamID.m_SteamID || data.group == steamPlayer.player.quests.groupID.m_SteamID)
                {
                    return;
                }

                // return if barricade owner is offline
                if (!Provider.clients.Exists(x => x.playerID.steamID.m_SteamID == data.owner || x.player.quests.groupID.m_SteamID == data.group))
                {
                    return;
                }

                this.StartPlayerRaid(instigatorSteamID);
            }
        }

        private void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            this.StopPlayerCombat(player.CSteamID);
        }

        internal void SendMessageToPlayer(CSteamID steamID, string translationKey, params object[] placeholder)
        {
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(steamID);
            if (player.Player == null)
            {
                return;
            }

            SendMessageToPlayer(player, translationKey, placeholder);
        }

        internal void SendMessageToPlayer(IRocketPlayer player, string translationKey, params object[] placeholder)
        {
            if (player == null)
            {
                return;
            }

            string msg = Translate(translationKey, placeholder);
            msg = msg.Replace("[[", "<").Replace("]]", ">");
            if (player is ConsolePlayer)
            {
                Logger.Log(msg);
                return;
            }

            UnturnedPlayer unturnedPlayer = (UnturnedPlayer)player;
            if (unturnedPlayer != null)
            {
                ChatManager.serverSendMessage(msg, MessageColor, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, Configuration.Instance.MessageIconUrl, true);
            }
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            // Basic notifications
            { "TargetNotFound", "Target not found" },
            { "CombatStart", "Combat mode activated" },
            { "CombatExpire", "Combat mode ended" },
            { "RaidStart", "Raid mode activated" },
            { "RaidExpire", "Raid mode ended" },
    
            // TPA Command help
            { "TPAHelp", "[[b]]TPA Commands:[[/b]]\n/tpa [[player]] - Send request\n/tpa accept - Accept request\n/tpa deny - Deny request\n/tpa cancel - Cancel your request" },
    
            // TPA Request messages
            { "TPACooldown", "Please wait [[b]]{0}[[/b]] seconds before sending another request" },
            { "TPADuplicate", "You already have a pending request to this player" },
            { "TPASent", "TPA request sent to [[b]]{0}[[/b]]" },
            { "TPAReceive", "[[b]]{0}[[/b]] wants to teleport to you\nType [[b]]/tpa accept[[/b]] to allow" },
    
            // TPA Status messages
            { "TPANoRequest", "No pending TPA requests" },
            { "TPAAccepted", "Accepted [[b]]{0}'s[[/b]] TPA request" },
            { "TPADelay", "Teleporting to [[b]]{0}[[/b]] in [[b]]{1}[[/b]] seconds..." },
    
            // Combat/Raid related
            { "TPAWhileCombat", "Teleport failed - [[b]]{0}[[/b]] is in combat" },
            { "TPAWhileCombatYou", "Teleport failed - You are in combat" },
            { "TPAWhileRaid", "Teleport failed - [[b]]{0}[[/b]] is in raid mode" },
            { "TPAWhileRaidYou", "Teleport failed - You are in raid mode" },
    
            // Other restrictions
            { "TPADead", "Teleport failed - Player death detected" },
            { "TPACave", "Teleport failed - [[b]]{0}[[/b]] is in a cave" },
            { "TPACaveYou", "Teleport failed - You are in a cave" },
            { "TPAVehicle", "Teleport failed - [[b]]{0}[[/b]] is in a vehicle" },
            { "TPAVehicleYou", "Teleport failed - You are in a vehicle" },
    
            // Request management
            { "TPANoSentRequest", "You have no pending outgoing requests" },
            { "TPACanceled", "TPA request to [[b]]{0}[[/b]] canceled" },
            { "TPADenied", "Denied TPA request from [[b]]{0}[[/b]]" },
    
            // Movement related
            { "TPACanceledSenderMoved", "Teleport canceled - [[b]]{0}[[/b]] moved" },
            { "TPACanceledYouMoved", "Teleport canceled - You moved" },
    
            // Success message
            { "TPASuccess", "Successfully teleported to [[b]]{0}[[/b]]" },
    
            // Invalid requests
            { "TPAYourself", "You cannot send a TPA request to yourself" }
        };
    }
}
