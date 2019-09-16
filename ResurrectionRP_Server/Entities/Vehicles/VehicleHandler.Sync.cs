using AltV.Net.Async;
using AltV.Net.Enums;
using ResurrectionRP.Entities.Vehicles.Data;
using ResurrectionRP_Server.Entities.Vehicles.Data;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using VehicleInfoLoader.Data;
using AltV.Net.Elements.Entities;

namespace ResurrectionRP_Server.Entities.Vehicles
{

    public partial class VehicleHandler
    {
        public TowTruck TowTruck { get; set; }
        public float Fuel { get; set; } = 100;
        public float FuelMax { get; set; } = 100;
        public float FuelConsumption { get; set; } = 5.5f;
        public bool Siren { get; set; } = false;
        public bool SirenSound { get; set; } = false;
        public uint RadioID { get; set; } = 255;
        public bool FreezePosition { get; set; }
        public float TorqueMultiplicator { get; set; }
        public float PowerMultiplicator { get; set; }
        public float Milage { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public ConcurrentDictionary<int, int> Mods { get; set; }
= new ConcurrentDictionary<int, int>();

        public uint BodyHealth { get; set; } = 1000;
        public int EngineHealth { get; set; } = 1000;
        public int PetrolTankHealth { get; set; } = 1000;

        public Tuple<bool, bool, bool, bool> NeonState { get; set; } = new Tuple<bool, bool, bool, bool>(false, false, false, false);
        public Color NeonsColor { get; set; } = Color.Empty;

        public byte Dirt { get; set; } = 0;
        public bool Engine { get; set; } = false;


        public byte PrimaryColor { get; set; } = 0;
        public byte SecondaryColor { get; set; } = 0;

        public byte WindowTint { get; set; } = 0;
        public bool ArmoredWindows { get; set; } = false;

        public byte SirenActive { get; set; } = 0;

        public byte FrontBumperDamage { get; set; } = 0;
        public byte RearBumperDamage { get; set; } = 0;

        public VehicleDoorState[] Door { get; set; } = new VehicleDoorState[7];
        public WindowState[] Window { get; set; } = new WindowState[4] { 0, 0, 0, 0 };

        public WheelsStruct Wheel { get; set; } = new WheelsStruct();

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

        public VehicleDoorState GetDoorState(VehicleDoor door) =>  (VehicleDoorState)Door[(byte)door]; 
        public async Task SetDoorState(VehicleDoor door, VehicleDoorState state)
        {
            Door[(byte)door] = (VehicleDoorState)state;
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
            temp.GetVehicleHandler()?.Update();

            return temp;
        }
    }
}
