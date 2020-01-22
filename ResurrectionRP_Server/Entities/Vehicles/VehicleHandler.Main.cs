using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles.Data;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using VehicleInfoLoader.Data;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public partial class VehicleHandler : Vehicle, IVehicleHandler
    {
        #region Fields
        private static HashSet<VehicleHandler> _vehicleOnWorld = new HashSet<VehicleHandler>();

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

        private Handling _vehicleHandling;
        public Handling VehicleHandling
        {
            get
            {
                if (_vehicleHandling == null)
                    _vehicleHandling = HandlingManager.Get(Model);
                return _vehicleHandling;
            }
            set => _vehicleHandling = value;
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
            IPlayer owner = null, ConcurrentDictionary<byte, byte> mods = null, int[] neon = null, bool spawnVeh = false, short dimension = GameMode.GlobalDimension, Inventory.Inventory inventory = null, bool freeze = false, byte dirt = 0, float health = 1000) : base(model, position, rotation)
        {
            if (model == 0)
                return;

            Dimension = dimension;
            SpawnVeh = spawnVeh;
            Owner = owner;

            VehicleData = new VehicleData(this)
            {
                Vehicle = this,
                OwnerID = socialClubName,
                Model = model,
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor,
                Plate = string.IsNullOrEmpty(plate) ? VehiclesManager.GenerateRandomPlate() : plate,
                LockState = locked ? VehicleLockState.Locked : VehicleLockState.Unlocked,
                Mods = (mods != null) ? mods : new ConcurrentDictionary<byte, byte>(),
                Location = new Location(position, rotation),
                Inventory = inventory,
                //OilTank = new OilTank()
            };    

            VehicleData.SpawnVehicle();
        }
        #endregion

        #region Methods
        public bool IsLocked()
        {
            return (LockState == VehicleLockState.Locked) ? true : false;
        }

        public async Task<bool> DeleteAsync(bool perm = false)
        {
            if (!Exists)
                return false;

            await AltAsync.Do(() =>
            {
                if (Exists)
                    Remove();
            });

            if (perm && !SpawnVeh)
            {
                _cancelUpdate = true;

                if (!await RemoveInDatabase())
                    return false;
            }

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

                if (LockState == VehicleLockState.Unlocked)
                {
                    var receverList = this.GetPlayersInRange(5f);
                    lock (receverList)
                    {
                        foreach (IPlayer recever in receverList)
                        {
                            if (!recever.Exists)
                                continue;

                            recever.PlaySoundFromEntity(this, 0, "5_SEC_WARNING", "HUD_MINI_GAME_SOUNDSET");
                        }
                    }
                }
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
            /*
            var players = player.GetNearestPlayers(GameMode.StreamDistance, false);

            lock (players)
            {
                foreach (var client in players)
                    if (client.Exists)
                        client.EmitLocked("vehicleFix", this);
            }
            */
            BodyHealth = 1000;
            VehicleData.Doors = new VehicleDoorState[Globals.NB_VEHICLE_DOORS] { 0, 0, 0, 0, 0, 0, 0, 0 };
            VehicleData.Windows = new WindowState[Globals.NB_VEHICLE_WINDOWS] { 0, 0, 0, 0 };
           // VehicleData.Wheels = new Wheel[WheelsCount];

            for (byte i = 0; i < WheelsCount; i++)
            {
                VehicleData.Wheels[i] = new Wheel();
                SetWheelBurst(i, false);
                SetWheelHasTire(i, true);
                SetWheelHealth(i, 100);
                SetWheelOnFire(i, false);
            }

            for (byte i = 0; i < 6; i++)
                SetLightDamaged(i, false);

            for (byte i = 0; i < 5; i++)
                SetPartDamageLevel(i, 0);

            for (byte i = 0; i < VehicleData.Doors.Length; i++)
                SetDoorState(i, (byte)VehicleDoorState.Closed);

            for (byte i = 0; i < VehicleData.Windows.Length; i++)
                SetWindowDamaged(i, false);

            VehicleData.FrontBumperDamage = 0;
            VehicleData.RearBumperDamage = 0;
            
        }

        public void SetOwner(IPlayer player) => VehicleData.OwnerID = player.GetSocialClub();

        public void SetOwner(PlayerHandler player) => VehicleData.OwnerID = player.Client.GetSocialClub();

        public void OnVehicleSpawned()
        {
            lock (_vehicleOnWorld)
            {
                if (!_vehicleOnWorld.Contains(this))
                {
                    _vehicleOnWorld.Add(this);
                }
            }
        }

        public override void OnRemove()
        {
            lock (_vehicleOnWorld)
            {
                if (_vehicleOnWorld.Contains(this))
                {
                    _vehicleOnWorld.Remove(this);
                }
                Exists = false; // doesn't work
                VehicleData.Vehicle = null;
            }
            base.OnRemove();
        }

        public static ICollection<VehicleHandler> GetAllWorldVehicle() => _vehicleOnWorld;
        #endregion
    }
}