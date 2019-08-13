using System.Numerics;

namespace ResurrectionRP_Server.Models
{
    public class Attachment
    {
        public byte Type { get; set; }
        public uint RemoteID { get; set; }
        public Vector3 PositionOffset { get; set; }
        public Vector3 RotationOffset { get; set; }
        public string Bone { get; set; }
    }
}
