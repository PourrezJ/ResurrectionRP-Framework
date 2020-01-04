using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Database
{
    public class VehicleData
    {
        [BsonId]
        public string Plate;

        [BsonRepresentation(BsonType.Int32, AllowOverflow = true)]
        public uint Model;

        public bool IsParked;
        public bool IsInPound;

        public DateTime LastUse = DateTime.Now;

        public string LastDriver;

        public bool PlateHide = false;

        public string ParkingName = string.Empty;

        public float Fuel;

        public float FuelMax { get; set; } = 100;

        public float FuelConsumption { get; set; } = 5.5f;


    }
}
