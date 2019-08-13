using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using System;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public class VehiclesManager
    {
        public VehiclesManager()
        {
            AltAsync.OnPlayerEnterVehicle += OnPlayerEnterVehicle;
        }

        private Task OnPlayerEnterVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            throw new NotImplementedException();
        }
    }
}
