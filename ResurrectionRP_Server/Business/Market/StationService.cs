using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ResurrectionRP_Server.Business
{
    public class StationService
    {
        public Vector3 location;
        public float Range;
        public int ID;
        public List<IVehicle> VehicleInStation = new List<IVehicle>();
        [BsonIgnore]
        public Entities.Blips.Blips StationBlip;
        [BsonIgnore]
        public IColShape Colshape { get; set; }

        public StationService(int id, float Range, Vector3 location)
        {
            this.location = location;
            this.Range = Range;
            this.ID = id;
        }
    }
}
