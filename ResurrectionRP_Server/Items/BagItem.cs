
using ResurrectionRP_Server.Models;

namespace ResurrectionRP_Server.Items
{
    public class BagItem : Item
    {
        public Inventory.Inventory InventoryBag { get; set; }
        public ClothData Clothing { get; set; }

        public BagItem(Models.InventoryData.ItemID id, string name, string description, ClothData bag, int weightMax, int maxSlot, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "item", string icon = "backpack", string classes = "backpack") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            Clothing = bag;
            InventoryBag = new Inventory.Inventory(weightMax, maxSlot);
        }

        public BagItem()
        {
        }

        //public override BagItem GetClone()
        //{
        //    return base.GetClone();
        //}
    }
}
