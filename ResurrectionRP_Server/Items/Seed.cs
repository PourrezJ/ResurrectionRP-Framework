using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Items
{
    class SeedItem : Models.Item
    {
        public Illegal.WeedLab.SeedType SeedType;

        public SeedItem(Models.InventoryData.ItemID id, string name, string description, Illegal.WeedLab.SeedType seedType, double weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, double itemPrice = 0, string type = "item", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            SeedType = seedType;
        }
    }
}
