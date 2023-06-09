using System.Xml.Serialization;

namespace RestoreMonarchy.Teleportation.Models.Configs
{
    public class GroupTPADelay
    {
        [XmlAttribute]
        public uint MaxMembers { get; set; }
        [XmlAttribute]
        public double TPADelay { get; set; }
    }
}
