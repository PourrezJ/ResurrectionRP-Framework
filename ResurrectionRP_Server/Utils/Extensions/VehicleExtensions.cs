using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Vehicles;
using System.Numerics;

namespace ResurrectionRP_Server
{
    public static class VehicleExtensions
    {
        public static VehicleHandler GetVehicleHandler(this IVehicle vehicle)
        {
            return VehiclesManager.GetHandlerByVehicle(vehicle);
        }
        public static Vector3 GetVehicleVector(this IVehicle vehicle)
        {
            var pos = vehicle.GetPosition();
            return new Vector3(pos.X, pos.Y, pos.Z);
        }

    }
}
