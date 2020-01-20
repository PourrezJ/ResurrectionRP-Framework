using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Streamer.Data;

namespace ResurrectionRP_Server.Loader.VehicleRentLoader
{
    public class VehicleRentPlace
    {
        public int ID;
        public Models.Location Location;
        public TextLabel TextLabelId;
        public Entities.Vehicles.VehicleHandler VehicleHandler;
        public CarDealerLoader.VehicleInfo VehicleInfo;
        public VehicleRentShop RentShop;

        public VehicleRentPlace(int id, Models.Location location, VehicleRentShop rentShop)
        {
            ID = id;
            Location = location;
            RentShop = rentShop;
        }
    }
}
