using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ResurrectionRP_Server.Entities.Players
{
    public partial class PlayerHandler
    {

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
