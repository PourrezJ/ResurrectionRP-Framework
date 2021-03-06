﻿using AltV.Net;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Loader
{
    public class CarParkModel
    {
        public int ID;
        public bool HasBlip = true;
        public Vector3 Borne;
        public Models.Location Spawn1;
        public Models.Location Spawn2;
    }

    class CarParkLoader
    {
        private static string _basePath = $"parkings{Path.DirectorySeparatorChar}";

        public static void LoadAllCarParks()
        {
            Alt.Server.LogColored("~w~----- ~b~ Parkings ~w~| Loading all parking ... ------");

            if (!Directory.Exists(MakePath()))
                Directory.CreateDirectory(MakePath());

            string[] files = Directory.GetFiles(MakePath(), "*.json");

            if (files.Length > 0)
            {
                foreach (var file in files)
                {
                    try
                    {
                        string _name = Path.GetFileNameWithoutExtension(file).Replace('_', ' ');
                        CarParkModel _carParkModel = JsonConvert.DeserializeObject<CarParkModel>(File.ReadAllText(file));
                        Services.CarPark carpark = null;

                        if (Services.CarPark.HasCarPark(_carParkModel.ID))
                            carpark = Services.CarPark.LoadCarPark(_carParkModel.ID, _carParkModel.Borne, _carParkModel.Spawn1, _carParkModel.Spawn2, _carParkModel.HasBlip);
                        else
                        {
                            carpark = Services.CarPark.CreateCarPark(_carParkModel.ID, _name, _carParkModel.Borne, _carParkModel.Spawn1, _carParkModel.Spawn2, _carParkModel.HasBlip);
                            Alt.Server.LogColored("~b~CarPark.Loader.cs ~w~| New carpark()");
                            //Alt.Server.LogInfo($"Car Park {_name} vient d'être ajouté!");
                        }
                        Models.Parking.ParkingList.Add(carpark.Parking);
                    }
                    catch (Exception ex)
                    {
                        Alt.Server.LogError(ex.Message);
                    }
                }
            }

            Alt.Server.LogColored($"~w~----- ~b~ Parkings ~w~| Parkings loaded, numbers: {files.Length} -------");
        }

        private static string MakePath(string relativePath = "") => Path.GetFullPath(Path.Combine(_basePath, relativePath));
    }
}
