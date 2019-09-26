using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Models;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Items
{
    public class Unusable : Item
    {
        public Unusable(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "item", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
        }

        public override async Task OnPickup(IPlayer client, Models.ResuPickup pickup)
        {
            await base.OnPickup(client, pickup);
        }

        public override void Use(IPlayer client, string inventoryType, int slot)
        {
            client.SendNotification("Item inutilisable");
        }
    }
}
