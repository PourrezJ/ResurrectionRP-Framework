using AlternateLife.RageMP.Net.Interfaces;
using System.Threading.Tasks;

namespace ResurrectionRP.Server
{
    public class Unusable : Item
    {
        public Unusable(ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "item", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
        }

        public override async Task OnPickup(IPlayer client, ResuPickup pickup)
        {
            await base.OnPickup(client, pickup);
        }

        public override async Task Use(IPlayer client, string inventoryType, int slot)
        {
            await client.NotifyAsync("Item inutilisable");
        }
    }
}
