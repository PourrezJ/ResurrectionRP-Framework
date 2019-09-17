using AltV.Net;
using MongoDB.Driver;
using ResurrectionRP_Server.Business;
using ResurrectionRP_Server.Entities.Players;
using System;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Database
{
    public static class DataMigration
    {
        public const int DATABASE_VERSION = 2;

        public async static Task<bool> MigrateDatabase()
        {
            if (GameMode.Instance.DatabaseVersion < DATABASE_VERSION)
            {
                if (GameMode.Instance.DatabaseVersion < DATABASE_VERSION && !await MigrateToV2())
                    return false;
            }

            return true;
        }

        private async static Task<bool> MigrateToV2()
        {
            Alt.Server.LogInfo($"Migrating database to version {DATABASE_VERSION}");

            try
            {
                var collection = MongoDB.GetCollectionSafe<PlayerHandler>("players");
                var filter = Builders<PlayerHandler>.Filter.Where(p => true);
                var update = Builders<PlayerHandler>.Update.Unset("Character.Decorations");
                await collection.UpdateManyAsync(filter, update);

                filter = Builders<PlayerHandler>.Filter.Where(p => true);
                update = Builders<PlayerHandler>.Update.Rename("Character.DecorationsNew", "Character.Decorations");
                await collection.UpdateManyAsync(filter, update);
                
                var busCollection = MongoDB.GetCollectionSafe<Business.Business>("businesses");
                var busFilter = Builders<Business.Business>.Filter.Where(b => true);
                var busUpdate = Builders<Business.Business>.Update.Rename("Employees_fix", "Employees");
                await busCollection.UpdateManyAsync(busFilter, busUpdate);
            }
            catch (Exception ex)
            {
                Alt.Server.LogError($"Migration error: {ex}");
                return false;
            }

            GameMode.Instance.DatabaseVersion = DATABASE_VERSION;
            Alt.Server.LogInfo($"Migrating to version {DATABASE_VERSION} successfull");
            return true;
        }
    }
}