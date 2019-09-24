using ResurrectionRP_Server.Streamer.Data;

namespace ResurrectionRP_Server.Loader.CarDealerLoader
{
    public class CarDealerPlace
    {
        public int ID;
        public Models.Location Location;
        public TextLabel TextLabelId;
        public Entities.Vehicles.VehicleHandler VehicleHandler;
        public VehicleInfo VehicleInfo;
        public CarDealer CarDealer;

        public CarDealerPlace(int id, Models.Location location, CarDealer carDealer)
        {
            ID = id;
            Location = location;
            CarDealer = carDealer;
        }
    }
}
