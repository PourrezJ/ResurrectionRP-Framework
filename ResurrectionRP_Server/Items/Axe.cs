using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using ResurrectionRP_Server.Models;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Items
{
    class Axe : Item
    {
        public Axe(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "axe", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
        }

        public override Task Give(IPlayer sender, IPlayer recever, int quantite)
        {
            return base.Give(sender, recever, quantite);
        }

        public override void Use(IPlayer client, string inventoryType, int slot)
        {
            uint Weapon = client.Weapon;

                if (Weapon == (uint)Utils.Enums.WeaponHash.Hatchet)
                {
                    client.RemoveWeapon((uint)Utils.Enums.WeaponHash.Hatchet);
                }
                else
                {
                    client.GiveWeapon((uint)Utils.Enums.WeaponHash.Hatchet, 200, true);
                }
        }
    }
}
