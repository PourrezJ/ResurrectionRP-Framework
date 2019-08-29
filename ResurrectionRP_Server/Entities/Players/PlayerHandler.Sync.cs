using AltV.Net.Async;
using ResurrectionRP_Server.Utils.Enums;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities.Players
{
    public partial class PlayerHandler
    {
        public async Task SetInvincible(bool value)
        {
            PlayerSync.IsInvincible = value;
        }

        public async Task SetCuff(bool cuff)
        {
            PlayerSync.IsCuff = cuff;

            if (cuff)
            {
                await Client.SetClothAsync(Models.ClothSlot.Bags, 0, 0, 0);
                await Client.RemoveAllWeaponsAsync();
                Client.CurrentWeapon = (uint)WeaponHash.Unarmed;
            }
            else if (BagInventory != null)
                await Client.SetClothAsync(Models.ClothSlot.Bags, 1, 0, 0);
        }

        public bool IsCuff() => PlayerSync.IsCuff;

        public bool IsInvinsible() => PlayerSync.IsInvincible;

        public async Task PlayAnimation(string animDict, string animName, float blendInSpeed = 8f, float blendOutSpeed = -8f, int duration = -1, ResurrectionRP_Server.Utils.Enums.AnimationFlags flags = (ResurrectionRP_Server.Utils.Enums.AnimationFlags)0, float playbackRate = 0f)
        {
            var animsync = new AnimationsSync()
            {
                AnimName = animName,
                AnimDict = animDict,
                BlendInSpeed = blendInSpeed,
                BlendOutSpeed = blendOutSpeed,
                Duraction = duration,
                Flag = (int)flags,
                PlaybackRate = playbackRate
            };

            PlayerSync.AnimationsSync = animsync;
            // LES ANIMATIONS SONT SYNCHRO DONC OSEF
/*            foreach (IPlayer player in GameMode.Instance.PlayerList)
            {
                if (!Client.Exists)
                    continue;

                if (!player.Exists)
                    continue;

                var ClientPosition = await Client.GetPositionAsync();
                
                if ((await player.GetPositionAsync()).DistanceTo2D(ClientPosition) <= GameMode.Instance.StreamDistance)
                {
                    await player.CallAsync("PlayerSync_PlayAnimation", Client.Id, JsonConvert.SerializeObject(animsync));
                }
            }*/

        }


    }
}
