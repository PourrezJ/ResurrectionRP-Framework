using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Items
{
    class HandCuff : Item
    {
        public HandCuff(Models.InventoryData.ItemID id, string name, string description, double weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "handcuff", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
        }

        public override void Use(IPlayer Client, string inventoryType, int slot)
        {
            if (this.id == ItemID.Serflex || this.name == "Serflex")
            {
                var ph = Client.GetPlayerHandler();

                if (ph == null)
                    return;

                if (inventoryType == Utils.Enums.InventoryTypes.Pocket)
                    ph.PocketInventory?.Delete(slot, 1);
                else if (inventoryType == Utils.Enums.InventoryTypes.Bag)
                    ph.BagInventory?.Delete(slot, 1);
            }

            base.Use(Client, inventoryType, slot);
        }
    }
}
