using AltV.Net.Async;
using Newtonsoft.Json;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Streamer.Data;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using VehicleInfoLoader.Data;

namespace ResurrectionRP_Server.Loader.CarDealerLoader
{
    public class CarDealer
    {

        [JsonIgnore]
        public string Name;
        [JsonIgnore]
        public List<CarDealerPlace> CarDealerPlaces = new List<CarDealerPlace>();

        public uint BlipSprite = 255;
        public Vector3 BlipPosition = new Vector3();
        public Models.LicenseType LicenseType = Models.LicenseType.Car;
        public List<Models.Location> LocationList = new List<Models.Location>();
        public List<VehicleInfo> VehicleInfoList = new List<VehicleInfo>();

        public void Load()
        {
            Entities.Blips.BlipsManager.CreateBlip(Name, BlipPosition, 0, (int)BlipSprite);

            for (int i = 0; i < LocationList.Count; i++)
            {
                var place = new CarDealerPlace(i, LocationList[i], this);
                Respawn(place);
                CarDealerPlaces.Add(place);
            }

            // Boucle pour vérifier si la place est vide.
            Utils.Util.SetInterval(async () =>
            {
                foreach (var place in CarDealerPlaces)
                {
                    await AltAsync.Do(() =>
                    {
                        if (place.VehicleHandler == null && !VehiclesManager.IsVehicleInSpawn(place.Location.Pos, 4))
                            Respawn(place);
                    });
                }
            }, 30000);
        }

        public void Respawn(CarDealerPlace place)
        {
            place.VehicleInfo = VehicleInfoList[Utils.Util.RandomNumber(VehicleInfoList.Count)];

            var pourcent = place.VehicleInfo.Price * 0.02;

            place.VehicleInfo.Price = Utils.Util.RandomNumber(Convert.ToInt32(place.VehicleInfo.Price - pourcent), Convert.ToInt32(place.VehicleInfo.Price + pourcent));
            Array colorArray = Enum.GetValues(typeof(Utils.Enums.VehicleColor));
            int color1 = (int)colorArray.GetValue(Utils.Util.RandomNumber(colorArray.Length));
            int color2 = (int)colorArray.GetValue(Utils.Util.RandomNumber(colorArray.Length));
            VehicleManifest manifest = VehicleInfoLoader.VehicleInfoLoader.Get((uint)place.VehicleInfo.VehicleHash);

            if (manifest != null)
            {
                place.VehicleHandler = VehiclesManager.SpawnVehicle("", (uint)place.VehicleInfo.VehicleHash, place.Location.Pos, place.Location.Rot, color1, color2, spawnVeh: true, freeze: true, inventory: new Inventory.Inventory(place.VehicleInfo.InventoryWeight, 20));

                if (place.VehicleHandler == null)
                    return;

                if (!place.VehicleHandler.Exists)
                    return;

                place.VehicleHandler.Freeze(true);
                place.VehicleHandler.Invincible(true);

                string str = $"{manifest.DisplayName} \n" +
                $"Prix Actuel $ {place.VehicleInfo.Price} \n" +
                $"Coffre: {place.VehicleInfo.InventoryWeight} \n" +
                $"Vitesse Max: {Math.Ceiling(manifest.MaxSpeed * (manifest.MaxSpeed / 20) * 2)} km/h \n" +
                $"Acceleration: {manifest.MaxAcceleration} \n" +
                $"Places: {manifest.MaxNumberOfPassengers + 1} \n";

                place.TextLabelId = TextLabel.CreateTextLabel(str, place.Location.Pos, System.Drawing.Color.White);

                place.VehicleHandler.SetData("CarDealer", place);
                place.VehicleHandler.SetData("CarDealerPrice", place.VehicleInfo.Price);
            }
        }

        public void BuyCar(CarDealerPlace vehicleplace, Entities.Players.PlayerHandler ph)
        {
            if (vehicleplace.TextLabelId != null)
                Task.Run(async ()=> vehicleplace.TextLabelId.Destroy());

            vehicleplace.VehicleHandler.SpawnVeh = false;
            vehicleplace.VehicleHandler.SetOwner(ph);
            vehicleplace.VehicleHandler.ResetData("CarDealer");
            ph.ListVehicleKey.Add(Models.VehicleKey.GenerateVehicleKey(vehicleplace.VehicleHandler));
            
            ph.Client.SendNotificationSuccess($"Vous avez acheté un(e) {vehicleplace.VehicleHandler.VehicleManifest.DisplayName}");
            vehicleplace.VehicleHandler.Freeze(false);
            vehicleplace.VehicleHandler.Invincible(false);
            vehicleplace.VehicleHandler.VehicleData.InsertVehicle();
            CarDealerPlaces.Find(c => c.VehicleHandler == vehicleplace.VehicleHandler).VehicleHandler = null;
        }
    }
}
