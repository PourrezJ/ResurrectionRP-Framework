using AltV.Net.Elements.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Models;

namespace ResurrectionRP_Server.Items
{
    public class HealItem : Item
    {
        [BsonRepresentation(BsonType.Int32, AllowOverflow = true)]
        public ushort Life;

        public HealItem()
        {

        }

        public HealItem(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, ushort life = 0, int itemPrice = 0, string type = "heal", string icon = "unknown-item", string classes = "health") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            Life = life;
        }

        public override void Use(IPlayer client, string inventoryType, int slot)
        {
            if (!client.Exists)
                return;

            ushort healthActual = client.Health;
            client.Health = (ushort)((healthActual + Life < 200) ? healthActual + Life : 200);
            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if (inventoryType == Utils.Enums.InventoryTypes.Pocket)
                ph.PocketInventory?.Delete(slot, 1);
            else if (inventoryType == Utils.Enums.InventoryTypes.Bag)
                ph.BagInventory?.Delete(slot, 1);
            if (Life > 100)
                client.SendNotification("Vous vous êtes appliqué un bandage");
        }
    }
}
