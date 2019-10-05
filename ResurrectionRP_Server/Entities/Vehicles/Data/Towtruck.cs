using ResurrectionRP_Server.Utils;
using System.Numerics;
using MongoDB.Bson.Serialization.Attributes;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Vehicles;

namespace ResurrectionRP.Entities.Vehicles.Data
{
    public class TowTruck
    {
        public string VehPlate;
        public Vector3 Position;
        [BsonIgnore]
        public IVehicle Vehicle;
        [BsonIgnore]
        public VehicleHandler Master;
        [BsonIgnore]
        public System.Timers.Timer timer;

        public TowTruck(string plate, Vector3 position, IVehicle vehicle, VehicleHandler master)
        {
            VehPlate = plate;
            Position = position;
            Vehicle = vehicle;
            Master = master;
            timer = Utils.SetInterval(() =>
            {
                if(!master.HaveTowVehicle())
                {
                    this.timer.Stop();
                    return;
                }
                if (!vehicle.Exists)
                    return;
                AltV.Net.Data.Position pos = master.Vehicle.Position;
                if((pos.Y - vehicle.Position.Y) < pos.Y)
                {
                    vehicle.Position = new AltV.Net.Data.Position(pos.X, pos.Y +2, pos.Z + 4);
                }
                else
                {
                    vehicle.Position = new AltV.Net.Data.Position(pos.X, pos.Y - 2, pos.Z + 4);
                }
                vehicle.Rotation = master.Vehicle.Rotation;
            }, 500);
        }
    }
}
