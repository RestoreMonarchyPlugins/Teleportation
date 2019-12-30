using Rocket.Unturned.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RestoreMonarchy.Teleportation.Models
{
    public class MovementDetector : MonoBehaviour
    {
        private TeleportationPlugin pluginInstance;
        private ITeleport teleport;
        Dictionary<UnturnedPlayer, Vector3> dict;

        public void Initialize(ITeleport teleport, params UnturnedPlayer[] players)
        {
            pluginInstance = GetComponent<TeleportationPlugin>();
            this.teleport = teleport;
            dict = players.ToDictionary(x => x, x=> x.Position);
        }

        void FixedUpdate()
        {
            foreach (var pair in dict)
            {
                if (pair.Key.Player.transform == null || pair.Key.Position == null)
                {
                    teleport.Cancel(pluginInstance.Translate("CancelDisconnect"));
                    Destroy(this);
                    return;
                }

                if (pair.Value != pair.Key.Position)
                {
                    teleport.Cancel(pluginInstance.Translate("CancelMove"));
                    Destroy(this);
                    return;
                }
            }
        }
    }
}
