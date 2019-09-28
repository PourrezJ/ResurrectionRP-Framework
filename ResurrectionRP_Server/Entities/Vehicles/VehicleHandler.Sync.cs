using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net.Data;
using ResurrectionRP.Entities.Vehicles.Data;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using ResurrectionRP_Server.Entities.Vehicles.Data;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public partial class VehicleHandler
    {
        #region constants
        const double FUEL_FACTOR = 5;
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

        #region Public fields and properties
        public TowTruck TowTruck { get; set; }
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

                if (_fuel == 0 && Vehicle != null && Vehicle.Exists)
                {
                    Vehicle.SetEngineOnAsync(false);
                    EngineOn = false;
                    UpdateFull();
                }

                if (Math.Floor(oldFuel * 10) != Math.Floor(_fuel * 10) && Vehicle != null && Vehicle.Driver != null && Vehicle.Driver.Exists)
                    Vehicle.Driver.EmitLocked("UpdateFuel", _fuel);
            }
        }
        public float FuelMax { get; set; } = 100;
        public float FuelConsumption { get; set; } = 5.5f;
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

        [BsonIgnoreIfNull]
        public OilTank OilTank = null;

        [BsonIgnore]
        public bool hasTrailer = false;

        [BsonIgnore]
        public IEntity Trailer;

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

        public bool EngineOn
        {
            get => _engineOn;

            set
            {
                _engineOn = value;

                if (Vehicle != null && Vehicle.Exists && Vehicle.EngineOn != value)
                    AltAsync.Do(() => { Vehicle.EngineOn = value; });
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

        //public bool ArmoredWindows { get; set; } = false;

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
                
                if (Vehicle != null && Vehicle.Exists)
                {
                    AltAsync.Do(() =>
                    {
                        for (byte i = 0; i < Globals.NB_VEHICLE_DOORS; i++)
                        {
                            if (Vehicle.GetDoorState(i) != (byte)_doors[i])
                                Vehicle.SetDoorState(i, (byte)_doors[i]);
                        }
                    });
                }
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
            get => new Location(new Vector3(LastKnowLocation.Pos.X, LastKnowLocation.Pos.Y, LastKnowLocation.Pos.Z), new Vector3(LastKnowLocation.Rot.X, LastKnowLocation.Rot.Y, LastKnowLocation.Rot.Z));

            set
            {
                LastKnowLocation = new Location(new Vector3(value.Pos.X, value.Pos.Y, value.Pos.Z), new Vector3(value.Rot.X, value.Rot.Y, value.Rot.Z));

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

        public Attachment Attachment { get; set; }
        #endregion

        #region Methods
        public VehicleDoorState GetDoorState(VehicleDoor door) => Doors[(byte)door];

        public void SetDoorState(IPlayer player, VehicleDoor door, VehicleDoorState state)
        {
            Doors[(byte)door] = state;

            if (Vehicle == null || !Vehicle.Exists)
                return;

            Vehicle.SetDoorStateFix(player, door, state, false);
            //Vehicle.SetDoorState(door, state);
        }

        public bool HaveTowVehicle() => TowTruck != null;

        public async Task TowVehicle(IVehicle vehicle)
        {
            if (await Vehicle.GetModelAsync() != (int)VehicleModel.Flatbed && !HaveTowVehicle())
                return;

            TowTruck = new TowTruck(vehicle.NumberplateText, new Vector3(0, -2, 1));
#pragma warning disable CS4014 // Dans la mesure où cet appel n'est pas attendu, l'exécution de la méthode actuelle continue avant la fin de l'appel
/*            Task.Run(async () =>
            {
                await UpdateAsync();
                while (HaveTowVehicle())
                {
                    if (!vehicle.Exists)
                        return;
                    await Task.Delay(500);
                    if (HaveTowVehicle())
                    {
                        AltV.Net.Data.Position pos = await Vehicle.GetPositionAsync();
                        await vehicle.SetPositionAsync( new AltV.Net.Data.Position(pos.X, pos.Y + 2, pos.Z + 3));
                        await vehicle.SetRotationAsync(await Vehicle.GetRotationAsync());
                    }

                }
            }); TODO VERIFIER POSITION VOITURE A L'ARRIERE ? NEST PAS CORRECT SPAWN EN DESSOUS DU VEH*/
#pragma warning restore CS4014 // Dans la mesure où cet appel n'est pas attendu, l'exécution de la méthode actuelle continue avant la fin de l'appel
        }

        public async Task<IVehicle> UnTowVehicle(Location position)
        {
            if (Vehicle.Model != (int)VehicleModel.Flatbed && TowTruck == null) return null;

            IVehicle temp = VehiclesManager.GetVehicleWithPlate(TowTruck.VehPlate);
            TowTruck = null;
            UpdateProperties();

            await temp.SetPositionAsync(position.Pos);
            await temp.SetRotationAsync(position.Rot);
            temp.GetVehicleHandler()?.UpdateFull();

            return temp;
        }

        public void SetNeonState(bool state)
        {
            NeonState = new Tuple<bool, bool, bool, bool>(state, state, state, state);
        }

        public void UpdateMilageAndFuel()
        {
            if (WasTeleported)
            {
                _previousPosition = Vehicle.Position;
                WasTeleported = false;
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

                if (Vehicle.Driver != null)
                    Vehicle.Driver.EmitLocked("UpdateFuel", Fuel);
            }

            _previousUpdate = updateTime;
        }
        #endregion
    }
}
