using AlternateLife.RageMP.Net.Interfaces;
using System.Threading.Tasks;

namespace ResurrectionRP.Server
{
    class HandCuff : Item
    {
        public HandCuff(ItemID id, string name, string description, double weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "handcuff", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
        }

        public override Task Give(IPlayer sender, IPlayer recever, int quantite)
        {
            return base.Give(sender, recever, quantite);
        }

        public override Task OnPickup(IPlayer client, ResuPickup pickup)
        {
            return base.OnPickup(client, pickup);
        }

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
