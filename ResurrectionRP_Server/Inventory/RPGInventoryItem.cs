using Newtonsoft.Json;

namespace ResurrectionRP_Server.Inventory
{
    public class RPGInventoryItem
    {
        [JsonIgnore]
        public Models.ItemStack stack { get; private set; }

        public Models.InventoryData.ItemID id { get; set; }
        public string name { get; set; }
        public string icon { get; set; }
        [JsonProperty("class")]
        public string classes { get; set; }
        public int quantity { get; set; }
        public int inventorySlot { get; set; }
        public string inventoryType { get; set; }
        public bool usable { get; set; }
        public bool givable { get; set; }
        public bool dropable { get; set; }
        public double weight { get; set; }
        public bool stackable { get; set; }
        public double price { get; set; }
        public int outfitPosition { get; set; }
        public bool equipable { get; set; }

        public RPGInventoryItem(Models.ItemStack stack, string inventoryType, int slotIndex)
        {
            this.stack = stack;
            this.inventoryType = inventoryType;
            this.inventorySlot = slotIndex;
            this.id = stack.Item.id;
            this.name = stack.Item.name;
            this.icon = stack.Item.icon;
            this.classes = stack.Item.classes;
            this.quantity = stack.Quantity;
            this.usable = stack.Item.isUsable;
            this.givable = stack.Item.isGiven;
            this.dropable = stack.Item.isDropable;
            this.weight = stack.Item.weight;
            this.stackable = stack.Item.isStackable;
            this.price = stack.Price;
            this.equipable = true;

            this.outfitPosition = getOutfitPosition(this.classes);
        }

        private int getOutfitPosition(string classes)
        {
            for (int i = 0; i < OutfitInventory.OutfitClasses.Length; i++)
            {
                if (OutfitInventory.OutfitClasses[i] == classes)
                    return (i + 1);
            }
            return -1;
        }
    }
}
