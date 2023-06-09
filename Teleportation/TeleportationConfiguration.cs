using RestoreMonarchy.Teleportation.Models.Configs;
using Rocket.API;
using System.Collections.Generic;

namespace RestoreMonarchy.Teleportation
{
    public class TeleportationConfiguration : IRocketPluginConfiguration
    {
        public bool Debug { get; set; }
        public string MessageColor { get; set; }
        public double TPACooldown { get; set; }
        public double TPADelay { get; set; }
        public double TPADuration { get; set; }
        public bool AllowCave { get; set; }
        public bool AllowRaid { get; set; }
        public double RaidDuration { get; set; }
        public bool AllowCombat { get; set; }
        public double CombatDuration { get; set; }
        public bool UseUnsafeTeleport { get; set; }
        public bool CancelOnMove { get; set; }
        public float MoveMaxDistance { get; set; } = 1;
        public GroupTPADelay[] GroupTPADelays { get; set; } = DefaultGroupTPADelays;

        public void LoadDefaults()
        {
            Debug = false;
            MessageColor = "gray";
            TPACooldown = 90;
            TPADelay = 3;
            TPADuration = 90;
            AllowCave = true;
            AllowRaid = false;
            RaidDuration = 30;
            AllowCombat = false;
            CombatDuration = 20;
            UseUnsafeTeleport = false;
            CancelOnMove = true;
            MoveMaxDistance = 1;
            GroupTPADelays = DefaultGroupTPADelays;
        }

        private static GroupTPADelay[] DefaultGroupTPADelays => new[]
        {
            new GroupTPADelay()
            {
                MaxMembers = 2,
                TPADelay = 5
            },
            new GroupTPADelay()
            {
                MaxMembers = 3,
                TPADelay = 6
            },
            new GroupTPADelay()
            {
                MaxMembers = 999,
                TPADelay = 8
            }
        };
    }
}