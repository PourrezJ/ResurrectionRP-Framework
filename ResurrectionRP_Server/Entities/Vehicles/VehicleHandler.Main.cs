using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using ResurrectionRP_Server.Database;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Tasks;
using VehicleInfoLoader;
using VehicleInfoLoader.Data;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public partial class VehicleHandler : Vehicle
    {
        #region Fields

        public IPlayer Owner { get; private set; }

        private VehicleManifest _vehicleManifest;
        public VehicleManifest VehicleManifest
        {
            get
            {
                if (_vehicleManifest == null)
                    _vehicleManifest = VehicleInfoLoader.VehicleInfoLoader.Get(Model);
                return _vehicleManifest;
            }
            set => _vehicleManifest = value;
        }

        public bool SpawnVeh { get; set; }
       
        public bool WasTeleported { get; set; } = false;

        public VehicleData VehicleData { get; set; }

        public bool HasTrailer = false;

        public IEntity Trailer;


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
            IPlayer owner = null, ConcurrentDictionary<byte, byte> mods = null, int[] neon = null, bool spawnVeh = false, short dimension = GameMode.GlobalDimension, Inventory.Inventory inventory = null, bool freeze = false, byte dirt = 0, float health = 1000) : base (model, position, rotation)
        {
            if (model == 0)
                return;

            Dimension = dimension;
            SpawnVeh = spawnVeh;
            Owner = owner;

            VehicleData = new VehicleData(this)
            {
                OwnerID = socialClubName,
                Model = model,
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor,
                Plate = string.IsNullOrEmpty(plate) ? VehiclesManager.GenerateRandomPlate() : plate,
                LockState = locked ? VehicleLockState.Locked : VehicleLockState.Unlocked,
                Mods = mods,   
                Location = new Location(position, rotation),
                Inventory = inventory,
                //OilTank = new OilTank()
            };
        }
        #endregion

        #region Methods
        public bool IsLocked()
        {
            return (LockState == VehicleLockState.Locked) ? true : false;
        }
        /*

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
            if (NeonColor != null && NeonColor != Color.Empty)
                Vehicle.NeonColor = NeonColor;
            Vehicle.SetNeonActive(NeonState.Item1, NeonState.Item2, NeonState.Item3, NeonState.Item4);
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
        }*/

        public async Task<bool> DeleteAsync(bool perm = false)
        {
            if (!Exists)
                return false;

            await AltAsync.Do(() =>
            {
                if (Exists)
                    Remove();
            });
            /*
            if (VehiclesManager.VehicleHandlerList.TryRemove(this, out _))
            {
                if (perm && !SpawnVeh)
                {
                    _cancelUpdate = true;

                    if (!await RemoveInDatabase())
                        return false;
               
                /*
                if (perm || SpawnVeh)
                    VehiclesManager.DeleteVehicleHandler(this);
                    
                return true;
            }*/

            if (perm && !SpawnVeh)
            {
                _cancelUpdate = true;

                if (!await RemoveInDatabase())
                    return false;
            }
            /*
            if (perm || SpawnVeh)
                VehiclesManager.DeleteVehicleHandler(this);
                */
            return true;
        }

        public void ApplyDamage()
        {
            if (Exists)
            {
                Dimension = -1;
                Dimension = GameMode.GlobalDimension;
            }
        }

        public bool LockUnlock(IPlayer client)
        {
            if (!client.Exists)
                return false;

            if (!Exists)
                return false;

            if (client.HasVehicleKey(NumberplateText) || SpawnVeh && VehicleData.OwnerID == client.GetSocialClub())
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
            if (VehicleData.Fuel + fuel > VehicleData.FuelMax)
                VehicleData.Fuel = VehicleData.FuelMax;
            else
                VehicleData.Fuel += fuel;

            UpdateInBackground();
        }

        public void Repair(IPlayer player)
        {
            if (!Exists || player == null || !player.Exists)
                return;

            BodyHealth = 1000;
            VehicleData.Doors = new VehicleDoorState[Globals.NB_VEHICLE_DOORS] { 0, 0, 0, 0, 0, 0, 0, 0 };
            VehicleData.Windows = new WindowState[Globals.NB_VEHICLE_WINDOWS] { 0, 0, 0, 0 };
            VehicleData.Wheels = new Wheel[WheelsCount];

            for (int i = 0; i < VehicleData.Wheels.Length; i++)
                VehicleData.Wheels[i] = new Wheel();

            VehicleData.FrontBumperDamage = 0;
            VehicleData.RearBumperDamage = 0;
            DamageData = string.Empty;
            player.EmitLocked("vehicleFix", this);
        }

        public void SetOwner(IPlayer player) => VehicleData.OwnerID = player.GetSocialClub();

        public void SetOwner(PlayerHandler player) => VehicleData.OwnerID = player.Client.GetSocialClub();
        #endregion
    }
}