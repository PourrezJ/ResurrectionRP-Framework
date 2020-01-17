using ResurrectionRP_Server.Models;

namespace ResurrectionRP_Server
{
    public class ClothItem : Item
    {
        public ClothData Clothing;

        public ClothItem()
        {
        }

        public ClothItem(Models.InventoryData.ItemID id, string name, string description, ClothData clothing, double weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "cloth", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            Clothing = clothing;
        }
    }
}
