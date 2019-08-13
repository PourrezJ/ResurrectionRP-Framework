using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP.Entities.Vehicles.Data;
using ResurrectionRP_Server.Models;
using System.Drawing;

namespace ResurrectionRP_Server.Entities.Vehicles.Data
{
    //Enums for ease of use
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

    [BsonIgnoreExtraElements]
    public class VehicleSync
    {
        [BsonIgnore]
        public IVehicle Vehicle;
        public TowTruck TowTruck { get; set; }
        public float Fuel { get; set; } = 100;
        public float FuelMax { get; set; } = 100;
        public bool NeonState { get; set; }
        public float Dirt { get; set; } = 0.0f;
        public bool Engine { get; set; } = false;
        public bool Siren { get; set; } = false;
        public bool SirenSound { get; set; } = false;
        public int RadioID { get; set; } = 255;
        public float BodyHealth { get; set; } = 1000f;
        public float EngineHealth { get; set; } = 1000f;
        public bool FreezePosition { get; set; }
        public float TorqueMultiplicator { get; set; }
        public float PowerMultiplicator { get; set; }
        public float Milage { get; set; }

        public Color NeonsColor { get; set; }
        public int[] Door { get; set; } = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        public int[] Window { get; set; } = new int[4] { 0, 0, 0, 0 };
        public int[] Wheel { get; set; } = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

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
    }
}
