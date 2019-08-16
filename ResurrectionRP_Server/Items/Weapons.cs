using AlternateLife.RageMP.Net.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ResurrectionRP.Server
{
    class Weapons : Item
    {
        [BsonRepresentation(BsonType.Int64, AllowOverflow = true)]
        public WeaponHash Hash;
        public int NSerie { get; private set; }

        public Weapons(ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = false, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "weapons", WeaponHash hash = WeaponHash.Unarmed, string icon = "weapon", string classes = "weapon") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            Hash = hash;
            NSerie = Utils.RandomNumber(11111111, 99999999);
            isStackable = false;
        }
    }
}
