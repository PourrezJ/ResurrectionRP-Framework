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
       
        public DateTime LastUse { get; set; } = DateTime.Now;

        public string LastDriver { get; set; }

        public bool PlateHide { get; set; } = false;

        public string ParkingName { get; set; }

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
            LockState = locked ? VehicleLockState.Locked : VehicleLockState.Unlocked;
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
        public bool IsLocked()
        {
            return (LockState == VehicleLockState.Locked) ? true : false;
        }

        public async Task<IVehicle> SpawnVehicle(Location location = null, bool setLastUse = true)
        {
            if (Dimension.ToString() == "-1")
                Dimension = short.MaxValue;

            await AltAsync.Do(() =>
            {
                IVehicle vehicle = null;

                try
                {
                    if (location == null)
                        vehicle = Alt.CreateVehicle(Model,  Location.Pos , Location.GetRotation());
                    else
                        vehicle = Alt.CreateVehicle(Model, location.Pos, location.GetRotation());
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError("SpawnVehicle: " + ex);
                }

                if (vehicle == null)
                    return;

                vehicle.ModKit = 1;
                vehicle.SetData("VehicleHandler", this);
                vehicle.NumberplateText = Plate;
                vehicle.PrimaryColor = PrimaryColor;
                vehicle.SecondaryColor = SecondaryColor;
                vehicle.PearlColor = PearlColor;

                if (Mods.Count > 0)
                {
                    foreach (KeyValuePair<byte, byte> mod in Mods)
                    {
                        vehicle.SetMod(mod.Key, mod.Value);

                        if (mod.Key == 69)
                            vehicle.WindowTint = mod.Value;
                    }
                }

                // BUG v792 : NeonState and NeonColor not working properly
                // if (NeonColor != null && NeonColor != Color.Empty)
                //     vehicle.NeonColor = NeonColor;
                // vehicle.SetNeonActive(NeonState.Item1, NeonState.Item2, NeonState.Item3, NeonState.Item4);
                vehicle.SetSyncedMetaData("NeonColor", NeonColor.ToArgb());
                vehicle.SetSyncedMetaData("NeonState", NeonState.Item1);

                vehicle.DirtLevel = DirtLevel;
                vehicle.LockState = LockState;
                vehicle.EngineOn = EngineOn;
                vehicle.EngineHealth = EngineHealth;
                vehicle.BodyHealth = BodyHealth;
                vehicle.RadioStation = RadioStation;
                IsParked = false;

                if (Wheels == null)
                {
                    Wheels = new Wheel[vehicle.WheelsCount];

                    for (int i = 0; i < Wheels.Length; i++)
                        Wheels[i] = new Wheel();
                }

                for (byte i = 0; i < vehicle.WheelsCount; i++)
                {
                    vehicle.SetWheelBurst(i, Wheels[i].Burst);
                    vehicle.SetWheelHealth(i, Wheels[i].Health);
                    vehicle.SetWheelHasTire(i, Wheels[i].HasTire);
                }
                
                for(byte i = 0; i < Globals.NB_VEHICLE_DOORS; i++)
                    vehicle.SetDoorState(i, (byte)Doors[i]);

                for (byte i = 0; i < Globals.NB_VEHICLE_WINDOWS; i++)
                {
                    if (Windows[i] == WindowState.WindowBroken)
                        vehicle.SetWindowDamaged(i, true);
                    else if (Windows[i] == WindowState.WindowDown)
                        vehicle.SetWindowOpened(i, true);
                }

                vehicle.SetBumperDamageLevel(VehicleBumper.Front, FrontBumperDamage);
                vehicle.SetBumperDamageLevel(VehicleBumper.Rear, RearBumperDamage);

                vehicle.SetWindowTint(WindowTint);
                /*
                if (!string.IsNullOrEmpty(DamageData))
                    vehicle.DamageData = DamageData;

                if (!string.IsNullOrEmpty(AppearanceData))
                    vehicle.AppearanceData = AppearanceData;*/

                if (setLastUse)
                    LastUse = DateTime.Now;

                if (location != null)
                    Location = location;

                vehicle.Position = Location.Pos;
                _previousPosition = Location.Pos;

                vehicle.Dimension = Dimension;
                Vehicle = vehicle;

                VehicleManifest = VehicleInfoLoader.VehicleInfoLoader.Get(Model);
                VehiclesManager.VehicleHandlerList.TryAdd(vehicle, this);
                
                // Needed as vehicles in database don't have this value
                if (FuelConsumption == 0)
                    FuelConsumption = 5.5f;
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
        
        public void LockUnlock(IPlayer client, bool locked)
        {
            VehicleHandler vh = Vehicle.GetVehicleHandler();

            if (client.HasVehicleKey(vh.Plate) || vh.SpawnVeh && vh.OwnerID == client.GetSocialClub())
            {
                LockState = locked ? VehicleLockState.Locked : VehicleLockState.Unlocked;
                client.SendNotification($"Vous avez {(locked ? " ~r~ouvert" : "~g~fermé")} ~w~le véhicule");
            }
        }  

        public async Task<bool> LockUnlock(IPlayer client)
        {
            VehicleHandler VH = Vehicle.GetVehicleHandler() ;

            if (client.HasVehicleKey( await Vehicle.GetNumberplateTextAsync()) || VH.SpawnVeh && VH.OwnerID == client.GetSocialClub())
            {
                LockState = (LockState == VehicleLockState.Locked) ? VehicleLockState.Unlocked : VehicleLockState.Locked;
                client.SendNotification($"Vous avez {(LockState == VehicleLockState.Locked ? " fermé" : "ouvert")} le véhicule");

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
            RadioStation = Vehicle.RadioStation;
            LockState = Vehicle.LockState;
            BodyHealth = Vehicle.BodyHealth;
            EngineHealth = Vehicle.EngineHealth;
            PetrolTankHealth = Vehicle.PetrolTankHealth;

            // BUG v792 : NeonColor and NeonState nor working properly 
            // bool neonActive = Vehicle.IsNeonActive;
            // NeonState = new Tuple<bool, bool, bool, bool>(neonActive, neonActive, neonActive, neonActive);
            // NeonColor = Vehicle.NeonColor;
            
            DirtLevel = Vehicle.DirtLevel;

            // Needed as when fuel gets to 0 there is a vehicle save and engine stop information hasn't been set back from client
            if (Fuel > 0)
                EngineOn = Vehicle.EngineOn;

            PrimaryColor = Vehicle.PrimaryColor;
            SecondaryColor = Vehicle.SecondaryColor;
            PearlColor = Vehicle.PearlColor;
            WindowTint = Vehicle.GetWindowTint();
            FrontBumperDamage = Vehicle.GetBumperDamageLevel(AltV.Net.Enums.VehicleBumper.Front);
            RearBumperDamage = Vehicle.GetBumperDamageLevel(AltV.Net.Enums.VehicleBumper.Rear);
            DamageData = Vehicle.DamageData;

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
            /*
            for (byte i = 0; i < 100; i++)
            {
                if (Enum.IsDefined(typeof(AltV.Net.Enums.VehicleModType), i) && Vehicle.GetMod(i) > 0)
                    Mods[i] = Vehicle.GetMod(i);
            }
            */
            AltV.Net.Enums.VehicleModType[] values = (AltV.Net.Enums.VehicleModType[])Enum.GetValues(typeof(AltV.Net.Enums.VehicleModType));
            
            foreach (AltV.Net.Enums.VehicleModType vehicleModType in values)
            {
                if (Vehicle.GetMod(vehicleModType) > 0)
                    Mods[(byte)vehicleModType] = Vehicle.GetMod(vehicleModType);
            }

            Location.Pos = Vehicle.Position;
            Location.Rot = Vehicle.Rotation;
        }

        public void Repair(IPlayer player)
        {
            if (Vehicle == null || !Vehicle.Exists || player == null || !player.Exists)
                return;

            BodyHealth = 1000;
            Doors = new VehicleDoorState[Globals.NB_VEHICLE_DOORS] { 0, 0, 0, 0, 0, 0, 0, 0 };
            Windows = new WindowState[Globals.NB_VEHICLE_WINDOWS] { 0, 0, 0, 0 };
            Wheels = new Wheel[Vehicle.WheelsCount];

            for (int i = 0; i < Wheels.Length; i++)
                Wheels[i] = new Wheel();

            FrontBumperDamage = 0;
            RearBumperDamage = 0;
            DamageData = string.Empty;
            player.EmitLocked("vehicleFix", Vehicle);
        }

        public void SetOwner(IPlayer player) => OwnerID = player.GetSocialClub();

        public void SetOwner(PlayerHandler player) => OwnerID = player.Client.GetSocialClub();
        #endregion
    }
}