using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using ResurrectionRP_Server.Models;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace ResurrectionRP_Server.Items
{
    class Pickaxe : Item
    {
        public float Health = 1000;
        public float Speed = 1.0f;
        public Pickaxe(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = false, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "pickaxe", string icon = "pickaxe", string classes = "weapon") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            Health = 1000;
            Speed = 1.0f;
        }

        public override Task Give(IPlayer sender, IPlayer recever, int quantite)
        {
            return base.Give(sender, recever, quantite);
        }
    }
}
