﻿using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace ResurrectionRP_Server
{
    public class Startup : AsyncResource
    {
        private GameMode gamemode = null;

        static void Main(string[] args)
        {

        }

        public async override void OnStart()
        {
            var ci = new CultureInfo("fr-FR");
            CultureInfo.DefaultThreadCurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            AppDomain.CurrentDomain.UnhandledException += OnException;

            AltAsync.OnPlayerConnect += AltAsync_OnPlayerConnect;

            Database.MongoDB.Init();

            var collection = Database.MongoDB.CollectionExist<GameMode>("gamemode");
            if (collection)
            {
                var database =  Database.MongoDB.GetMongoDatabase();

                if (database == null)
                    return;


                var collectionData = Database.MongoDB.GetCollectionSafe<GameMode>("gamemode");
                var data = await collectionData.FindAsync<GameMode>(new BsonDocument());
                gamemode = await data.FirstOrDefaultAsync();

                if (data == null)
                    return;
                
                await gamemode.OnStartAsync();
            }
            else
            {
                // Fresh Server
                gamemode = new GameMode();
                await gamemode.OnStartAsync();
                await gamemode.Save();
            }
        }

        private void OnException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Alt.Server.LogError("MyHandler caught : " + e.Message);
            Alt.Server.LogError($"Runtime terminating: {args.IsTerminating}");
        }

        private async Task AltAsync_OnPlayerConnect(IPlayer player, string reason)
        {
            player.Emit("FadeOut", 0);

            while (gamemode == null)
                await Task.Delay(50);

            while (gamemode.PlayerManager == null)
                await Task.Delay(50);

        }

        public override void OnStop()
        {
            Alt.Log("GameMode Stopped");
        }
    }
}
