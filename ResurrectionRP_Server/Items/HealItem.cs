using AltV.Net.Elements.Entities;
using System.Threading.Tasks;

namespace ResurrectionRP_Server
{
    public class HealItem : Models.Item
    {
        public int Life;

        public HealItem(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int life = 0, int itemPrice = 0, string type = "heal", string icon = "unknown-item", string classes = "health") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            Life = life;
        }

        public override async Task Use(IPlayer client, string inventoryType, int slot)
        {
            ushort healthActual = client.Health;
            client.Health =  (ushort)((healthActual + (ushort)Life < 100) ? healthActual + Life : 100); 

            if (inventoryType == Utils.Enums.InventoryTypes.Pocket)
                Entities.Players.PlayerManager.GetPlayerByClient(client)?.PocketInventory?.Delete(slot, 1);
            else if (inventoryType == Utils.Enums.InventoryTypes.Bag)
                Entities.Players.PlayerManager.GetPlayerByClient(client)?.BagInventory?.Delete(slot, 1);
            if (Life > 0)
                await client.NotifyAsync("Vous vous êtes appliqué un bandage");
        }
    }
}
