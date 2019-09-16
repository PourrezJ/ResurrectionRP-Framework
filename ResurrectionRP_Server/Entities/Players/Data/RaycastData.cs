using System.Numerics;

namespace ResurrectionRP_Server.Entities.Players.Data
{
    public struct RaycastData
    {
        public bool isHit;
        public Vector3 pos;
        public int hitEntity;
        public uint entityHash;
        public int entityType;
    }
}
