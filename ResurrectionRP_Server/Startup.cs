using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using ResurrectionRP_Server.Colshape;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Utils;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ResurrectionRP_Server
{
    public class Startup : AsyncResource
    {
        private GameMode gamemode = null;

        public static int MainThreadId { get; private set; }

        static void Main(string[] args)
        {

        }

        public override void OnStart()
        {
            MainThreadId = Thread.CurrentThread.ManagedThreadId;

            var ci = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = ci;
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            AppDomain.CurrentDomain.UnhandledException += OnException;

            Alt.OnPlayerConnect += Alt_OnPlayerConnect;
            Alt.OnPlayerDead += Alt_OnPlayerDead;

            Database.MongoDB.Init();

            var collection = Database.MongoDB.CollectionExist<GameMode>("gamemode");
            if (collection)
            {
                var database = Database.MongoDB.GetMongoDatabase();

                if (database == null)
                    return;

                Task.Run(async () =>
                {
                    var collectionData = Database.MongoDB.GetCollectionSafe<GameMode>("gamemode");
                    var data = await collectionData.FindAsync<GameMode>(new BsonDocument());
                    if (data == null)
                        return;
                    gamemode = await data.FirstOrDefaultAsync();

                    await AltAsync.Do(() => gamemode.OnStart());
                });
            }
            else
            {
                // Fresh Server
                gamemode = new GameMode();
                gamemode.OnStart();
                Task.Run(async () => await gamemode.Save());
            }
        }

        private void OnException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Alt.Server.LogError("MyHandler caught : " + e.Message);
            Alt.Server.LogError($"Runtime terminating: {args.IsTerminating}");
        }

        private void Alt_OnPlayerConnect(IPlayer player, string reason)
        {
            player.Emit("FadeOut", 0);
        }

        private void Alt_OnPlayerDead(IPlayer player, IEntity killer, uint weapon)
        {
            PlayerManager.Alt_OnPlayerDead(player, killer, weapon);
        }

        public override void OnStop()
        {
            Alt.Log("GameMode Stopped");
        }

        public override void OnTick()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) 
                FPSCounter.OnTick();
            ColshapeManager.OnTick();
            VehiclesManager.OnTick();
            base.OnTick();
        }
    }
}
