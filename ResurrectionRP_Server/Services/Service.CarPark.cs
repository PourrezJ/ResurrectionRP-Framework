using AltV.Net;
using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Entities.Vehicles;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Services
{
    class CarPark
    {
        #region Variables
        [BsonId]
        public int ID;

        public Models.Parking Parking;

        #endregion

        #region Init
        public void Init()
        {
            if (Parking != null)
            {
                Parking.Init(alpha: 150, scale: 0.7f, name: "Parking", blip: true);
                Parking.ParkingType = ParkingType.Public;
                Parking.Spawn1.Rot = Parking.Spawn1.Rot.ConvertRotationToRadian();
                Parking.Spawn2.Rot = Parking.Spawn2.Rot.ConvertRotationToRadian();
                Parking.OnSaveNeeded = async () => await Update();
                Parking.OnVehicleStored += OnVehicleStored;
                Parking.OnVehicleOut += OnVehicleOut;

                List<ParkedCar> poundList = Parking.ListVehicleStored.FindAll(veh => DateTime.Now > veh.ParkTime.AddMonths(1));

                foreach(ParkedCar ve in poundList)
                {
                    ve.ParkTime = DateTime.Now;
                    Parking.RemoveVehicle(ve.GetVehicleHandler());
                    Pound.AddVehicleInPound(ve.GetVehicleHandler());
                }

                if (poundList.Count != 0)
                    Task.Run(async ()=> await Update());
            }
        }
        #endregion

        #region Event handlers
        private void OnVehicleOut(IPlayer client, VehicleHandler vehicle, Models.Location Spawn)
        {
            Task.Run(async ()=>await Update());
            client.SendNotificationSuccess($"Vous avez sorti votre {vehicle.VehicleManifest.LocalizedName}!"); 
        }

        private void OnVehicleStored(IPlayer client, VehicleHandler vehicle)
        {
            Task.Run(async ()=> await Update());
            client.SendNotificationSuccess($"Vous avez rangé votre véhicule {vehicle.VehicleManifest.LocalizedName}");
        }
        #endregion

        #region Methods
        public async Task Insert() => await Database.MongoDB.Insert("carparks", this);

        public async Task Update()
        {
            try
            {
                if (GameMode.IsDebug)
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
        public static CarPark LoadCarPark(int id, Vector3 borne, Models.Location spawn1, Models.Location spawn2)
        {
            CarPark carpark = Database.MongoDB.GetCollectionSafe<CarPark>("carparks").Find(p => p.ID == id).First();
            carpark.ID = id;
            carpark.Parking.Location = borne;
            carpark.Parking.Spawn1 = spawn1;
            carpark.Parking.Spawn2 = spawn2;
            carpark.Init();
            return carpark;
        }

        public static bool HasCarPark(int id) =>
            Database.MongoDB.GetCollectionSafe<CarPark>("carparks").Find(p => p.ID == id).Any();

        public static CarPark CreateCarPark(int ID, string name, Vector3 borne, Models.Location spawn1, Models.Location spawn2)
        {
            CarPark carPark = new CarPark();
            carPark.ID = ID;
            carPark.Parking = new Models.Parking(borne, spawn1, spawn2, name, maxVehicles: 2100, hidden: false);
            Task.Run(async ()=> await carPark.Insert());
            carPark.Init();
            return carPark;
        }
        #endregion
    }
}
