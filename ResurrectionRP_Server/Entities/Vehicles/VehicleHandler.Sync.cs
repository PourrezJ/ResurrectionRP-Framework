﻿using AltV.Net.Async;
using AltV.Net.Enums;
using ResurrectionRP.Entities.Vehicles.Data;
using ResurrectionRP_Server.Entities.Vehicles.Data;
using ResurrectionRP_Server.Models;
using System.Drawing;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public partial class VehicleHandler
    {
        public TowTruck TowTruck { get; set; }
        public float Fuel { get; set; } = 100;
        public float FuelMax { get; set; } = 100;
        public bool NeonState { get; set; }
        public byte Dirt { get; set; } = 0;
        public bool Engine { get; set; } = false;
        public bool Siren { get; set; } = false;
        public bool SirenSound { get; set; } = false;
        public uint RadioID { get; set; } = 255;
        public uint BodyHealth { get; set; } = 1000;
        public int EngineHealth { get; set; } = 1000;
        public bool FreezePosition { get; set; }
        public float TorqueMultiplicator { get; set; }
        public float PowerMultiplicator { get; set; }
        public float Milage { get; set; }

        public Color NeonsColor { get; set; } = Color.Empty;
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
