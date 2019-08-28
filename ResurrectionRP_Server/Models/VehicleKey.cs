using VehicleInfoLoader.Data;

namespace ResurrectionRP_Server.Models
{
    public class VehicleKey
    {
        #region Public fields
        public string VehicleName;
        public string Plate;
        #endregion

        #region Constructor
        public VehicleKey(string vehicleName, string plate)
        {
            VehicleName = vehicleName;
            Plate = plate;
        }
        #endregion

        #region Static methods
        public static VehicleKey GenerateVehicleKey(Entities.Vehicles.VehicleHandler veh)
        {
            VehicleManifest manifest = VehicleInfoLoader.VehicleInfoLoader.Get(veh.Model);
            return new VehicleKey(manifest.DisplayName, veh.Plate);
        }
        #endregion
    }
}
