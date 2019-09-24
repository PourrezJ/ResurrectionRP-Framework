using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Models;
using VehicleHandler = ResurrectionRP_Server.Entities.Vehicles.VehicleHandler;

namespace ResurrectionRP_Server.Services
{
    class CarPark
    {
        #region Variables
        [BsonId]
        public int ID;

        public Models.Parking Parking;

        #endregion

        #region Methods
        public async void Load()
        {
            if (Parking != null)
            {
                Parking.Load(alpha: 150, scale: 0.7f, name: "Parkings", blip: true);
                Parking.OnVehicleStored += OnVehicleStored;
                Parking.OnVehicleOut += OnVehicleOut;
                Parking.ParkingType = ParkingType.Public;
                Parking.OnSaveNeeded = async () => await Update();
                Parking.Spawn1.Rot = Parking.Spawn1.Rot.ConvertRotationToRadian();
                Parking.Spawn2.Rot = Parking.Spawn2.Rot.ConvertRotationToRadian();

                List<ParkedCar> _poundList = Parking.ListVehicleStored.FindAll(veh => DateTime.Now > veh.ParkTime.AddMonths(1));

                foreach(ParkedCar ve in _poundList.ToList())
                {
                    ve.ParkTime = DateTime.Now;
                    Parking.RemoveVehicle(Entities.Vehicles.VehiclesManager.GetVehicleHandler(ve.Plate));
                    GameMode.Instance?.PoundManager.AddVehicleInPound(Entities.Vehicles.VehiclesManager.GetVehicleHandler(ve.Plate) );
                }

                if (_poundList.Count != 0)
                    await Update();
            }
        }

        private async Task OnVehicleOut(IPlayer client, VehicleHandler vehicle, Models.Location Spawn)
        {
            //vehicle.Vehicle.Rotation = Spawn.Rot.ConvertRotationToRadian();
            await Update();
            client.SendNotificationSuccess($"Vous avez sorti votre {vehicle.VehicleManifest.LocalizedName}!"); 
        }

        private async Task OnVehicleStored(IPlayer client, VehicleHandler vehicle)
        {
            await Update();
            client.SendNotificationSuccess($"Vous avez rangé votre véhicule {vehicle.VehicleManifest.LocalizedName}");
        }

        public async Task Insert() => await Database.MongoDB.Insert("carparks", this);

        public async Task Update()
        {
            try
            {
                if (GameMode.Instance.IsDebug)
                    Alt.Server.LogColored("~b~Service CarPark ~w~| Saving parkings ()");

                await Database.MongoDB.Update(this, "carparks", ID);
            }
            catch (Exception ex)
            {
                Alt.Server.LogError($"Service.CarPark.cs | {ex.ToString()}");
            } 
        }
        #endregion

        #region Static methods
        public static async Task<CarPark> LoadCarPark(int id, Vector3 borne, Models.Location spawn1, Models.Location spawn2)
        {
            CarPark carpark = await Database.MongoDB.GetCollectionSafe<CarPark>("carparks").Find(p => p.ID == id).FirstAsync();
            carpark.ID = id;
            carpark.Parking.Location = borne;
            carpark.Parking.Spawn1 = spawn1;
            carpark.Parking.Spawn2 = spawn2;
            carpark.Load();
            return carpark;
        }

        public static async Task<bool> HasCarPark(int id) =>
            await Database.MongoDB.GetCollectionSafe<CarPark>("carparks").Find(p => p.ID == id).AnyAsync();

        public static async Task<CarPark> CreateCarPark(int ID, string name, Vector3 borne, Models.Location spawn1, Models.Location spawn2)
        {
            var _carpark = new CarPark();
            _carpark.ID = ID;
            _carpark.Parking = new Models.Parking(borne, spawn1, spawn2, name, maxVehicles: 2100, hidden: false);
            await _carpark.Insert();
            _carpark.Load();
            return _carpark;
        }
        #endregion
    }
}
