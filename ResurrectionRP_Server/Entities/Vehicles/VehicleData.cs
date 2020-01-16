using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using ResurrectionRP.Entities.Vehicles.Data;
using ResurrectionRP_Server.Entities.Vehicles.Data;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public class VehicleData
    {
        #region Constants
        const double FUEL_FACTOR = 3.75;
        #endregion

        #region Private fields
        private DateTime _previousUpdate = DateTime.Now;
        private Vector3 _previousPosition;
        private float _milage = 0;
        private float _fuel = 100;
        private uint _bodyhealth;
        private int _engineHealth = 1000;
        private int _petrolTankHealth = 1000;
        private byte _primaryColor = 0;
        private byte _secondaryColor = 0;
        private byte _pearlColor = 0;
        private bool _engineOn = false;
        private byte _dirtLevel = 0;
        private uint _radioStation = 255;
        private VehicleLockState _lockState = VehicleLockState.Locked;
        private Tuple<bool, bool, bool, bool> _neonState = new Tuple<bool, bool, bool, bool>(false, false, false, false);
        private Color _neonColor = Color.Empty;
        private WindowTint _windowTint = WindowTint.None;
        private VehicleBumperDamage _frontBumperDamage = VehicleBumperDamage.NotDamaged;
        private VehicleBumperDamage _rearBumperDamage = VehicleBumperDamage.NotDamaged;
        private string _damageData = string.Empty;
        private VehicleDoorState[] _doors = new VehicleDoorState[Globals.NB_VEHICLE_DOORS] { 0, 0, 0, 0, 0, 0, 0, 0 };
        private WindowState[] _windows = new WindowState[Globals.NB_VEHICLE_WINDOWS] { 0, 0, 0, 0 };
        private Wheel[] _wheels; // Number of wheels is defined at runtime so no default initialization possible
        #endregion

        [BsonId]
        public string Plate;

        [BsonRepresentation(BsonType.Int32, AllowOverflow = true)]
        public uint Model;

        public string OwnerID { get; set; } // SocialClubName

        public bool IsParked;

        public bool IsInPound;

        public DateTime LastUse = DateTime.Now;

        public string LastDriver;

        public bool PlateHide = false;

        public string ParkingName = string.Empty;

        public float FuelMax { get; set; } = 100;

        public float FuelConsumption { get; set; } = 5.5f;

        public Inventory.Inventory Inventory { get; set; }

        [BsonIgnore]
        public VehicleHandler Vehicle;

        public float Fuel
        {
            get => _fuel;

            set
            {
                float oldFuel = _fuel;

                if (value < 0)
                    _fuel = 0;
                else
                    _fuel = value;

                if (_fuel == 0)
                {
                    EngineOn = false;
                    Vehicle?.UpdateInBackground(false);
                }

                if (Math.Ceiling(oldFuel * 10) != Math.Ceiling(_fuel * 10) && Vehicle != null && Vehicle.Driver != null && Vehicle.Driver.Exists)
                    Vehicle.Driver.EmitLocked("UpdateFuel", _fuel);
            }
        }

        public bool EngineOn
        {
            get => _engineOn;

            set
            {
                if (!value || Fuel > 0)
                {
                    _engineOn = value;

                    if (Vehicle != null && Vehicle.Exists)
                        Vehicle.EngineOn = value;
                }
            }
        }

        public byte PrimaryColor
        {
            get => _primaryColor;

            set
            {
                _primaryColor = value;

                if (Vehicle != null && Vehicle.Exists && Vehicle.PrimaryColor != value)
                    AltAsync.Do(() => { Vehicle.PrimaryColor = value; });
            }
        }

        public byte SecondaryColor
        {
            get => _secondaryColor;

            set
            {
                _secondaryColor = value;

                if (Vehicle != null && Vehicle.Exists && Vehicle.SecondaryColor != value)
                    AltAsync.Do(() => { Vehicle.SecondaryColor = value; });
            }
        }

        public byte PearlColor
        {
            get => _pearlColor;

            set
            {
                _pearlColor = value;

                if (Vehicle != null && Vehicle.Exists && Vehicle.PearlColor != value)
                    AltAsync.Do(() => { Vehicle.PearlColor = value; });
            }
        }

        public WindowTint WindowTint
        {
            get => _windowTint;

            set
            {
                _windowTint = value;

                if (Vehicle != null && Vehicle.Exists && Vehicle.WindowTint != (byte)value)
                    AltAsync.Do(() => { Vehicle.SetWindowTint(value); });
            }
        }

        public VehicleBumperDamage FrontBumperDamage
        {
            get => _frontBumperDamage;

            set
            {
                _frontBumperDamage = value;

                if (Vehicle != null && Vehicle.Exists && Vehicle.GetBumperDamageLevel(VehicleBumper.Front) != value)
                    AltAsync.Do(() => { Vehicle.SetBumperDamageLevel(VehicleBumper.Front, value); });
            }
        }

        public VehicleBumperDamage RearBumperDamage
        {
            get => _rearBumperDamage;

            set
            {
                _rearBumperDamage = value;

                if (Vehicle != null && Vehicle.Exists && Vehicle.GetBumperDamageLevel(VehicleBumper.Rear) != value)
                    AltAsync.Do(() => { Vehicle.SetBumperDamageLevel(VehicleBumper.Rear, value); });
            }
        }

        // public string AppearanceData { get; set; }
        public string DamageData
        {
            get => _damageData;

            set
            {
                _damageData = value;

                // BUG v792: DamageData makes server crash
                // if (Vehicle != null && Vehicle.Exists && Vehicle.DamageData != value)
                //     AltAsync.Do(() => { Vehicle.DamageData = value; });
            }
        }

        public VehicleDoorState[] Doors
        {
            get => _doors;

            set
            {
                _doors = value;

                AltAsync.Do(() =>
                {
                    if (Vehicle != null && Vehicle.Exists)
                    {
                        for (byte i = 0; i < Globals.NB_VEHICLE_DOORS; i++)
                        {
                            if (Vehicle.GetDoorState(i) != (byte)_doors[i])
                                Vehicle.SetDoorState(i, (byte)_doors[i]);
                        }
                    }
                });
            }
        }

        public WindowState[] Windows
        {
            get => _windows;

            set
            {
                _windows = value;

                if (Vehicle != null && Vehicle.Exists)
                {
                    AltAsync.Do(() =>
                    {
                        for (byte i = 0; i < Globals.NB_VEHICLE_WINDOWS; i++)
                        {
                            if (_windows[i] == WindowState.WindowBroken)
                                Vehicle.SetWindowDamaged(i, true);
                            else if (_windows[i] == WindowState.WindowDown)
                                Vehicle.SetWindowOpened(i, true);
                            else
                                Vehicle.SetWindowOpened(i, false);
                        }
                    });
                }
            }
        }

        public Wheel[] Wheels
        {
            get => _wheels;

            set
            {
                _wheels = value;

                if (Vehicle != null && Vehicle.Exists)
                {
                    AltAsync.Do(() =>
                    {
                        for (byte i = 0; i < Vehicle.WheelsCount; i++)
                        {
                            Vehicle.SetWheelHealth(i, _wheels[i].Health);
                            Vehicle.SetWheelBurst(i, _wheels[i].Burst);
                        }
                    });
                }
            }
        }

        public Location LastKnowLocation { get; set; }

        public Location Location
        {
            get => new Location(LastKnowLocation.Pos, LastKnowLocation.Rot);

            set
            {
                LastKnowLocation = new Location(value.Pos, value.Rot);

                if (Vehicle != null && Vehicle.Exists)
                {
                    AltAsync.Do(() =>
                    {
                        Vehicle.Position = LastKnowLocation.Pos;
                        Vehicle.Rotation = LastKnowLocation.Rot;
                    });
                }
            }
        }

        public Tuple<bool, bool, bool, bool> NeonState
        {
            get => _neonState;

            set
            {
                _neonState = value;

                if (Vehicle != null && Vehicle.Exists)
                {
                    // BUG v792 : Vehicle.SetNeonActive isn't working correctly
                    Vehicle.GetSyncedMetaData("NeonState", out bool neonState);

                    if (neonState != value.Item1)
                    {
                        // AltAsync.Do(() => { Vehicle.SetNeonActive(value.Item1, value.Item2, value.Item3, value.Item4); });
                        AltAsync.Do(() => { Vehicle.SetSyncedMetaData("NeonState", value.Item1); });
                    }
                }
            }
        }

        public Color NeonColor
        {
            get => Color.FromArgb(_neonColor.A, _neonColor.R, _neonColor.G, _neonColor.B);

            set
            {
                _neonColor = value;

                if (Vehicle != null && Vehicle.Exists)
                {
                    // BUG v792 : Vehicle.NeonColor isn't working correctly
                    Vehicle.GetSyncedMetaData("NeonColor", out int neonColor);

                    if (neonColor != _neonColor.ToArgb())
                    {
                        // AltAsync.Do(() => { Vehicle.NeonColor = new Rgba(value.R, value.G, value.B, value.A); });
                        AltAsync.Do(() => { Vehicle.SetSyncedMetaData("NeonColor", value.ToArgb()); });
                    }
                }
            }
        }

        public byte DirtLevel
        {
            get => _dirtLevel;

            set
            {
                _dirtLevel = value;

                if (Vehicle != null && Vehicle.Exists && Vehicle.DirtLevel != value)
                    AltAsync.Do(() => { Vehicle.DirtLevel = value; });
            }
        }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public ConcurrentDictionary<byte, byte> Mods { get; set; } = new ConcurrentDictionary<byte, byte>();

        public uint BodyHealth
        {
            get => _bodyhealth;

            set
            {
                _bodyhealth = value;

                if (Vehicle != null && Vehicle.Exists && Vehicle.BodyHealth != value)
                    AltAsync.Do(() => { Vehicle.BodyHealth = value; });
            }
        }

        public int EngineHealth
        {
            get => _engineHealth;

            set
            {
                _engineHealth = value;

                if (Vehicle != null && Vehicle.Exists && Vehicle.EngineHealth != value)
                    AltAsync.Do(() => { Vehicle.EngineHealth = value; });
            }
        }

        public int PetrolTankHealth
        {
            get => _petrolTankHealth;

            set
            {
                _petrolTankHealth = value;

                if (Vehicle != null && Vehicle.Exists && Vehicle.PetrolTankHealth != value)
                    AltAsync.Do(() => { Vehicle.PetrolTankHealth = value; });
            }
        }

        [BsonIgnoreIfNull]
        public OilTank OilTank = null;
        [BsonIgnoreIfNull]
        public Attachment Attachment { get; set; }
        [BsonIgnoreIfNull]
        public TowTruck TowTruck { get; set; }

        public bool Siren { get; set; } = false;
        public bool SirenSound { get; set; } = false;

        public uint RadioStation
        {
            get => _radioStation;

            set
            {
                _radioStation = value;

                if (Vehicle != null && Vehicle.Exists && Vehicle.RadioStation != value)
                    AltAsync.Do(() => { Vehicle.RadioStation = value; });
            }
        }

        public VehicleLockState LockState
        {
            get => _lockState;

            set
            {
                _lockState = value;

                if (Vehicle != null && Vehicle.Exists && Vehicle.LockState != value)
                    AltAsync.Do(() => { Vehicle.LockState = value; });
            }
        }

        private float _torqueMultiplicator;
        public float TorqueMultiplicator
        {
            get => _torqueMultiplicator;
            set
            {
                AltAsync.Do(() => Vehicle?.SetSyncedMetaData("torqueMultiplicator", value));
                _torqueMultiplicator = value;
            }
        }
        private float _powerMultiplicator;
        public float PowerMultiplicator
        {
            get => _powerMultiplicator;
            set
            {
                AltAsync.Do(() => Vehicle?.SetSyncedMetaData("powerMultiplicator", value));
                _powerMultiplicator = value;
            }
        }

        public float Milage
        {
            get => _milage;

            set
            {
                float oldMilage = _milage;
                _milage = value;

                if (Math.Floor(oldMilage * 10) != Math.Floor(_milage * 10) && Vehicle != null && Vehicle.Driver != null && Vehicle.Driver.Exists)
                    Vehicle.Driver.EmitLocked("UpdateMilage", _milage);
            }
        }

        private Handling _customHandling;
        public Handling CustomHandling
        {
            get => _customHandling;
            set
            {
                if (Vehicle != null)
                    Vehicle.SetSyncedMetaData("CustomHandling", Newtonsoft.Json.JsonConvert.SerializeObject(value));
                _customHandling = value;
            }
        }

        public VehicleData(VehicleHandler vehicleHandler)
        {
            Vehicle = vehicleHandler;
        }

        public void UpdateProperties()
        {
            Utils.Utils.CheckThread("UpdateProperties");

            if (Vehicle == null)
                return;

            if (!Vehicle.Exists)
                return;

            _radioStation = Vehicle.RadioStation;
            _lockState = Vehicle.LockState;
            _bodyhealth = Vehicle.BodyHealth;
            _engineHealth = Vehicle.EngineHealth;
            _petrolTankHealth = Vehicle.PetrolTankHealth;

            bool neonActive = Vehicle.IsNeonActive;
            NeonState = new Tuple<bool, bool, bool, bool>(neonActive, neonActive, neonActive, neonActive);
            NeonColor = Vehicle.NeonColor;

            _dirtLevel = Vehicle.DirtLevel;
            _engineOn = Vehicle.EngineOn;
            _primaryColor = Vehicle.PrimaryColor;
            _secondaryColor = Vehicle.SecondaryColor;
            _pearlColor = Vehicle.PearlColor;
            _windowTint = Vehicle.GetWindowTint();
            _frontBumperDamage = Vehicle.GetBumperDamageLevel(VehicleBumper.Front);
            _rearBumperDamage = Vehicle.GetBumperDamageLevel(VehicleBumper.Rear);
            _damageData = Vehicle.DamageData;

            for (byte i = 0; i < Globals.NB_VEHICLE_DOORS; i++)
                _doors[i] = (VehicleDoorState)Vehicle.GetDoorState(i);

            for (byte i = 0; i < Globals.NB_VEHICLE_WINDOWS; i++)
            {
                if (Vehicle.IsWindowDamaged(i))
                    _windows[i] = WindowState.WindowBroken;
                else if (Vehicle.IsWindowOpened(i))
                    _windows[i] = WindowState.WindowDown;
                else
                    _windows[i] = WindowState.WindowFixed;
            }

            if (Vehicle.WheelsCount > _wheels.Length)
                _wheels = new Wheel[Vehicle.WheelsCount];

            for (byte i = 0; i < Vehicle.WheelsCount; i++)
            {
                _wheels[i] = new Wheel();
                _wheels[i].Health = Vehicle.GetWheelHealth(i);
                _wheels[i].Burst = Vehicle.IsWheelBurst(i);
            }

            VehicleModType[] values = (VehicleModType[])Enum.GetValues(typeof(VehicleModType));

            foreach (VehicleModType vehicleModType in values)
            {
                if (Vehicle.GetMod(vehicleModType) > 0)
                    Mods[(byte)vehicleModType] = Vehicle.GetMod(vehicleModType);
            }

            LastKnowLocation = new Location(Vehicle.Position, Vehicle.Rotation);
        }

        public void UpdateMilageAndFuel()
        {
            if (Vehicle.WasTeleported)
            {
                _previousPosition = Vehicle.Position.ConvertToVector3();
                Vehicle.WasTeleported = false;
                return;
            }

            DateTime updateTime = DateTime.Now;
            Vector3 oldPos = _previousPosition;
            Vector3 newPos = Vehicle.Position;
            double distance = 0;
            double speed = 0;

            if (newPos != oldPos)
            {
                float deltaX = newPos.X - oldPos.X;
                float deltaY = newPos.Y - oldPos.Y;
                float deltaZ = newPos.Z - oldPos.Z;
                distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ) / 1000;
                if (distance >= 300)
                    return;
                Milage += (float)distance;
                _previousPosition = newPos;
                speed = distance * 3600000 / (updateTime - _previousUpdate).TotalMilliseconds;
            }

            if (speed == 0)
                Fuel -= FuelConsumption / 10000;
            else
            {
                double speedFuel;

                if (speed < 80)
                    speedFuel = (2 * FUEL_FACTOR) - speed * FUEL_FACTOR / 80;
                else
                    speedFuel = speed / 80 * FUEL_FACTOR;

                Fuel -= (float)(FuelConsumption * distance * speedFuel / 100);
            }

            _previousUpdate = updateTime;
        }

        public async Task<bool> DeleteAsync(bool perm = false)
        {
            await AltAsync.Do(() =>
            {
                if (Vehicle != null && Vehicle.Exists)
                    Vehicle.Remove();
            });

            if (perm && !Vehicle.SpawnVeh)
            {
                //_cancelUpdate = true;

                if (!await Vehicle.RemoveInDatabase())
                    return false;
            }
            /*
            if (perm || Vehicle.SpawnVeh)
                VehiclesManager.DeleteVehicleHandler(this);
                */
            return true;
        }

        public IVehicle SpawnVehicle(Location location = null, bool setLastUse = true)
        {
            Utils.Utils.CheckThread("SpawnVehicle");

            //Dimension = GameMode.GlobalDimension;
            try
            {
                if (location != null)
                    Location = location;
                if (Vehicle == null)
                    Vehicle = new VehicleHandler(Model, Location.Pos, Location.GetRotation());
                Vehicle.VehicleData = this;
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
            Vehicle.ManualEngineControl = true;

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
            //Vehicle.Dimension = Dimension;
            Vehicle.VehicleManifest = VehicleInfoLoader.VehicleInfoLoader.Get(Model);
            // Needed as vehicles in database don't have this value
            if ((Vehicle.VehicleManifest.fuelConsum <= 0 || Vehicle.VehicleManifest.fuelReservoir <= 0) && Vehicle.VehicleManifest.VehicleClass != 13)
            {
                Alt.Server.LogError("Erreur sur le chargement d'un véhicule, le fuel réservoir ou la consommation existe pas : " + Vehicle.Model);
                FuelConsumption = 5.5f;
                FuelMax = 70;
            }
            if (FuelMax == 100)
            {
                FuelConsumption = Vehicle.VehicleManifest.fuelConsum;
                FuelMax = Vehicle.VehicleManifest.fuelReservoir;
            }
            if (Fuel > FuelMax)
                Fuel = FuelMax;
            //VehiclesManager.VehicleHandlerList.TryAdd(Vehicle, this);
            /*
            if (HaveTowVehicle())
            {
                IVehicle _vehtowed = VehiclesManager.GetVehicleWithPlate(TowTruck.VehPlate);
                if (_vehtowed != null)
                    Task.Run(async () => { await TowVehicle(_vehtowed); });
            }*/

            if (CustomHandling != null)
                Vehicle.SetSyncedMetaData("CustomHandling", Newtonsoft.Json.JsonConvert.SerializeObject(CustomHandling));

            ParkingName = string.Empty;
            IsInPound = false;
            IsParked = false;
            Vehicle.OnVehicleSpawned();
            return Vehicle;
        }

        public void InsertVehicle()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (Vehicle != null && Vehicle.SpawnVeh)
                        return;

                    await Database.MongoDB.Insert("vehicles", this);
                }
                catch (BsonException be)
                {
                    Alt.Server.LogError(be.Message);
                }
            });
        }
    }
}
