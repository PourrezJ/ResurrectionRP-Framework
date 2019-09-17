using AltV.Net;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System.Drawing;
using ResurrectionRP_Server.Bank;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Factions;
using ResurrectionRP_Server.Models;
using System;
using System.Numerics;
using System.Threading.Tasks;
using ResurrectionRP_Server.Business;

namespace ResurrectionRP_Server.Database
{
    public class MongoDB
    {
        #region Public classes
        public class DbStats
        {
            #region Properties
            public int Calls { get; set; }
            public int Errors { get; set; }
            #endregion

            #region Constructors
            public DbStats()
            { }

            public DbStats(DbStats dbStats)
            {
                Calls = dbStats.Calls;
                Errors = dbStats.Errors;
            }
            #endregion

            #region Methods
            public void Reset()
            {
                Calls = 0;
                Errors = 0;
            }
            #endregion
        }
        #endregion

        #region Private static variables
        private static IMongoClient _client;
        private static IMongoDatabase _database;
        private static DbStats _dbStats = new DbStats();
        #endregion

        #region Private methods
        private static string GetObjectDetails(object objet)
        {
            if (objet.GetType() == typeof(GameMode))
            {
                GameMode gameMode = (GameMode)objet;
                return $"Players: ??? - Vehicles: {VehiclesManager.VehicleHandlerList.Count}";
            }
            else if (objet.GetType() == typeof(PlayerHandler))
            {
                PlayerHandler player = (PlayerHandler)objet;
                return $"Id: {player.PID} - Name: {player.Identite.Name}";
            }
            else if (objet.GetType() == typeof(VehicleHandler))
            {
                VehicleHandler vehicle = (VehicleHandler)objet;
                return $"Vehicle: {vehicle.Plate} - Owner: {vehicle.OwnerID} - Last driver: {vehicle.LastDriver}";
            }
            /*
            else if (objet.GetType() == typeof(CarPark))
            {
                CarPark park = (CarPark)objet;
                return $"CarPark: {park.Parking.Name} - Vehicles: {park.Parking.ListVehicleStored.Count}";
            }
            else if (objet.GetType().IsSubclassOf(typeof(Faction)))
            {
                Faction faction = (Faction)objet;

                if (faction.Parking != null)
                    return $"Faction: {faction.FactionName} - Vehicles: {faction.Parking.ListVehicleStored.Count}";
                else
                    return $"Faction: {faction.FactionName}";
            }
            else if (objet.GetType() == typeof(House))
            {
                House house = (House)objet;

                if (house.Parking != null)
                    return $"House: {house.ID} - Vehicles: {house.Parking.ListVehicleStored.Count}";
                else
                    return $"House: {house.ID}";
            }
            else if (objet.GetType().IsSubclassOf(typeof(Society)))
            {
                Society society = (Society)objet;

                if (society.Parking != null)
                    return $"Society: {society.SocietyName} - Vehicles: {society.Parking.ListVehicleStored.Count}";
                else
                    return $"Society: {society.SocietyName}";
            }
            */
            return $"Unsupported object type {objet.GetType().ToString()}";
        }
        #endregion

        #region Public static methods
        public static bool Init()
        {
            Alt.Server.LogInfo("MongoDB Starting ...");

            try
            {
                string host = Config.GetSetting<string>("host");
                string databaseName = Config.GetSetting<string>("database");
                string user = Config.GetSetting<string>("user");
                string password = Config.GetSetting<string>("password");
                int port = Config.GetSetting<int>("port");

                if (!string.IsNullOrEmpty(host))
                    _client = new MongoClient($"mongodb://{user}:{password}@{host}:{port}");
                else
                    _client = new MongoClient();

                _database = _client.GetDatabase(databaseName);

                var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
                ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);
                
                BsonSerializer.RegisterSerializer(typeof(Vector3), new VectorSerializer());
                BsonSerializer.RegisterSerializer(typeof(ClothData), new ClothDataSerializer());
                BsonSerializer.RegisterSerializer(typeof(Color), new ColorBsonSerializer());
                
                BsonClassMap.RegisterClassMap<Location>(cm =>
                {
                    cm.AutoMap();
                    cm.MapProperty(c => c.Pos).SetSerializer(new VectorSerializer());
                    cm.MapProperty(c => c.Rot).SetSerializer(new VectorSerializer());
                });
                /*
                BsonClassMap.RegisterClassMap<Attachment>(cm =>
                {
                    cm.AutoMap();
                    cm.MapProperty(c => c.Bone).SetSerializer(new StringSerializer());
                    cm.MapProperty(c => c.PositionOffset).SetSerializer(new VectorSerializer());
                    cm.MapProperty(c => c.RemoteID).SetSerializer(new UInt32Serializer(BsonType.Int32, new RepresentationConverter(true, true)));
                    cm.MapProperty(c => c.RotationOffset).SetSerializer(new VectorSerializer());
                    cm.MapProperty(c => c.Type).SetSerializer(new ByteSerializer());
                });
                */
                Alt.Server.LogInfo("MongoDB Started!");
                return true;
            }
            catch (Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
                return false;
            }
        }

