using AltV.Net.Elements.Entities;
using System.Threading.Tasks;

namespace ResurrectionRP_Server
{
    class HandCuff : Models.Item
    {
        public HandCuff(Models.InventoryData.ItemID id, string name, string description, double weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "handcuff", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
        }

        public override Task Give(IPlayer sender, IPlayer recever, int quantite)
        {
            return base.Give(sender, recever, quantite);
        }

/*        public override Task OnPickup(IPlayer client, Models.ResuPickup pickup)
        {
            return base.OnPickup(client, pickup);
        }*/

        public override Task OnPlayerGetItem(IPlayer player)
        {
            return base.OnPlayerGetItem(player);
        }

        public override Task Use(IPlayer Client, string inventoryType, int slot)
        {
            return base.Use(Client, inventoryType, slot);
        }
    }
}
