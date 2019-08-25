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
namespace ResurrectionRP_Server.Entities.Vehicles
{
    public enum WindowID
    {
        WindowFrontRight,
        WindowFrontLeft,
        WindowRearRight,
        WindowRearLeft
    }

    public enum WindowState
    {
        WindowFixed,
        WindowDown,
        WindowBroken
    }

    public enum DoorID
    {
        DoorFrontLeft,
        DoorFrontRight,
        DoorRearLeft,
        DoorRearRight,
        DoorHood,
        DoorTrunk
    }

    public enum DoorState
    {
        DoorClosed,
        DoorOpen,
        DoorBroken,
    }

    public enum WheelID
    {
        Wheel0,
        Wheel1,
        Wheel2,
        Wheel3,
        Wheel4,
        Wheel5,
        Wheel6,
        Wheel7,
        Wheel8,
        Wheel9
    }

    public enum WheelState
    {
        WheelFixed,
        WheelBurst,
        WheelOnRim,
    }

    public partial class VehicleHandler
    {
        public TowTruck TowTruck { get; set; }
        public float Fuel { get; set; } = 100;
        public float FuelMax { get; set; } = 100;
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
        public int PetrolTankHealth { get; set; }

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


        public VehicleDoorState[] Door { get; set; } = new VehicleDoorState[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        public WindowState[] Window { get; set; } = new WindowState[4] { 0, 0, 0, 0 };
        public WheelState[] Wheel { get; set; } = new WheelState[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

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


        public VehicleDoorState GetDoorState(VehicleDoor door) => Door[(byte)door];

        public async Task SetDoorState(VehicleDoor door, VehicleDoorState state)
        {
            await Vehicle.SetDoorStateAsync(door, state);
            
            Door[(byte)door] = state;
            /*
            if (Vehicle != null && Vehicle.Exists)
            {
                var players = AltV.Net.Server.


                var players = await MP.Players.GetInRangeAsync(await Vehicle.GetPositionAsync(), Config.GetInt("stream-distance"));

                foreach (var player in players)
                {
                    if (!player.Exists)
                        continue;
                    await player.CallAsync("VehStream_SetDoorsStatus", Vehicle.Id, door, state);
                }
            }*/
        }
    }
}