        public async static Task Insert<T>(string collectionName, T objet, [System.Runtime.CompilerServices.CallerMemberName] string caller = "", [System.Runtime.CompilerServices.CallerFilePath] string file = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
        {
            _dbStats.Calls++;

            try
            {
                if (Config.GetSetting<bool>("DBProfiling"))
                    Alt.Server.LogColored($"~m~{caller}() in {file.Substring(file.LastIndexOf('\\') + 1)} line {line} - {GetObjectDetails(objet)}");

                await GetCollectionSafe<T>(collectionName).InsertOneAsync(objet);
            }
            catch (MongoWriteException be)
            {
                _dbStats.Errors++;
                Alt.Server.LogError(be.Message);
            }
        }

        public async static Task<ReplaceOneResult> Update<T>(T objet, string collectionName, object ID, int requests = 1, [System.Runtime.CompilerServices.CallerMemberName] string caller = "", [System.Runtime.CompilerServices.CallerFilePath] string file = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
        {
            _dbStats.Calls++;

            try
            {
                if (Config.GetSetting<bool>("DBProfiling"))
                    Alt.Server.LogColored($"~m~{caller}() in {file.Substring(file.LastIndexOf('\\') + 1)} line {line} - Requests: {requests} - {GetObjectDetails(objet)}");

                return await GetCollectionSafe<T>(collectionName).ReplaceOneAsync(Builders<T>.Filter.Eq("_id", ID), objet);
            }
            catch (BsonException be)
            {
                _dbStats.Errors++;
                Alt.Server.LogError(be.Message);
            }

            return null;
        }

        public async static Task<UpdateResult> UpdateBankAccount(BankAccount bankAccount, [System.Runtime.CompilerServices.CallerMemberName] string caller = "", [System.Runtime.CompilerServices.CallerFilePath] string file = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
        {
            try
            {
                if (Config.GetSetting<bool>("DBProfiling"))
                    Alt.Server.LogColored($"~m~{caller}() in {file.Substring(file.LastIndexOf('\\') + 1)} line {line} - Bank account: {bankAccount.AccountNumber} - Type: {bankAccount.AccountType.ToString()}");
                
                if (bankAccount.AccountType == AccountType.Business)
                {
                    var collection = GetCollectionSafe<Business.Business>("businesses");
                    var filter = Builders<Business.Business>.Filter.Eq("_id", ((Business.Business)bankAccount.Owner)._id);
                    var update = Builders<Business.Business>.Update.Set("BankAccount", bankAccount);
                    return await collection.UpdateOneAsync(filter, update);
                }
                else if (bankAccount.AccountType == AccountType.Faction)
                {
                    var collection = GetCollectionSafe<Faction>("factions");
                    var filter = Builders<Faction>.Filter.Eq("_id", ((Faction)bankAccount.Owner).FactionName);
                    var update = Builders<Faction>.Update.Set("BankAccount", bankAccount);
                    return await collection.UpdateOneAsync(filter, update);
                }
                else if (bankAccount.AccountType == AccountType.Society)
                {
                    var collection = GetCollectionSafe<Society.Society>("society");
                    var filter = Builders<Society.Society>.Filter.Eq("_id", ((Society.Society)bankAccount.Owner)._id);
                    var update = Builders<Society.Society>.Update.Set("BankAccount", bankAccount);
                    return await collection.UpdateOneAsync(filter, update);
                }
                else  if (bankAccount.AccountType == AccountType.Personal)
                {
                    var collection = GetCollectionSafe<PlayerHandler>("players");
                    var filter = Builders<PlayerHandler>.Filter.Eq("_id", ((PlayerHandler)bankAccount.Owner).PID);
                    var update = Builders<PlayerHandler>.Update.Set("BankAccount", bankAccount);
                    return await collection.UpdateOneAsync(filter, update);
                }
            }
            catch (BsonException be)
            {
                Alt.Server.LogError(be.Message);
            }

            return null;
        }

        public async static Task<DeleteResult> Delete<T>(string collectionName, object ID, [System.Runtime.CompilerServices.CallerMemberName] string caller = "", [System.Runtime.CompilerServices.CallerFilePath] string file = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
        {
            _dbStats.Calls++;

            try
            {
                if (Config.GetSetting<bool>("DBProfiling"))
                    Alt.Server.LogColored($"~m~{caller}() in {file.Substring(file.LastIndexOf('\\') + 1)} line {line} - Object ID: {ID}");

                return await _database.GetCollection<T>(collectionName).DeleteOneAsync(Builders<T>.Filter.Eq("_id", ID));
            }
            catch (BsonException be)
            {
                _dbStats.Errors++;
                Alt.Server.LogError(be.Message);
            }

            return null;
        }

        public static IMongoCollection<T> GetCollectionSafe<T>(string collectionName)
        {
            if (_database.GetCollection<T>(collectionName) == null)
                _database.CreateCollectionAsync(collectionName);

            return _database.GetCollection<T>(collectionName);
        }

        public static bool CollectionExist<T>(string collectionName)
        {

            if (_database == null)
                return false;

            if (_database.GetCollection<T>(collectionName) == null)
                return false;

            if (_database.GetCollection<T>(collectionName).CountDocuments(new BsonDocument()) == 0)
                return false;

            return true;
        }

        public static IMongoDatabase GetMongoDatabase() => _database;
        #endregion
    }
}