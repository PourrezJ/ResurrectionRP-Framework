using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles.Data;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
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
        #region Fields
        [BsonId]
        public string Plate { get; set; }

        [BsonIgnore, JsonIgnore]
        public IPlayer Owner { get; private set; }

        [BsonIgnore, JsonIgnore]
        public VehicleManifest VehicleManifest;

        [BsonRepresentation(BsonType.Int32, AllowOverflow = true)]
        public uint Model { get; private set; }
        
        public string OwnerID { get; set; } // SocialClubName
        [BsonRepresentation(BsonType.Int32, AllowOverflow = true)]
        public short Dimension { get; set; } = short.MaxValue;

        [BsonIgnore, JsonIgnore]
        public IVehicle Vehicle { get; set; }

        public Inventory.Inventory Inventory { get; set; }

        public bool IsParked { get; set; } = false;
        public bool IsInPound { get; set; } = false;

        [BsonIgnore]
        public bool SpawnVeh { get; set; }
        public bool Locked { get; set; } = true;
        
        public DateTime LastUse { get; set; } = DateTime.Now;
        public string LastDriver { get; set; }

        public bool PlateHide;

        [BsonIgnoreIfNull]
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
            IPlayer owner = null, ConcurrentDictionary<byte, byte> mods = null, int[] neon = null, bool spawnVeh = false, short dimension = short.MaxValue, Inventory.Inventory inventory = null, bool freeze = false, byte dirt = 0, float health = 1000)
        {
            if (model == 0)
                return;

            OwnerID = socialClubName;
            Model = model;
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;

            Plate = string.IsNullOrEmpty(plate) ? VehiclesManager.GenerateRandomPlate() : plate;
            Locked = locked;
            Owner = owner;

            if (mods != null)
                Mods = mods;

            SpawnVeh = spawnVeh;
            Dimension = dimension;
            Location = new Location(position, rotation);
            
            if (inventory != null)
                Inventory = inventory;

            if (OilTank == null)
                OilTank = new OilTank();
        }
        #endregion

        #region Method
        public async Task<IVehicle> SpawnVehicle(Location location = null, bool setLastUse = true)
        {
            if (Dimension.ToString() == "-1")
                Dimension = short.MaxValue;

            await AltAsync.Do(() =>
            {
                try
                {
                    if(location == null)
                        Vehicle = Alt.CreateVehicle(Model,  Location.Pos , Location.GetRotation());
                    else
                        Vehicle = Alt.CreateVehicle(Model, location.Pos, location.GetRotation());
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError("SpawnVehicle: " + ex);
                }

                if (Vehicle == null)
                    return;

                Vehicle.ModKit = 1;
                Vehicle.SetData("VehicleHandler", this);
                Vehicle.NumberplateText = Plate;
                Vehicle.PrimaryColor = PrimaryColor;
                Vehicle.SecondaryColor = SecondaryColor;

                if (Mods.Count > 0)
                {
                    foreach (KeyValuePair<byte, byte> mod in Mods)
                    {
                        Vehicle.SetMod(mod.Key, mod.Value);

                        if (mod.Key == 69)
                            Vehicle.WindowTint = mod.Value;
                    }
                }

                if (NeonColor != null && NeonColor != new Color())
                    Vehicle.NeonColor = NeonColor;

                Vehicle.DirtLevel = Dirt;
                Vehicle.LockState = Locked ? VehicleLockState.Locked : VehicleLockState.Unlocked;
                Vehicle.EngineOn = EngineOn;
                Vehicle.EngineHealth = EngineHealth;
                Vehicle.BodyHealth = BodyHealth;
                Vehicle.RadioStation = RadioStation;
                IsParked = false;

                if (Wheels == null)
                {
                    Wheels = new Wheel[Vehicle.WheelsCount];

                    for (int i = 0; i < Wheels.Length; i++)
                        Wheels[i] = new Wheel();
                }

                for (byte i = 0; i < Vehicle.WheelsCount; i++)
                {
                    Vehicle.SetWheelBurst(i, Wheels[i].Burst);
                    Vehicle.SetWheelHealth(i, Wheels[i].Health);
                    Vehicle.SetWheelHasTire(i, Wheels[i].HasTire);
                }
                
                for(byte i = 0; i < Globals.NB_VEHICLE_DOORS; i++)
                    Vehicle.SetDoorState(i, (byte)Doors[i]);

                for (byte i = 0; i < Globals.NB_VEHICLE_WINDOWS; i++)
                {
                    if (Windows[i] == WindowState.WindowBroken)
                        Vehicle.SetWindowDamaged(i, true);
                    else if (Windows[i] == WindowState.WindowDown)
                        Vehicle.SetWindowOpened(i, true);
                }

                Vehicle.SetBumperDamageLevel(VehicleBumper.Front, FrontBumperDamage);
                Vehicle.SetBumperDamageLevel(VehicleBumper.Rear, RearBumperDamage);

                Vehicle.SetWindowTint(WindowTint);
                /*
                if (!string.IsNullOrEmpty(DamageData))
                    Vehicle.DamageData = DamageData;

                if (!string.IsNullOrEmpty(AppearanceData))
                    Vehicle.AppearanceData = AppearanceData;*/

                if (setLastUse)
                    LastUse = DateTime.Now;

                if (location != null)
                    Location = location;

                Vehicle.LockState = Locked ? VehicleLockState.Locked : VehicleLockState.Unlocked;
                Vehicle.EngineOn = EngineOn;
                Vehicle.Position = Location.Pos;
                _previousPosition = Location.Pos;

                VehicleManifest = VehicleInfoLoader.VehicleInfoLoader.Get(Model);
                VehiclesManager.VehicleHandlerList.TryAdd(Vehicle, this);
                
                // Needed as vehicles in database don't have this value
                if (FuelConsumption == 0)
                    FuelConsumption = 5.5f;

                Vehicle.Dimension = Dimension;
            });

            if (HaveTowVehicle())
            {
                IVehicle _vehtowed = VehiclesManager.GetVehicleWithPlate(TowTruck.VehPlate);

                if (_vehtowed != null)
                    await TowVehicle(_vehtowed);
            }

            return Vehicle;
        }

        public async Task<bool> Delete(bool perm = false)
        {
            if (Vehicle.Exists)
                await Vehicle.RemoveAsync();

            if (VehiclesManager.VehicleHandlerList.TryRemove(Vehicle, out VehicleHandler _))
            {
                if (perm && !SpawnVeh)
                {
                    if (!await RemoveInDatabase())
                        return false;
                }

                return true;
            }

            return false;
        }

        public async Task ApplyDamageAsync()
        {
            if (Vehicle != null && Vehicle.Exists)
            {
                await Vehicle.SetDimensionAsync(-1);
                await Vehicle.SetDimensionAsync(GameMode.GlobalDimension);
            }
        }
        
        public async Task LockUnlock(IPlayer client, bool statut)
        {
            VehicleHandler VH = Vehicle.GetVehicleHandler();

            if (client.HasVehicleKey(await Vehicle.GetNumberplateTextAsync()) || VH.SpawnVeh && VH.OwnerID == client.GetSocialClub())
            {
                Locked = statut;
                await Vehicle.SetLockStateAsync(statut ? VehicleLockState.Locked : VehicleLockState.Unlocked);
                client.SendNotification($"Vous avez {(statut ? " ~r~ouvert" : "~g~fermé")} ~w~le véhicule");
            }
        }  

        public async Task<bool> LockUnlock(IPlayer client)
        {
            VehicleHandler VH = Vehicle.GetVehicleHandler() ;

            if (client.HasVehicleKey( await Vehicle.GetNumberplateTextAsync()) || VH.SpawnVeh && VH.OwnerID == client.GetSocialClub())
            {
                Locked = await Vehicle.GetLockStateAsync() == VehicleLockState.Locked ? false : true;
                await Vehicle.SetLockStateAsync(Locked ? VehicleLockState.Locked : VehicleLockState.Unlocked);
                client.SendNotification($"Vous avez {(Locked ? " fermé" : "ouvert")} le véhicule");

                return true;
            }

            return false;
        }
        
        public void AddFuel(float fuel)
        {
            if (Fuel + fuel > FuelMax)
                Fuel = FuelMax;
            else
                Fuel += fuel;

            UpdateFull();
        }

        public void UpdateProperties()
        {
            try
            {
                // Needed as when fuel gets to 0 there is a vehicle save and engine stop information hasn't been set back from client
                if (Fuel != 0)
                    EngineOn = Vehicle.EngineOn;

                for (byte i = 0; i < 100; i++)
                {
                    if (Enum.IsDefined(typeof(AltV.Net.Enums.VehicleModType), i) && Vehicle.GetMod(i) > 0)
                        Mods[i] = Vehicle.GetMod(i);
                }
                /*
                AltV.Net.Enums.VehicleModType[] values = (AltV.Net.Enums.VehicleModType[])Enum.GetValues(typeof(AltV.Net.Enums.VehicleModType));

                foreach (AltV.Net.Enums.VehicleModType vehicleModType in values)
                {
                    if (Vehicle.GetMod(vehicleModType) > 0)
                        Mods[(int)vehicleModType] = Vehicle.GetMod(vehicleModType);
                }
                */
                DamageData = Vehicle.DamageData;
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("Error on vehicle save: " + ex.ToString());
            }
        }

        public Task PutPlayerInVehicle( IPlayer client )
        {
            //TODO
            return Task.CompletedTask;
        }

        public void SetOwner(IPlayer player) => OwnerID = player.GetSocialClub();

        public void SetOwner(PlayerHandler player) => OwnerID = player.Client.GetSocialClub();
        #endregion
    }
}