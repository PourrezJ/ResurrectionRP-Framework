using AltV.Net;
using AltV.Net.Elements.Entities;
using System;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    class VehicleHandlerFactory : IEntityFactory<IVehicle>
    {
        public IVehicle Create(IntPtr vehiclePointer, ushort id)
        {
            return new VehicleHandler(vehiclePointer, id);
        }
    }
}