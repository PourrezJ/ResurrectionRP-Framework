using AlternateLife.RageMP.Net.Interfaces;
using System.Threading.Tasks;

namespace ResurrectionRP.Server
{
    public class HealItem : Item
    {
        public int Life;

        public HealItem(ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int life = 0, int itemPrice = 0, string type = "heal", string icon = "unknown-item", string classes = "health") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            Life = life;
        }

        public override async Task Use(IPlayer client, string inventoryType, int slot)
        {
            var healthActual = await client.GetHealthAsync();
            if ((healthActual += Life) > 100)
                await client.SetHealthAsync(100);
            else
                await client.SetHealthAsync(healthActual + Life);

            if (inventoryType == InventoryTypes.Pocket)
            {
                PlayerManager.GetPlayerByClient(client)?.PocketInventory?.Delete(slot, 1);
            }
            else if (inventoryType == InventoryTypes.Bag)
            {
                PlayerManager.GetPlayerByClient(client)?.BagInventory?.Delete(slot, 1);
            }
            if (Life > 0)
                await client.NotifyAsync("Vous vous êtes appliqué un bandage");
        }
    }
}
