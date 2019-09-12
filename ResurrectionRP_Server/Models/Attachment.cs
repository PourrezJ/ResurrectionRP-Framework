using System.Numerics;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Streamer.Data;

namespace ResurrectionRP_Server.Models
{
    public class Attachment
    {
        public EntityType Type { get; set; }
        public uint RemoteID { get; set; }
        public Vector3 PositionOffset { get; set; }
        public Vector3 RotationOffset { get; set; }
        public string Bone { get; set; }
    }
}
