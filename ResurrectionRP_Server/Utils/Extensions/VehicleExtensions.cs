using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Vehicles;

namespace ResurrectionRP_Server.Utils.Extensions
{
    public static class VehicleExtensions
    {
        public static VehicleHandler GetVehicleHandler(this IVehicle vehicle)
        {
            return VehiclesManager.GetHandlerByVehicle(vehicle);
        }

    }
}
