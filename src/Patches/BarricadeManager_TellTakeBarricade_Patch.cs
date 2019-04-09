using Harmony;
using SDG.Unturned;
using UnityEngine;

namespace RestoreMonarchy.TeleportationPlugin.Patches
{
    [HarmonyPatch(typeof(BarricadeManager), "tellTakeBarricade")]
    public static class BarricadeManager_TellTakeBarricade_Patch
    {
        [HarmonyPrefix]
        public static void TellTakeBarricade_Prefix(byte x, byte y, ushort plant, ushort index)
        {
            BarricadeManager.tryGetRegion(x, y, plant, out BarricadeRegion barricadeRegion);

            InteractableBed interactableBed = barricadeRegion.drops[index].interactable as InteractableBed;
            if (interactableBed == null)
            {
                return;
            }

            Vector3 position = interactableBed.transform.position;
            TeleportationPlugin.Instance.Database.RemoveBed(position.ToString());
        } 
    }
}
