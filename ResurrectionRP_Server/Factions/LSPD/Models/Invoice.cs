using System;
using AltV.Net.Elements.Entities;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;


namespace ResurrectionRP_Server.Factions
{
    public class Invoice
    {
        public double Amount;

        public string Desc;

        [BsonIgnore]
        public IPlayer Player;

        public String SocialClub;

        public DateTime Date = DateTime.Now.AddYears(20);

        public bool paid = false;


    }
}
