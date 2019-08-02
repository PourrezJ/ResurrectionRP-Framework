using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using System;
using System.Threading.Tasks;

namespace ResurrectionRP_Server
{
    public class Startup : AsyncResource
    {
        public override void OnStart()
        {
            AltAsync.OnPlayerConnect += AltAsync_OnPlayerConnect;

            Alt.Log("GameMode Started");
        }

        private Task AltAsync_OnPlayerConnect(IPlayer player, string reason)
        {

            return Task.CompletedTask;
        }

        public override void OnStop()
        {
            Alt.Log("GameMode Stopped");
        }
    }
}
