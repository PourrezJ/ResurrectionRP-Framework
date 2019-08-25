using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Loader.VehicleRentLoader
{
    public class VehicleRentPlace
    {
        public int ID;
        public Models.Location Location;
        public int TextLabelId;
        public Entities.Vehicles.VehicleHandler VehicleHandler;
        public Loader.CarDealerLoader.VehicleInfo VehicleInfo;
        public VehicleRentShop RentShop;

        public VehicleRentPlace(int id, Models.Location location, VehicleRentShop rentShop)
        {
            ID = id;
            Location = location;
            RentShop = rentShop;
        }
    }
}
