using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ResurrectionRP_Server.Entities.Players.Data
{
    public class Decoration
    {
        [BsonRepresentation(BsonType.Int32, AllowOverflow = true)]
        public uint Collection { get; set; }
        [BsonRepresentation(BsonType.Int32, AllowOverflow = true)]
        public uint Overlay { get; set; }

        public Decoration(uint collection, uint overlay)
        {
            Collection = collection;
            Overlay = overlay;
        }
    }
}
