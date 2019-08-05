using AltV.Net;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using ResurrectionRP.Server;
using ResurrectionRP_Server.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Database
{
    public class MongoDB
    {
        #region Private Variables
        private static IMongoClient _client;
        private static IMongoDatabase _database;
        #endregion

        public static bool Init()
        {
            Alt.Server.LogInfo("MongoDB Starting ...");
            try
            {
                string _host = Config.GetSetting<string>("host");
                string _databaseName = Config.GetSetting<string>("database");
                string _user = Config.GetSetting<string>("user");
                string _password = Config.GetSetting<string>("password");
                int _port = Config.GetSetting<int>("port");

                if (!string.IsNullOrEmpty(_host))
                {
                    _client = new MongoClient($"mongodb://{_user}:{_password}@{_host}:{_port}");
                }
                else
                {
                    _client = new MongoClient();
                }

                _database = _client.GetDatabase(_databaseName);

                var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
                ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);
                /*
                BsonSerializer.RegisterSerializer(typeof(Vector3), new VectorSerializer());
                BsonSerializer.RegisterSerializer(typeof(ClothData), new ClothDataSerializer());
                BsonSerializer.RegisterSerializer(typeof(Color), new ColorBsonSerializer());
                */
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

        public async static Task Insert<T>(string collectionName, T objet)
        {
            try
            {
                await GetCollectionSafe<T>(collectionName).InsertOneAsync(objet);
            }
            catch (MongoWriteException be)
            {
                Alt.Server.LogError(be.Message);
            }
        }

        public async static Task<ReplaceOneResult> Update<T>(T objet, string collectionName, object ID)
        {
            try
            {
                return await GetCollectionSafe<T>(collectionName).ReplaceOneAsync(Builders<T>.Filter.Eq("_id", ID), objet, new UpdateOptions { IsUpsert = true });
            }
            catch (BsonException be)
            {
                Alt.Server.LogError(be.Message);
            }
            return null;
        }

        public async static Task<DeleteResult> Delete<T>(string collectionName, object ID)
        {
            try
            {
                return await _database.GetCollection<T>(collectionName).DeleteOneAsync(Builders<T>.Filter.Eq("_id", ID));
            }
            catch (BsonException be)
            {
                Alt.Server.LogError(be.Message);
            }
            return null;
        }

        public static IMongoCollection<T> GetCollectionSafe<T>(string collectionName)
        {
            if (_database.GetCollection<T>(collectionName) == null)
            {
                _database.CreateCollectionAsync(collectionName);
            }
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
    }
}