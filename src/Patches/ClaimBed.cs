using Harmony;
using SDG.Unturned;
using Steamworks;
using System;
using System.Linq;
using UnityEngine;

namespace TeleportationPlugin.Patches
{
    [HarmonyPatch(typeof(BarricadeManager), "askClaimBed")]
    public static class ClaimBed
    {
        [HarmonyPrefix]
        public static bool Prefix(BarricadeManager __instance, CSteamID steamID, byte x, byte y, ushort plant, ushort index)
        {
            BarricadeManager.tryGetRegion(x, y, plant, out BarricadeRegion barricadeRegion);
            InteractableBed interactableBed = barricadeRegion.drops[(int)index].interactable as InteractableBed;
            Player player = PlayerTool.getPlayer(steamID);
            Vector3 position = interactableBed.transform.position;

            if (interactableBed != null && interactableBed.isClaimable && interactableBed.checkClaim(player.channel.owner.playerID.steamID))
            {
                if (interactableBed.isClaimed)
                {
                    TeleportationPlugin.Instance.Database.RemoveBed(position.ToString());
                    if (plant == 65535)
                    {
                        __instance.channel.send("tellClaimBed", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                        {
                                x,
                                y,
                                plant,
                                index,
                                CSteamID.Nil
                        });
                    }
                    else
                    {
                        __instance.channel.send("tellClaimBed", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                        {
                                x,
                                y,
                                plant,
                                index,
                                CSteamID.Nil
                        });
                    }

                    return false;
                }
                else
                {
                    
                    string sSteamID = steamID.m_SteamID.ToString();

                    int? bedNumber = TeleportationPlugin.Instance.Database.GetAllBeds(sSteamID).Count();
                    string bedName = "bed";

                    if (TeleportationPlugin.Instance.Database.ExistsBed(steamID.m_SteamID.ToString(), bedName + bedNumber))
                        bedNumber = bedNumber + 1;

                    
                    if (bedNumber != null)
                    {
                        bedName = bedName + bedNumber;
                    }

                    bool result = TeleportationPlugin.Instance.Database.AddBed(bedName, sSteamID, position.ToString());

                    if (!result)
                    {
                        ChatManager.say(steamID, "You can't have more beds!", Color.red, true);
                        return false;
                    }
                        


                    if (plant == 65535)
                    {
                        __instance.channel.send("tellClaimBed", ESteamCall.ALL, x, y, BarricadeManager.BARRICADE_REGIONS, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                        {
                                x,
                                y,
                                plant,
                                index,
                                player.channel.owner.playerID.steamID
                        });
                    }
                    else
                    {
                        __instance.channel.send("tellClaimBed", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                        {
                                x,
                                y,
                                plant,
                                index,
                                player.channel.owner.playerID.steamID
                        });
                    }


                }
                BitConverter.GetBytes(interactableBed.owner.m_SteamID).CopyTo(barricadeRegion.barricades[(int)index].barricade.state, 0);
            }
            return true;

        }
    }
}
