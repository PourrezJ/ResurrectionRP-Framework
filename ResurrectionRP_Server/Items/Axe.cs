using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using System.Numerics;
using System.Threading.Tasks;
using Flags = ResurrectionRP_Server.Utils.Enums.AnimationFlags;

namespace ResurrectionRP_Server.Items
{
    class Axe : Models.Item
    {
        public Axe(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "axe", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
        }

        public override Task Give(IPlayer sender, IPlayer recever, int quantite)
        {
            return base.Give(sender, recever, quantite);
        }

        public override async Task Use(IPlayer client, string inventoryType, int slot)
        {
            uint Weapon = client.Weapon;

                if (Weapon == (uint)Utils.Enums.WeaponHash.Hatchet)
                {
                    await client.RemoveWeaponAsync((uint)Utils.Enums.WeaponHash.Hatchet);
                }
                else
                {
                    await client.GiveWeaponAsync((uint)Utils.Enums.WeaponHash.Hatchet, 200, true);
                }
        }
    }
}
