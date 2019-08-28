using AltV.Net;
using AltV.Net.Elements.Entities;
using System.Threading.Tasks;
using Mask = ResurrectionRP_Server.Models.Mask;

namespace ResurrectionRP_Server.Items
{
    class MaskItem : Models.Item
    {
        private bool used = false;
        public Mask Mask = new Mask("",0, 0);

        public MaskItem(Models.InventoryData.ItemID id, string name, string description, Mask mask, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "mask", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice,type, icon, classes)
        {
            Mask = mask;
        }

        public override async Task Use(IPlayer client, string inventoryType, int slot)
        {
            if (!used)
            {
                await client.SetClothAsync(Models.ClothSlot.Mask, Mask.variation, Mask.texture, 0);
                used = true;
            }
            else
            {
                await client.SetClothAsync(Models.ClothSlot.Mask, 0, 0, 0);
                used = false;
            }
                
        }

        public override Task Give(IPlayer sender, IPlayer recever, int quantite)
        {
            return base.Give(sender, recever, quantite);
        }
    }
}
