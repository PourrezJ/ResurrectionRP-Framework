using AltV.Net.Elements.Entities;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Items
{
    public class HealItem : Models.Item
    {
        public int Life;

        public HealItem(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int life = 0, int itemPrice = 0, string type = "heal", string icon = "unknown-item", string classes = "health") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            Life = life;
        }

        public override Task Use(IPlayer client, string inventoryType, int slot)
        {
            ushort healthActual = client.Health;
            client.Health =  (ushort)((healthActual + (ushort)Life < 100) ? healthActual + Life : 100); 

            if (inventoryType == Utils.Enums.InventoryTypes.Pocket)
                client.GetPlayerHandler()?.PocketInventory?.Delete(slot, 1);
            else if (inventoryType == Utils.Enums.InventoryTypes.Bag)
                client.GetPlayerHandler()?.BagInventory?.Delete(slot, 1);
            if (Life > 0)
                client.SendNotification("Vous vous êtes appliqué un bandage");
            return Task.CompletedTask;
        }
    }
}
