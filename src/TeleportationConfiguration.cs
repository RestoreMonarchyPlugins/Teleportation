using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleportationPlugin
{
    public class TeleportationConfiguration
    {
        public bool TPEnabled = true;
        public bool HomeEnabled = true;
        public int TeleportationDelay = 3;
        public int MaxRequestsDefault = 3;
        public int MaxHomesDefault = 1;
        public List<KeyValuePair<string, byte>> MaxHomesGroups = new List<KeyValuePair<string, byte>>() { new KeyValuePair<string, byte>("vip", 3), new KeyValuePair<string, byte>("moderator", 5) };
        public List<KeyValuePair<string, byte>> MaxRequestsGroups = new List<KeyValuePair<string, byte>>() { new KeyValuePair<string, byte>("vip", 5), new KeyValuePair<string, byte>("moderator", 8) };
    }
}
