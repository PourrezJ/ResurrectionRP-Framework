using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Models.InventoryData.Items
{
    class Axe : Item
    {
        public Axe(ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "axe", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
        }

        public override Task Give(IPlayer sender, IPlayer recever, int quantite)
        {
            return base.Give(sender, recever, quantite);
        }

        public override async Task Use(IPlayer client, string inventoryType, int slot)
        {
            var weapons = await client.GetWeaponAsync();
            /*
            if (weapons.Count > 0)
            {
                if (weapons.ContainsKey(WeaponModel.Hatchet))
                {
                    await client.RemoveWeaponAsync((uint)WeaponModel.Hatchet);
                }
                else
                {
                    await client.GiveWeaponAsync((uint)WeaponModel.Hatchet, 200, true);
                }
            }
            else
            {
                await client.GiveWeaponAsync((uint)WeaponModel.Hatchet, 200, true);
            }*/
        }
    }
}
