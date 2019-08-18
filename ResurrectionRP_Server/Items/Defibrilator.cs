namespace ResurrectionRP_Server.Items
{
    public class Defibrilator : Models.Item
    {
        public int Usage;

        public Defibrilator(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "defibrilator", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice ,type, icon, classes)
        {
        }
    }
}
