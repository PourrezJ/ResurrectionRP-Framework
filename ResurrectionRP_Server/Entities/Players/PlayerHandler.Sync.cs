using ResurrectionRP_Server.Utils.Enums;

namespace ResurrectionRP_Server.Entities.Players
{
    public partial class PlayerHandler
    {
        public void SetCuff(bool cuff)
        {
            PlayerSync.IsCuff = cuff;

            if (cuff)
            {
                Client.SetCloth(Models.ClothSlot.Bags, 0, 0, 0);
                Client.RemoveAllWeapons();
                Client.CurrentWeapon = (uint)WeaponHash.Unarmed;
            }
            else if (BagInventory != null)
                Client.SetCloth(Models.ClothSlot.Bags, 1, 0, 0);
        }

        public bool IsCuff() => PlayerSync.IsCuff;
    }
}
