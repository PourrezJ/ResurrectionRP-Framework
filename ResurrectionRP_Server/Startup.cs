using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
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

        private async Task AltAsync_OnPlayerConnect(IPlayer player, string reason)
        {
            player.Emit("FadeOut", 0);

            while (gamemode == null)
                await Task.Delay(50);

            while (gamemode.PlayerManager == null)
                await Task.Delay(50);

            player.Emit("GetSocialClub", "Events_PlayerJoin");
            //await gamemode.PlayerManager.Events_PlayerJoin(player, reason);
        }

        public override void OnStop()
        {
            Alt.Log("GameMode Stopped");
        }
    }
}
