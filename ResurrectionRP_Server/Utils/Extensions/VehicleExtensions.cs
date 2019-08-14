using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Vehicles;

namespace ResurrectionRP_Server.Utils.Extensions
{
    public static class VehicleExtensions
    {
        public static VehicleHandler GetVehicleHandler(this IVehicle vehicle)
        {

            if (vehicle == null)
                return null;

            if (!vehicle.Exists)
                return null;

            return null; // en attendant le vehicle manager.
            /*
            if (GameMode.Instance.VehicleManager.VehicleHandlerList.TryGetValue(vehicle, out VehicleHandler value))
            {
                return value;
            }
            else
            {
                return null;
            }*/
        }
    }
}
