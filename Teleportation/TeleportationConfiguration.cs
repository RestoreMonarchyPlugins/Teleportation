using Rocket.API;

namespace RestoreMonarchy.Teleportation
{
    public class TeleportationConfiguration : IRocketPluginConfiguration
    {
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
        public float MoveMaxDistance { get; set; }

        public void LoadDefaults()
        {
            MessageColor = "gray";
            TPACooldown = 90;
            TPADelay = 3;
            TPADuration = 90;
            AllowCave = false;
            AllowRaid = false;
            RaidDuration = 30;
            AllowCombat = false;
            CombatDuration = 20;
            UseUnsafeTeleport = false;
            CancelOnMove = true;
        }
    }
}