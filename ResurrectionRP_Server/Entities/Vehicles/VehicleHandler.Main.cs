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
        #region Variable
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

            await AltAsync.Do(async () =>
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
                            Vehicle.WindowTint = (byte)mod.Value;
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
                IsParked = false;

                for (byte i = 0; i < Vehicle.WheelsCount; i++)
                {
                    // TODO : Wheels == null when spawned vehicle
                    // Vehicle.SetWheelBurst(i, Wheels[i].Burst);
                    // Vehicle.SetWheelHealth(i, Wheels[i].Health);
                    // Vehicle.SetWheelHasTire(i, Wheels[i].HasTire);
                }

                for(byte i = 0; i < Globals.NB_VEHICLE_DOORS; i++)
                    await Vehicle.SetDoorStateAsync(i,(byte) Doors[i]);

                for (byte i = 0; i < Globals.NB_VEHICLE_WINDOWS; i++)
                {
                    if (Windows[i] == WindowState.WindowBroken)
                        Vehicle.SetWindowDamaged(i, true);
                    else if (Windows[i] == WindowState.WindowDown)
                        await Vehicle.SetWindowOpenedAsync(i, true);
                }

                await Vehicle.SetLockStateAsync(Locked ? VehicleLockState.Locked : VehicleLockState.Unlocked);
                await Vehicle.SetEngineOnAsync(Engine);
                await Vehicle.SetPositionAsync(Vehicle.Position.X, Vehicle.Position.Y, Vehicle.Position.Z );

                if (setLastUse)
                    LastUse = DateTime.Now;

                if (location != null)
                    Location = location;

                Vehicle.LockState = Locked ? VehicleLockState.Locked : VehicleLockState.Unlocked;
                Vehicle.EngineOn = Engine;
                Vehicle.Position = Location.Pos;

                VehicleManifest = VehicleInfoLoader.VehicleInfoLoader.Get(Model);
                VehiclesManager.VehicleHandlerList.TryAdd(Vehicle, this);

                if (Vehicle.GetVehicleHandler()?.FuelConsumption == 0)
                    Vehicle.GetVehicleHandler().FuelConsumption = 5.5f;

                if (HaveTowVehicle())
                {
                    IVehicle _vehtowed = VehiclesManager.GetVehicleWithPlate(TowTruck.VehPlate);

                    if (_vehtowed != null)
                        await TowVehicle(_vehtowed);
                }
            });
            
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
        
        public void SetFuel(float fuel)
        {
            Fuel = fuel;

            if(Vehicle.Driver != null)
                Vehicle.Driver.EmitLocked("UpdateFuel", fuel);

            Update();
        }

        public void AddFuel(float fuel)
        {
            if (Fuel + fuel > FuelMax)
            {
                Fuel = FuelMax;
            }
            else
                Fuel += fuel;

            Update();
        }
        public float GetFuel() => Fuel;

        public void UpdateProperties()
        {
            if (Doors == null)
                Doors = new VehicleDoorState[Globals.NB_VEHICLE_DOORS];

            if (Wheels == null)
                Wheels = new Wheel[Vehicle.WheelsCount];

            try
            {
                Dirt = Vehicle.DirtLevel;
                Engine = Vehicle.EngineOn;
                EngineHealth = Vehicle.EngineHealth;
                BodyHealth = Vehicle.BodyHealth;
                RadioID = Vehicle.RadioStation;
                bool neonActive = Vehicle.IsNeonActive;
                Tuple<bool, bool, bool, bool> NeonState = new Tuple<bool, bool, bool, bool>(neonActive, neonActive, neonActive, neonActive);
                NeonsColor = Vehicle.NeonColor;

                for (byte i = 0; i < Globals.NB_VEHICLE_DOORS; i++)
                    Doors[i] = (VehicleDoorState)Vehicle.GetDoorState(i);

                for (byte i = 0; i < Globals.NB_VEHICLE_WINDOWS; i++)
                {
                    if (Vehicle.IsWindowDamaged(i))
                        Windows[i] = WindowState.WindowBroken;
                    else if (Vehicle.IsWindowOpened(i))
                        Windows[i] = WindowState.WindowDown;
                    else
                        Windows[i] = WindowState.WindowFixed;
                }

                for (byte i = 0; i < Vehicle.WheelsCount; i++)
                {
                    Wheels[i] = new Wheel();
                    Wheels[i].Health = Vehicle.GetWheelHealth(i);
                    Wheels[i].Burst = Vehicle.IsWheelBurst(i);
                }

                Location.Pos = Vehicle.Position;
                Location.Rot = Vehicle.Rotation;
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("Error on veicle save: " + ex.ToString());
            }

            //this.NeonState.Clear();
            //this.NeonState.Add(NeonState.Item1);
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