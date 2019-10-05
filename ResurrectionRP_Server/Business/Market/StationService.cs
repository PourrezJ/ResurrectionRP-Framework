using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Colshape;
using System.Collections.Concurrent;
using System.Numerics;

namespace ResurrectionRP_Server.Business
{
    public class StationService
    {
        public Vector3 Location;
        public float Range;
        public int ID;
        public int EssencePrice = 1;
        public float Litrage;
        public int LitrageMax = 3000;
        [BsonIgnore]
        public ConcurrentDictionary<int, IVehicle> VehicleInStation;
        [BsonIgnore]
        public Entities.Blips.Blips StationBlip;
        [BsonIgnore]
        public IColshape Colshape { get; set; }
        [BsonIgnore]
        public static uint[] allowedTrailers = new uint[3] { 0xB8081009, 0xD46F4737, 0x74998082 };

        public StationService(int id, float range, Vector3 location)
        {
            Location = location;
            Range = range;
            ID = id;
        }
    }
}
