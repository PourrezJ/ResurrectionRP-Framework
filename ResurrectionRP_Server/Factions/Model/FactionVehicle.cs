using AltV.Net.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ResurrectionRP_Server.Factions.Model
{
    public class FactionVehicle
    {
        public int Rang;

        [BsonRepresentation(BsonType.Int64, AllowOverflow = true)]
        public VehicleModel Hash;
        public int Price;
        public int Weight;
        public int MaxSlot;
        public int PrimaryColor;
        public int SecondaryColor;

        public FactionVehicle(int rang, VehicleModel hash, int price = 0, int weight = 40, int maxSlot = 20, byte primaryColor = 0, byte secondaryColor = 0)
        {
            Rang = rang;
            Hash = hash;
            Price = price;
            Weight = weight;
            MaxSlot = maxSlot;
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;
        }
    }
}
