using ResurrectionRP_Server.Utils;
using System.Numerics;
using MongoDB.Bson.Serialization.Attributes;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Vehicles;
using AltV.Net.Async;

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
            timer = Util.SetInterval(async () =>
            {
                if(!master.HaveTowVehicle())
                {
                    this.timer.Stop();
                    return;
                }
                if (!await vehicle.ExistsAsync())
                    return;

                AltV.Net.Data.Position pos = await master.GetPositionAsync();
                if((pos.Y - (await vehicle.GetPositionAsync()).Y) < pos.Y)
                {
                    await vehicle.SetPositionAsync(new AltV.Net.Data.Position(pos.X, pos.Y + 2, pos.Z + 4));
                }
                else
                {
                    await vehicle.SetPositionAsync(new AltV.Net.Data.Position(pos.X, pos.Y - 2, pos.Z + 4));
                }
                await vehicle.SetRotationAsync(await master.GetRotationAsync());
                
            }, 150);
        }
    }
}
