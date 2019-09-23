using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
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
        private byte _primaryColor;
        private byte _secondaryColor;
        private bool _engineOn = false;
        private byte _dirt = 0;
        private uint _radioStation = 255;
        private Tuple<bool, bool, bool, bool> _neonState = new Tuple<bool, bool, bool, bool>(false, false, false, false);
        private Color _neonColor = Color.Empty;
        private WindowTint _windowTint = WindowTint.None;
        private VehicleBumperDamage _frontBumperDamage = VehicleBumperDamage.NotDamaged;
        private VehicleBumperDamage _rearBumperDamage = VehicleBumperDamage.NotDamaged;
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
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                    _radioStation = Vehicle.RadioStation;

                return _radioStation;
            }

            set
            {
                _radioStation = value;

                if (Vehicle != null && Vehicle.Exists)
                    Vehicle.RadioStation = value;
            }
        }

        public float TorqueMultiplicator { get; set; }
        public float PowerMultiplicator { get; set; }

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

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public ConcurrentDictionary<byte, byte> Mods { get; set; } = new ConcurrentDictionary<byte, byte>();

        public uint BodyHealth
        {
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                    _bodyhealth = Vehicle.BodyHealth;

                return _bodyhealth;
            }

            set
            {
                _bodyhealth = value;

                if (Vehicle != null && Vehicle.Exists)
                    Vehicle.BodyHealth = value;
            }
        }

        public int EngineHealth
        {
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                    _engineHealth = Vehicle.EngineHealth;

                return _engineHealth;
            }

            set
            {
                _engineHealth = value;

                if (Vehicle != null && Vehicle.Exists)
                    Vehicle.EngineHealth = value;
            }
        }

        public int PetrolTankHealth
        {
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                    _petrolTankHealth = Vehicle.PetrolTankHealth;

                return _petrolTankHealth;
            }

            set
            {
                _petrolTankHealth = value;

                if (Vehicle != null && Vehicle.Exists)
                    Vehicle.PetrolTankHealth = value;
            }
        }

        public Tuple<bool, bool, bool, bool> NeonState
        {
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                {
                    bool neonActive = Vehicle.IsNeonActive;
                    _neonState = new Tuple<bool, bool, bool, bool>(neonActive, neonActive, neonActive, neonActive);
                }

                return _neonState;
            }

            set
            {
                _neonState = value;

                if (Vehicle != null && Vehicle.Exists)
                    Vehicle.SetNeonActive(_neonState.Item1, _neonState.Item2, _neonState.Item3, _neonState.Item4);
            }
        }

        public Color NeonColor
        {
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                    _neonColor = Vehicle.NeonColor;

                return _neonColor;
            }

            set
            {
                _neonColor = value;

                if (Vehicle != null && Vehicle.Exists)
                    Vehicle.NeonColor = value;
            }
        }

        public byte Dirt
        {
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                    _dirt = Vehicle.DirtLevel;

                return _dirt;
            }
            set
            {
                _dirt = value;

                if (Vehicle != null && Vehicle.Exists)
                    Vehicle.DirtLevel = value;
            }
        }

        public bool EngineOn
        {
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                    _engineOn = Vehicle.EngineOn;

                return _engineOn;
            }

            set
            {
                _engineOn = value;

                if (Vehicle != null && Vehicle.Exists)
                    Vehicle.EngineOn = value;
            }
        }

        public byte PrimaryColor
        {
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                    _primaryColor = Vehicle.PrimaryColor;

                return _primaryColor;
            }

            set
            {
                _primaryColor = value;

                if (Vehicle != null && Vehicle.Exists)
                    Vehicle.PrimaryColor = value;
            }
        }

        public byte SecondaryColor
        {
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                    _secondaryColor = Vehicle.SecondaryColor;

                return _secondaryColor;
            }

            set
            {
                _secondaryColor = value;

                if (Vehicle != null && Vehicle.Exists)
                    Vehicle.SecondaryColor = value;
            }
        }

        public byte PearlColor { get; internal set; }

        public WindowTint WindowTint
        {
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                    _windowTint = Vehicle.GetWindowTint();

                return _windowTint;
            }

            set
            {
                _windowTint = value;

                if (Vehicle != null && Vehicle.Exists)
                    Vehicle.SetWindowTint(value);
            }
        }

        //public bool ArmoredWindows { get; set; } = false;

        public VehicleBumperDamage FrontBumperDamage
        {
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                    _frontBumperDamage = Vehicle.GetBumperDamageLevel(VehicleBumper.Front);

                return _frontBumperDamage;
            }

            set
            {
                _frontBumperDamage = value;

                if (Vehicle != null && Vehicle.Exists)
                    Vehicle.SetBumperDamageLevel(VehicleBumper.Front, value);
            }
        }

        public VehicleBumperDamage RearBumperDamage
        {
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                    _rearBumperDamage = Vehicle.GetBumperDamageLevel(VehicleBumper.Rear);

                return _rearBumperDamage;
            }

            set
            {
                _rearBumperDamage = value;

                if (Vehicle != null && Vehicle.Exists)
                    Vehicle.SetBumperDamageLevel(VehicleBumper.Rear, value);
            }
        }

        // public string AppearanceData { get; set; }
        public string DamageData { get; set; }

        public VehicleDoorState[] Doors
        {
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                {
                    for (byte i = 0; i < Globals.NB_VEHICLE_DOORS; i++)
                        _doors[i] = (VehicleDoorState)Vehicle.GetDoorState(i);
                }

                return _doors;
            }

            set
            {
                _doors = value;

                if (Vehicle != null && Vehicle.Exists)
                {
                    for (byte i = 0; i < Globals.NB_VEHICLE_DOORS; i++)
                        Vehicle.SetDoorState(i, (byte)_doors[i]);
                }
            }
        }

        public WindowState[] Windows
        {
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                {
                    for (byte i = 0; i < Globals.NB_VEHICLE_WINDOWS; i++)
                    {
                        if (Vehicle.IsWindowDamaged(i))
                            _windows[i] = WindowState.WindowBroken;
                        else if (Vehicle.IsWindowOpened(i))
                            _windows[i] = WindowState.WindowDown;
                        else
                            _windows[i] = WindowState.WindowFixed;
                    }
                }

                return _windows;
            }

            set
            {
                _windows = value;

                if (Vehicle != null && Vehicle.Exists)
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
                }
            }
        }

        public Wheel[] Wheels
        {
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                {
                    _wheels = new Wheel[Vehicle.WheelsCount];

                    for (byte i = 0; i < Vehicle.WheelsCount; i++)
                    {
                        _wheels[i] = new Wheel();
                        _wheels[i].Health = Vehicle.GetWheelHealth(i);
                        _wheels[i].Burst = Vehicle.IsWheelBurst(i);
                    }
                }

                return _wheels;
            }

            set
            {
                _wheels = value;

                if (Vehicle != null && Vehicle.Exists)
                {
                    for (byte i = 0; i < Vehicle.WheelsCount; i++)
                    {
                        Vehicle.SetWheelHealth(i, _wheels[i].Health);
                        Vehicle.SetWheelBurst(i, _wheels[i].Burst);
                    }
                }
            }
        }

        public Location LastKnowLocation { get; set; }

        public Location Location
        {
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                    LastKnowLocation = new Location(Vehicle.Position, Vehicle.Rotation);

                return LastKnowLocation;
            }

            set
            {
                LastKnowLocation = value;

                if (Vehicle != null && Vehicle.Exists)
                {
                    Vehicle.Position = value.Pos;
                    Vehicle.Rotation = value.Rot;
                }
            }
        }

        public Attachment Attachment { get; set; }
        #endregion

        public VehicleDoorState GetDoorState(VehicleDoor door) => Doors[(byte)door];

        public async Task SetDoorState(VehicleDoor door, VehicleDoorState state)
        {
            Doors[(byte)door] = state;
            await Vehicle.SetDoorStateAsync(door, state);
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

        public void UpdateMilageAndFuel()
        {
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
                double oldDistance = distance;
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
    }
}
