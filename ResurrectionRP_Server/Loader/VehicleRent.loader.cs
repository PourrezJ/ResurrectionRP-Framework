
using Newtonsoft.Json;
using System.IO;
using AltV.Net;


namespace ResurrectionRP_Server.Loader
{
    public static class VehicleRentLoaders
    {
        private static string _basePath = $"vehicleRent{Path.DirectorySeparatorChar}";

        public static void LoadAllVehicleRent()
        {
            Alt.Server.LogColored("~grey~ ---- VehicleRent ~w~| Loading all vehicle rent ... ----");

            if (!Directory.Exists(MakePath())) Directory.CreateDirectory(MakePath());

            string[] files = Directory.GetFiles(MakePath(), "*.json");

            foreach (var file in files)
            {
                VehicleRentLoader.VehicleRentShop _cardealer = JsonConvert.DeserializeObject<VehicleRentLoader.VehicleRentShop>(File.ReadAllText(file));
                _cardealer.Name = Path.GetFileNameWithoutExtension(file);
                _cardealer.Load();
            }
            Alt.Server.LogColored("~grey~ ---- VehicleRent ~w~| All Rent Vehicle loaded ... ----");

        }

        private static string MakePath(string relativePath = "") => Path.GetFullPath(Path.Combine(_basePath, relativePath));
    }
}
