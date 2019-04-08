using Harmony;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace TeleportationPlugin.Patches
{
    [HarmonyPatch(typeof(BarricadeManager), "tellTakeBarricade")]
    public static class BarricadeDestroy
    {
        [HarmonyPrefix]
        public static void Prefix(BarricadeManager __instance, CSteamID steamID, byte x, byte y, ushort plant, ushort index)
        {
            BarricadeManager.tryGetRegion(x, y, plant, out BarricadeRegion barricadeRegion);
                        
            try
            {
                InteractableBed interactableBed = barricadeRegion.drops[(int)index].interactable as InteractableBed;
                Player player = PlayerTool.getPlayer(steamID);

                Vector3 position = interactableBed.transform.position;

                TeleportationPlugin.Instance.Database.RemoveBed(position.ToString());
                
            } catch
            {

            }
            
        } 
    }
}
