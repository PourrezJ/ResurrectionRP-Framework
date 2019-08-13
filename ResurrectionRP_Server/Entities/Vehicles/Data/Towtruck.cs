using System.Numerics;

namespace ResurrectionRP.Entities.Vehicles.Data
{
    public class TowTruck
    {
        public string VehPlate;
        public Vector3 Position;

        public TowTruck(string plate, Vector3 position)
        {
            VehPlate = plate;
            Position = position;
        }
    }
}
