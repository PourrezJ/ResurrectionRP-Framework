using AltV.Net.Async;
using ResurrectionRP_Server.Utils.Enums;
using System.Threading.Tasks;

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

        public void SetWalkingStyle(string walkingAnim)
        {
            PlayerSync.WalkingAnim = walkingAnim;
            Client?.SetSyncedMetaData("WalkingStyle", walkingAnim);
        }

        public void ResetWalkingStyle()
        {
            PlayerSync.WalkingAnim = null;
            Client?.SetSyncedMetaData("WalkingStyle", null);
        }

        public void SetFacialAnim(string facial)
        {
            PlayerSync.MoodAnim = facial;
            Client?.SetSyncedMetaData("FacialAnim", facial);
        }

        public void ResetFacialAnim()
        {
            PlayerSync.MoodAnim = null;
            Client?.SetSyncedMetaData("FacialAnim", null);
        }
    }
}
