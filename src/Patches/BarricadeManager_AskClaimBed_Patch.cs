using System;
using System.Linq;
using Harmony;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace RestoreMonarchy.TeleportationPlugin.Patches
{
    [HarmonyPatch(typeof(BarricadeManager), "askClaimBed")]
    public static class BarricadeManager_AskClaimBed_Patch
    {
        [HarmonyPrefix]
        public static bool AskClaimBed_Prefix(BarricadeManager __instance, CSteamID steamID, byte x, byte y, ushort plant, ushort index)
        {
            BarricadeManager.tryGetRegion(x, y, plant, out BarricadeRegion barricadeRegion);
            InteractableBed interactableBed = barricadeRegion.drops[index].interactable as InteractableBed;
            if (interactableBed == null)
            {
                return true;
            }

            Player player = PlayerTool.getPlayer(steamID);

            if (!interactableBed.isClaimable || !interactableBed.checkClaim(player.channel.owner.playerID.steamID))
            {
                return true;
            }

            Vector3 position = interactableBed.transform.position;
            if (interactableBed.isClaimed)
            {
                TeleportationPlugin.Instance.Database.RemoveBed(position.ToString());
                if (plant == 65535)
                {
                    __instance.channel.send("tellClaimBed", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, x, y, plant, index, CSteamID.Nil);
                }
                else
                {
                    __instance.channel.send("tellClaimBed", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, x, y, plant, index, CSteamID.Nil);
                }

                return false;
            }

            string sSteamID = steamID.m_SteamID.ToString();

            int? bedNumber = TeleportationPlugin.Instance.Database.GetAllBeds(sSteamID).Count();
            string bedName = "bed";

            if (TeleportationPlugin.Instance.Database.ExistsBed(steamID.m_SteamID.ToString(), bedName + bedNumber))
                bedNumber++;

            bedName += bedNumber;

            bool result = TeleportationPlugin.Instance.Database.AddBed(bedName, sSteamID, position.ToString());

            if (!result)
            {
                ChatManager.say(steamID, "You can't have more beds!", Color.red, true);
                return false;
            }

            if (plant == 65535)
            {
                __instance.channel.send("tellClaimBed", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, x, y, plant, index, player.channel.owner.playerID.steamID);
            }
            else
            {
                __instance.channel.send("tellClaimBed", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, x, y, plant, index, player.channel.owner.playerID.steamID);
            }

            BitConverter.GetBytes(interactableBed.owner.m_SteamID).CopyTo(barricadeRegion.barricades[index].barricade.state, 0);
            return true;
        }
    }
}
