using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Threading.Tasks;

namespace ResurrectionRP_Server
{
    public class Startup : AsyncResource
    {
        private GameMode gamemode = null;
        public async override void OnStart()
        {
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

        private Task AltAsync_OnPlayerConnect(IPlayer player, string reason)
        {
            while (gamemode == null)
                Task.Delay(50);
           
            
            return Task.CompletedTask;
        }

        public override void OnStop()
        {
            Alt.Log("GameMode Stopped");
        }
    }
}
