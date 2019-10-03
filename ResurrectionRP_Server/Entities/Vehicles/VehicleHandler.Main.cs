﻿using AltV.Net;
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
        public short Dimension { get; set; } = GameMode.GlobalDimension;

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

        public string ParkingName { get; set; } = string.Empty;

        [BsonIgnore]
        public bool WasTeleported { get; set; } = false;


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
            IPlayer owner = null, ConcurrentDictionary<byte, byte> mods = null, int[] neon = null, bool spawnVeh = false, short dimension = GameMode.GlobalDimension, Inventory.Inventory inventory = null, bool freeze = false, byte dirt = 0, float health = 1000)
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

        #region Methods
        public bool IsLocked()
        {
            return (LockState == VehicleLockState.Locked) ? true : false;
        }

        public async Task<IVehicle> SpawnVehicleAsync(Location location = null, bool setLastUse = true)
        {
            IVehicle vehicle = null;

            await AltAsync.Do(() => { vehicle = SpawnVehicle(location, setLastUse); });

            return vehicle;
        }

        public IVehicle SpawnVehicle(Location location = null, bool setLastUse = true)
        {
            Dimension = GameMode.GlobalDimension;

            try
            {
                if (location != null)
                    Location = location;

                Vehicle = Alt.CreateVehicle(Model, Location.Pos, Location.GetRotation());
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("SpawnVehicle: " + ex);
            }

            if (Vehicle == null)
                return null;

            Vehicle.ModKit = 1;
            Vehicle.SetData("VehicleHandler", this);
            Vehicle.NumberplateText = Plate;
            Vehicle.PrimaryColor = PrimaryColor;
            Vehicle.SecondaryColor = SecondaryColor;
            Vehicle.PearlColor = PearlColor;

            if (Mods.Count > 0)
            {
                foreach (KeyValuePair<byte, byte> mod in Mods)
                {
                    Vehicle.SetMod(mod.Key, mod.Value);

                    if (mod.Key == 69)
                        Vehicle.WindowTint = mod.Value;
                }
            }

            // BUG v792 : NeonState and NeonColor not working properly
            // if (NeonColor != null && NeonColor != Color.Empty)
            //     vehicle.NeonColor = NeonColor;
            // vehicle.SetNeonActive(NeonState.Item1, NeonState.Item2, NeonState.Item3, NeonState.Item4);
            Vehicle.SetSyncedMetaData("NeonColor", NeonColor.ToArgb());
            Vehicle.SetSyncedMetaData("NeonState", NeonState.Item1);

            Vehicle.DirtLevel = DirtLevel;
            Vehicle.LockState = LockState;
            Vehicle.EngineOn = EngineOn;
            Vehicle.EngineHealth = EngineHealth;
            Vehicle.BodyHealth = BodyHealth;
            Vehicle.RadioStation = RadioStation;

            if (Wheels == null)
            {
                Wheel[] wheels = new Wheel[Vehicle.WheelsCount];

                for (int i = 0; i < wheels.Length; i++)
                    wheels[i] = new Wheel();

                Wheels = wheels;
            }

            for (byte i = 0; i < Vehicle.WheelsCount; i++)
            {
                Vehicle.SetWheelBurst(i, Wheels[i].Burst);
                Vehicle.SetWheelHealth(i, Wheels[i].Health);
                Vehicle.SetWheelHasTire(i, Wheels[i].HasTire);
            }

            for (byte i = 0; i < Globals.NB_VEHICLE_DOORS; i++)
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

            Vehicle.SetSyncedMetaData("torqueMultiplicator", TorqueMultiplicator);
            Vehicle.SetSyncedMetaData("powerMultiplicator", PowerMultiplicator);
            /*
            if (!string.IsNullOrEmpty(DamageData))
                vehicle.DamageData = DamageData;

            if (!string.IsNullOrEmpty(AppearanceData))
                vehicle.AppearanceData = AppearanceData;*/

            if (setLastUse)
                LastUse = DateTime.Now;

            _previousPosition = Location.Pos;
            Vehicle.Dimension = Dimension;

            VehicleManifest = VehicleInfoLoader.VehicleInfoLoader.Get(Model);

            // Needed as vehicles in database don't have this value
            if((VehicleManifest.fuelConsum <= 0 || VehicleManifest.fuelReservoir <= 0) && VehicleManifest.VehicleClass != 13)
            {
                Alt.Server.LogError("Erreur sur le chargement d'un véhicule, le fuel réservoir ou la consommation existe pas : " + Vehicle.Model);
                FuelConsumption = 5.5f;
                FuelMax = 70;
            }
            if(FuelMax == 100 )
            {
                FuelConsumption = VehicleManifest.fuelConsum;
                FuelMax = VehicleManifest.fuelReservoir;
            }

            if (Fuel > FuelMax)
                Fuel = FuelMax;

            VehiclesManager.VehicleHandlerList.TryAdd(Vehicle, this);

            if (HaveTowVehicle())
            {
                IVehicle _vehtowed = VehiclesManager.GetVehicleWithPlate(TowTruck.VehPlate);

                if (_vehtowed != null)
                    Task.Run(async() => { await TowVehicle(_vehtowed); }); 
            }

            ParkingName = string.Empty;
            IsInPound = false;
            IsParked = false;

            return Vehicle;
        }

        public async Task<bool> DeleteAsync(bool perm = false)
        {
            if (Vehicle == null)
                return false;

            if (await Vehicle.ExistsAsync())
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

        public void ApplyDamage()
        {
            if (Vehicle != null && Vehicle.Exists)
            {
                Vehicle.Dimension = -1;
                Vehicle.Dimension = GameMode.GlobalDimension;
            }
        }

        public async Task ApplyDamageAsync()
        {
            if (Vehicle != null && await Vehicle.ExistsAsync())
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

        public bool LockUnlock(IPlayer client)
        {
            VehicleHandler VH = Vehicle.GetVehicleHandler() ;

            if (client.HasVehicleKey(Vehicle.NumberplateText) || VH.SpawnVeh && VH.OwnerID == client.GetSocialClub())
            {
                LockState = (LockState == VehicleLockState.Locked) ? VehicleLockState.Unlocked : VehicleLockState.Locked;
                client.SendNotification($"Vous avez {(LockState == VehicleLockState.Locked ? " fermé" : "ouvert")} le véhicule");
                UpdateInBackground();
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

            UpdateInBackground();
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