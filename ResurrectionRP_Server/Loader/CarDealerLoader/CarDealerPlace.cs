using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Loader.CarDealerLoader
{
    public class CarDealerPlace
    {
        public int ID;
        public Models.Location Location;
        public int TextLabelId;
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
