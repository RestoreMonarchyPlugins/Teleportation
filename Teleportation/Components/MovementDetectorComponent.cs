using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RestoreMonarchy.Teleportation.Components
{
    public class MovementDetectorComponent : MonoBehaviour
    {
        private class MovementPlayer
        {
            public Player Player { get; set; }
            public System.Action MoveCallback { get; set; }
            public Vector3 Position { get; set; }
        }

        private TeleportationPlugin pluginInstance => TeleportationPlugin.Instance;
        private List<MovementPlayer> Players { get; set; }

        void Awake()
        {
            Players = new List<MovementPlayer>();
        }

        void Start()
        {
            InvokeRepeating("CheckMovement", 0.1f, 0.1f);
        }

        public void AddPlayer(Player player, System.Action callback)
        {
            Players.Add(new MovementPlayer()
            {
                Player = player,
                MoveCallback = callback,
                Position = player.transform.position
            });
        }

        public void RemovePlayer(Player player)
        {
            Players.RemoveAll(x => x.Player == player);
        }

        private void CheckMovement()
        {
            foreach (var player in Players.ToList())
            {
                if (player.Player == null)
                {
                    Players.Remove(player);
                }

                if (Vector3.Distance(player.Position, player.Player.transform.position) > pluginInstance.Configuration.Instance.MoveMaxDistance)
                {
                    player.MoveCallback.Invoke();
                    Players.Remove(player);
                }
            }
        }
    }
}
