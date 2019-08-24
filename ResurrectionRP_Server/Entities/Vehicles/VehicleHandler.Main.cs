using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using Newtonsoft.Json;
using ResurrectionRP.Server.Entities.Vehicles.Data;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles.Data;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using VehicleInfoLoader.Data;

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

        [BsonIgnore, JsonIgnore]
        public VehicleManifest VehicleManifest;

        [BsonRepresentation(BsonType.Int32, AllowOverflow = true)]
        public uint Model { get; private set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public ConcurrentDictionary<int, int> Mods { get; set; }
            = new ConcurrentDictionary<int, int>();

        public string OwnerID { get; set; } // SocialClubName
        [BsonRepresentation(BsonType.Int32, AllowOverflow = true)]
        public short Dimension { get; set; } = short.MaxValue;

        [BsonIgnore, JsonIgnore]
        public IVehicle Vehicle { get; set; }

        public bool isParked { get; set; } = false;

        public bool SpawnVeh { get; set; }
        public bool Locked { get; set; } = true;

        public byte PrimaryColor { get; set; }
        public byte SecondaryColor { get; set; }

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
        public VehicleHandler(string socialClubName, uint model, Vector3 position, Vector3 rotation, byte primaryColor = 0, byte secondaryColor = 0,
            float fuel = 100, float fuelMax = 100, string plate = null, bool engineStatus = false, bool locked = true,
            IPlayer owner = null, ConcurrentDictionary<int, int> mods = null, int[] neon = null, bool spawnVeh = false, short dimension = short.MaxValue, Inventory.Inventory inventory = null, bool freeze = false, byte dirt = 0, float health = 1000)
        {
            if (model == 0)
                return;
            OwnerID = socialClubName;
            Model = model;
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;

            FreezePosition = freeze;

            Plate = (string.IsNullOrEmpty(plate)) ? VehiclesManager.GenerateRandomPlate() : plate;

            Locked = locked;
            Owner = owner;

            if (mods != null)
                Mods = mods;

            SpawnVeh = spawnVeh;
            Dimension = dimension;
            Location = new Location(position, rotation);
            /*
            if (inventory != null)
                Inventory = inventory;*/

            if (OilTank == null)
                OilTank = new OilTank();
        }
        #endregion

        #region Method
        public async Task<IVehicle> SpawnVehicle(Location location = null)
        {
            if (Dimension.ToString() == "-1")
                Dimension = short.MaxValue;

            await AltAsync.Do(async () =>
            {
                try
                {
                    if(location == null)
                        Vehicle = Alt.CreateVehicle(Model,  Location.Pos , Location.GetRotation() );
                    else
                        Vehicle = Alt.CreateVehicle(Model, location.Pos, location.GetRotation());
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError("SpawnVehicle: " + ex);
                }
                if (Vehicle == null)
                    return;

                Vehicle = Vehicle;
                Vehicle.SetData("VehicleHandler", this);

                Vehicle.Dimension = Dimension;
                Vehicle.NumberplateText = Plate;
                Vehicle.PrimaryColor = PrimaryColor;
                Vehicle.SecondaryColor = SecondaryColor;

                if (Mods.Count > 0)
                {
                    foreach (KeyValuePair<int, int> mod in Mods)
                    {
                        Vehicle.SetMod((byte)mod.Key, (byte)mod.Value);
                        if (mod.Key == 69)
                        {
                            Vehicle.WindowTint = (byte)mod.Value;
                        }
                    }
                }

                if (NeonsColor != null && NeonsColor != new Color())
                    Vehicle.NeonColor = NeonsColor;


                Vehicle.DirtLevel = Dirt;
                Vehicle.LockState = Locked ? VehicleLockState.Locked : VehicleLockState.Unlocked;
                Vehicle.EngineOn = Engine;
                Vehicle.EngineHealth = EngineHealth;
                Vehicle.BodyHealth = BodyHealth;
                Vehicle.RadioStation = RadioID;

                await Vehicle.SetLockStateAsync(Locked ? VehicleLockState.Locked : VehicleLockState.Unlocked);
                await Vehicle.SetEngineOnAsync(Engine);


                LastUse = DateTime.Now;

                if (location != null)
                {
                    Location = location;
                }

                VehicleManifest = VehicleInfoLoader.VehicleInfoLoader.Get(Model);

                GameMode.Instance.VehicleManager.VehicleHandlerList.TryAdd(Vehicle, this);
            });
            /*
            if (HaveTowVehicle())
            {
                IVehicle _vehtowed = VehicleManager.GetVehicleWithPlate(VehicleSync.TowTruck.VehPlate);
                if (_vehtowed != null)
                {
                    await TowVehicle(_vehtowed);
                }
            }*/

            return Vehicle;
        }

        public async Task Delete(bool perm = false)
        {
            if (Vehicle.Exists)
                await Vehicle.RemoveAsync();

            if (GameMode.Instance.VehicleManager.VehicleHandlerList.Remove(Vehicle, out VehicleHandler value))
            {
                if (perm)
                {
                    await RemoveInDatabase();
                    GameMode.Instance.PlateList.Remove(Plate);
                }
            }
        }
        
        public async Task LockUnlock(IPlayer client, bool statut)
        {
            VehicleHandler VH = VehiclesManager.GetHandlerByVehicle(Vehicle);

            if (PlayerManager.HasVehicleKey(client, await Vehicle.GetNumberplateTextAsync()) || VH.SpawnVeh && VH.OwnerID == client.GetSocialClub())
            {
                Locked = statut;
                await Vehicle.SetLockStateAsync(statut ? VehicleLockState.Locked : VehicleLockState.Unlocked);
                await client.NotifyAsync($"Vous avez {(statut ? " ~r~ouvert" : "~g~fermé")} ~w~le véhicule");
            }
        }  

        public async Task<bool> LockUnlock(IPlayer client)
        {
            VehicleHandler VH = VehiclesManager.GetHandlerByVehicle(Vehicle);

            if (PlayerManager.HasVehicleKey(client, await Vehicle.GetNumberplateTextAsync()) || VH.SpawnVeh && VH.OwnerID == client.GetSocialClub())
            {
                Locked = await Vehicle.GetLockStateAsync() == VehicleLockState.Locked ? false : true;
                await Vehicle.SetLockStateAsync(Locked ? VehicleLockState.Locked : VehicleLockState.Unlocked);
                await client.NotifyAsync($"Vous avez {(Locked ? " ~r~fermé" : "~g~ouvert")} ~w~le véhicule");

                return true;
            }
            return false;
        }
        
        public void SetOwner(IPlayer player) => OwnerID = player.GetSocialClub();
        public void SetOwner(PlayerHandler player) => OwnerID = player.Client.GetSocialClub();

        #endregion
    }
}