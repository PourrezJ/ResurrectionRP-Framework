using System;
using System.Collections.Generic;
using System.Text;
using VehicleInfoLoader.Data;

namespace ResurrectionRP_Server.Models
{
    public class VehicleKey
    {
        public string VehicleName;
        public string Plate;

        public VehicleKey(string vehicleName, string plate)
        {
            VehicleName = vehicleName;
            Plate = plate;
        }

        public static VehicleKey GenerateVehicleKey(Entities.Vehicles.VehicleHandler veh)
        {
            VehicleManifest manifest = VehicleInfoLoader.VehicleInfoLoader.Get(veh.Model);
            return new VehicleKey(manifest.DisplayName, veh.Plate);
        }
    }
}
