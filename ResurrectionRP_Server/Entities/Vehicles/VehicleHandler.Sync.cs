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

        #region Fields
        private DateTime _previousUpdate = DateTime.Now;
        private Vector3 _previousPosition;
        private float _milage = 0;
        private float _fuel = 100;
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
                    Engine = false;
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
        public uint RadioID { get; set; } = 255;
        public bool FreezePosition { get; set; }
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
        public ConcurrentDictionary<int, int> Mods { get; set; }
            = new ConcurrentDictionary<int, int>();

        public uint BodyHealth { get; set; }

        public int EngineHealth { get; set; }

        public int PetrolTankHealth
        {
            get;
            set;
        } = 1000;

        public Tuple<bool, bool, bool, bool> NeonState { get; set; } = new Tuple<bool, bool, bool, bool>(false, false, false, false);
        public Color NeonsColor { get; set; } = Color.Empty;

        public byte Dirt { get; set; }

        public bool Engine { get; set; }

        public byte PrimaryColor { get; set; }
        public byte SecondaryColor { get; set; }

        public byte PearlColor { get; internal set; }

        public WindowTint WindowTint { get; set; } = 0;
        //public bool ArmoredWindows { get; set; } = false;

        public VehicleBumperDamage FrontBumperDamage { get; set; } = 0;
        public VehicleBumperDamage RearBumperDamage { get; set; } = 0;

        public string AppearanceData { get; set; }
        public string DamageData { get; set; }


        public VehicleDoorState[] Doors { get; set; } = new VehicleDoorState[Globals.NB_VEHICLE_DOORS];
        public WindowState[] Windows { get; set; } = new WindowState[Globals.NB_VEHICLE_WINDOWS] { 0, 0, 0, 0 };

        public Wheel[] Wheels { get; set; }

        public Location LastKnowLocation;

        public Location Location
        {
            get
            {
                if (Vehicle != null && Vehicle.Exists)
                {
                    return new Location(Vehicle.Position, Vehicle.Rotation);
                }

                return LastKnowLocation;
            }
            set
            {
                if (Vehicle != null && Vehicle.Exists)
                {
                    Vehicle.Position = value.Pos;
                    Vehicle.Rotation = value.Rot;
                }
                
                LastKnowLocation = value;
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

        public void Freeze(bool statut)
        {
            FreezePosition = statut;
            UpdateProperties();
        }

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

            if (Vehicle.EngineOn)
            {
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
            }

            _previousUpdate = updateTime;
        }
    }
}
