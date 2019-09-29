using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Text;

namespace ResurrectionRP_Server.Business
{
    public class StationService
    {
        public Vector3 location;
        public float Range;
        public int ID;
        public int EssencePrice = 1;
        public float Litrage = 0;
        public int LitrageMax = 1000;
        public float buyEssencePrice = 0.5f;
        [BsonIgnore]
        public ConcurrentDictionary<int, IVehicle> VehicleInStation = new ConcurrentDictionary<int,IVehicle>();
        [BsonIgnore]
        public Entities.Blips.Blips StationBlip;
        [BsonIgnore]
        public IColShape Colshape { get; set; }
        [BsonIgnore]
        public static uint[] allowedTrailers = new uint[3] { 0xB8081009, 0xD46F4737, 0x74998082 };

        public StationService(int id, float Range, Vector3 location)
        {
            this.location = location;
            this.Range = Range;
            this.ID = id;
        }
    }
}
