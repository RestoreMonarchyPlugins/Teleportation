using Rocket.API;

namespace RestoreMonarchy.Teleportation
{
    public class TeleportationConfiguration : IRocketPluginConfiguration
    {
        public string MessageColor { get; set; }
        public string MessageIconUrl { get; set; } = "https://i.imgur.com/wr879ca.png";
        public double TPACooldown { get; set; }
        public double TPADelay { get; set; }
        public double TPADuration { get; set; }
        public bool AllowCave { get; set; } = true;
        public bool ShouldSerializeAllowCave() => !AllowCave;
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
            MessageIconUrl = "https://i.imgur.com/wr879ca.png";
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
            MoveMaxDistance = 0.5f;
        }
    }
}