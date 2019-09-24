using AltV.Net.Async;
using Newtonsoft.Json;
using ResurrectionRP_Server.Entities.Vehicles;
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

        public async Task Load()
        {
            Entities.Blips.BlipsManager.CreateBlip(Name, BlipPosition, 0, (int)BlipSprite);

            for (int i = 0; i < LocationList.Count; i++)
            {
                var place = new CarDealerPlace(i, LocationList[i], this);
                await Respawn(place);
                CarDealerPlaces.Add(place);
            }

            // Boucle pour vérifier si la place est vide.
            Utils.Utils.Delay(30000, false, async () =>
            {
                foreach (var place in CarDealerPlaces)
                {
                    if (place.VehicleHandler == null && !VehiclesManager.IsVehicleInSpawn(place.Location.Pos, 4))
                        await Respawn(place);
                }
            });
        }

        public async Task Respawn(CarDealerPlace place)
        {
            place.VehicleInfo = VehicleInfoList[Utils.Utils.RandomNumber(VehicleInfoList.Count)];

            var pourcent = place.VehicleInfo.Price * 0.02;

            place.VehicleInfo.Price = Utils.Utils.RandomNumber(Convert.ToInt32(place.VehicleInfo.Price - pourcent), Convert.ToInt32(place.VehicleInfo.Price + pourcent));
            Array colorArray = Enum.GetValues(typeof(Utils.Enums.VehicleColor));
            int color1 = (int)colorArray.GetValue(Utils.Utils.RandomNumber(colorArray.Length));
            int color2 = (int)colorArray.GetValue(Utils.Utils.RandomNumber(colorArray.Length));
            VehicleManifest manifest = VehicleInfoLoader.VehicleInfoLoader.Get((uint)place.VehicleInfo.VehicleHash);

            if (manifest != null)
            {
                place.VehicleHandler = await VehiclesManager.SpawnVehicle("", (uint)place.VehicleInfo.VehicleHash, place.Location.Pos, place.Location.Rot, color1, color2, spawnVeh: true, freeze: true, inventory: new Inventory.Inventory(place.VehicleInfo.InventoryWeight, 20));

                if (place.VehicleHandler == null)
                    return;

                if (!await place.VehicleHandler.Vehicle.ExistsAsync())
                    return;

                await place.VehicleHandler.Vehicle.FreezeAsync(true);
                await place.VehicleHandler.Vehicle.InvincibleAsync(true);

                string str = $"{manifest.DisplayName} \n" +
                $"Prix $ {place.VehicleInfo.Price} \n" +
                $"Coffre: {place.VehicleInfo.InventoryWeight} \n" +
                $"Vitesse Max: {Math.Ceiling(manifest.MaxSpeed * (manifest.MaxSpeed / 20) * 2)} km/h \n" +
                $"Acceleration: {manifest.MaxAcceleration} \n" +
                $"Places: {manifest.MaxNumberOfPassengers + 1} \n";

                place.TextLabelId = GameMode.Instance.Streamer.AddEntityTextLabel(str, place.Location.Pos, 1, 255, 255, 255, 180);

                place.VehicleHandler.Vehicle.SetData("CarDealer", place);
                place.VehicleHandler.Vehicle.SetData("CarDealerPrice", place.VehicleInfo.Price);
            }
        }

        public async Task BuyCar(CarDealerPlace vehicleplace, Entities.Players.PlayerHandler ph)
        {
            if (vehicleplace.TextLabelId != null)
                vehicleplace.TextLabelId.Destroy();

            vehicleplace.VehicleHandler.SpawnVeh = false;
            vehicleplace.VehicleHandler.SetOwner(ph);
            vehicleplace.VehicleHandler.Vehicle.ResetData("CarDealer");
            ph.ListVehicleKey.Add(Models.VehicleKey.GenerateVehicleKey(vehicleplace.VehicleHandler));
            await vehicleplace.VehicleHandler.InsertVehicle();
            ph.Client.SendNotificationSuccess($"Vous avez acheté un(e) {vehicleplace.VehicleHandler.VehicleManifest.DisplayName}");
            await vehicleplace.VehicleHandler.Vehicle.FreezeAsync(false);
            await vehicleplace.VehicleHandler.Vehicle.InvincibleAsync(false);
            CarDealerPlaces.Find(c => c.VehicleHandler == vehicleplace.VehicleHandler).VehicleHandler = null;
        }
    }
}
