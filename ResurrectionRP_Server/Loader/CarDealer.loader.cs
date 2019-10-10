using Newtonsoft.Json;
using System.IO;
using AltV.Net;
using CarDealer = ResurrectionRP_Server.Loader.CarDealerLoader.CarDealer;

namespace ResurrectionRP_Server.Loader
{
    public static class CarDealerLoaders
    {
        private static string _basePath = $"cardealer{Path.DirectorySeparatorChar}";

        public static void LoadAllCardealer()
        {
            Alt.Server.LogColored("~g~CarDealer ~w~| Loading all cardealer ...");

            if (!Directory.Exists(MakePath())) Directory.CreateDirectory(MakePath());

            string[] files = Directory.GetFiles(MakePath(), "*.json");
            foreach (var file in files)
            {
                CarDealer _cardealer = JsonConvert.DeserializeObject<CarDealer>(File.ReadAllText(file));
                _cardealer.Name = Path.GetFileNameWithoutExtension(file);
                _cardealer.Load();
            }

            Alt.Server.LogColored("~g~CarDealer ~w~| CarDealer loaded!");
        }

        private static string MakePath(string relativePath = "") => Path.GetFullPath(Path.Combine(_basePath, relativePath));
    }
}
