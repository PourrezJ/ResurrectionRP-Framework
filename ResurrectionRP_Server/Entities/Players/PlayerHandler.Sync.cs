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

        public async Task SetWalkingStyleAsync(string walkingAnim)
        {
            PlayerSync.WalkingAnim = walkingAnim;
            await Client?.SetSyncedMetaDataAsync("WalkingStyle", walkingAnim);
        }

        public async Task ResetWalkingStyleAsync()
        {
            PlayerSync.WalkingAnim = null;
            await Client?.SetSyncedMetaDataAsync("WalkingStyle", null);
        }
    }
}
