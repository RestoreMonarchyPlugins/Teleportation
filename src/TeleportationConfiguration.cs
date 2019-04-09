using System.Collections.Generic;

namespace RestoreMonarchy.TeleportationPlugin
{
    public class TeleportationConfiguration
    {
        public bool TPEnabled { get; set; } = true;
        public bool HomeEnabled { get; set; } = true;
        public int TeleportationDelay { get; set; } = 3;
        public int MaxRequestsDefault { get; set; } = 3;
        public int MaxHomesDefault { get; set; } = 1;
        public Dictionary<string, byte> MaxHomesGroups { get; set; } = new Dictionary<string, byte> { {"vip", 3}, {"moderator", 5} };
        public Dictionary<string, byte> MaxRequestsGroups { get; set; } = new Dictionary<string, byte> { { "vip", 5 }, { "moderator", 8 } };
    }
}
