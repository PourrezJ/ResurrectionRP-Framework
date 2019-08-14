/**using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using Newtonsoft.Json;
using ResurrectionRP.Server.Entities.Vehicles.Data;
using ResurrectionRP_Server.Entities.Vehicles.Data;
using ResurrectionRP_Server.Models;
using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    [BsonIgnoreExtraElements]
    public partial class VehicleHandler
    {
        #region Variable
        [BsonId]
        public string Plate { get; set; }

        [BsonIgnore, JsonIgnore]
        public IPlayer Owner { get; private set; }

        private IVehicle vehicle;
        [BsonIgnore, JsonIgnore]
        public IVehicle Vehicle
        {
            get
            {
                return vehicle;
            }
            set
            {
                if (value != null)
                {
                    VehicleSync.Vehicle = value;
                }
                vehicle = value;
            }
        }

        [BsonRepresentation(BsonType.Int32, AllowOverflow = true)]
        public uint Model { get; private set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public ConcurrentDictionary<int, int> Mods { get; set; } = new ConcurrentDictionary<int, int>();

        public string OwnerID { get; set; } // SocialClubName
        [BsonRepresentation(BsonType.Int32, AllowOverflow = true)]
        public uint Dimension { get; set; } = uint.MaxValue;

        public bool SpawnVeh { get; set; }
        public bool Locked { get; set; } = true;

        public int PrimaryColor { get; set; }
        public int SecondaryColor { get; set; }

        private VehicleSync _vehicleSync = null;
        public VehicleSync VehicleSync
        {
            get
            {
                if (_vehicleSync == null)
                    _vehicleSync = new VehicleSync();
                return _vehicleSync;
            }
            set => _vehicleSync = value;
        }

        public DateTime LastUse { get; set; } = DateTime.Now;
        public string LastDriver { get; set; }

        public bool PlateHide;

        public OilTank OilTank = null;
        #endregion

        #region Events
        public delegate Task PlayerEnterVehicle(IPlayer client, IVehicle vehicle, int seat);
        public delegate Task PlayerQuitVehicle(IPlayer client);
        [BsonIgnore, JsonIgnore]
        public PlayerQuitVehicle OnPlayerQuitVehicle { get; set; } = null;
        [BsonIgnore, JsonIgnore]
        public PlayerEnterVehicle OnPlayerEnterVehicle { get; set; } = null;
        #endregion

        #region Constructor
        public VehicleHandler(string socialClubName, VehicleModel model, Vector3 position, Vector3 rotation, int primaryColor = 0, int secondaryColor = 0,
            float fuel = 100, float fuelMax = 100, string plate = null, bool engineStatus = false, bool locked = true,
            IPlayer owner = null, ConcurrentDictionary<int, int> mods = null, int[] neon = null, bool spawnVeh = false, uint dimension = uint.MaxValue, Inventory inventory = null, bool freeze = false, float dirt = 0, float health = 1000)
        {
            if (model == 0)
                return;
            OwnerID = socialClubName;
            Model = (uint)model;
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;

            VehicleSync = new VehicleSync
            {
                Location = new Location(position, rotation),
                Fuel = fuel,
                FuelMax = fuelMax,
                Engine = engineStatus,
                FreezePosition = freeze,
                Dirt = dirt,
            };

            VehicleSync.FreezePosition = freeze;

            //Plate = (string.IsNullOrEmpty(plate)) ? VehicleManager.GenerateRandomPlate() : plate;

            Locked = locked;
            Owner = owner;

            if (mods != null)
                Mods = mods;

            SpawnVeh = spawnVeh;
            Dimension = dimension;
            
            if (inventory != null)
                Inventory = inventory;
                
            if (OilTank == null)
                OilTank = new OilTank();

        }
        #endregion
    }
}
**/