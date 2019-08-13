using AltV.Net.Elements.Entities;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Models.InventoryData.Items
{
    class MaskItem : Item
    {
        private bool used = false;
        public Mask Mask = new Mask("",0, 0);

        public MaskItem(ItemID id, string name, string description, Mask mask, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "mask", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice,type, icon, classes)
        {
            Mask = mask;
        }

        public override async Task Use(IPlayer client, string inventoryType, int slot)
        {
            if (!used)
            {
                await client.SetClothAsync(AlternateLife.RageMP.Net.Enums.ClothSlot.Mask, new AlternateLife.RageMP.Net.Data.ClothData(Mask.variation, Mask.texture, 0));
                used = true;
            }
            else
            {
                await client.SetClothAsync(AlternateLife.RageMP.Net.Enums.ClothSlot.Mask, new AlternateLife.RageMP.Net.Data.ClothData(0,0,0));
                used = false;
            }
                
        }

        public override Task Give(IPlayer sender, IPlayer recever, int quantite)
        {
            return base.Give(sender, recever, quantite);
        }
    }
}
