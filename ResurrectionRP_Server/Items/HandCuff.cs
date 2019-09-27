using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Models;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Items
{
    class HandCuff : Item
    {
        public HandCuff(Models.InventoryData.ItemID id, string name, string description, double weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "handcuff", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
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

        public override void OnPlayerGetItem(IPlayer player)
        {
            base.OnPlayerGetItem(player);
        }
    }
}
