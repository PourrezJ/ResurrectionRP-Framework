
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using VehicleInfoLoader.Data;
using Location = ResurrectionRP_Server.Models.Location;
using VehicleInfo = ResurrectionRP_Server.Loader.CarDealerLoader.VehicleInfo;

namespace ResurrectionRP_Server.Loader.VehicleRentLoader
{
    public class VehicleRentShop
    {
        [JsonIgnore]
        public string Name;
        [JsonIgnore]
        public List<VehicleRentPlace> VehicleRentPlaces = new List<VehicleRentPlace>();

        public Vector3 BlipPosition;
        public int BlipSprite;
        public List<Location> LocationList = new List<Location>();
        public List<VehicleInfo> VehicleInfoList = new List<VehicleInfo>();

        public async Task Load()
        {
            //await MP.Blips.NewAsync(BlipSprite, BlipPosition, 1, 0, $"[Location] {Name}", 255, 10, true);
            Entities.Blips.BlipsManager.CreateBlip($"[Location] {Name}", BlipPosition, 0, BlipSprite);


            for (int i = 0; i < LocationList.Count; i++)
            {
                var _vehrent = new VehicleRentPlace(i, LocationList[i], this);
                await Respawn(_vehrent);
                VehicleRentPlaces.Add(_vehrent);
            }

            // Boucle pour vérifié si place est vide.
            Utils.Utils.Delay(30000, false, async () =>
            {
                foreach (var place in VehicleRentPlaces)
                {
                    if (place.VehicleHandler == null && !Entities.Vehicles.VehiclesManager.IsVehicleInSpawn(place.Location.Pos, 4))
                        await Respawn(place);
                }
            });
        }

        public async Task Respawn(VehicleRentPlace place)
        {
            place.VehicleInfo = VehicleInfoList[Utils.Utils.RandomNumber(VehicleInfoList.Count)];
            place.VehicleInfo.Price = Convert.ToInt32(place.VehicleInfo.Price);
            Array colorArray = Enum.GetValues(typeof(Utils.Enums.VehicleColor));
            int color1 = (int)colorArray.GetValue(Utils.Utils.RandomNumber(colorArray.Length));
            int color2 = (int)colorArray.GetValue(Utils.Utils.RandomNumber(colorArray.Length));
            VehicleManifest manifest = VehicleInfoLoader.VehicleInfoLoader.Get((uint)place.VehicleInfo.VehicleHash);

            if (manifest != null)
            {
                place.VehicleHandler = await Entities.Vehicles.VehiclesManager.SpawnVehicle("", place.VehicleInfo.VehicleHash, place.Location.Pos, place.Location.Rot, color1, color2, spawnVeh: true, freeze: true, inventory: new Inventory.Inventory(place.VehicleInfo.InventoryWeight, 20));
                place.VehicleHandler?.Vehicle.SetData("RentShop", place);
                string str = $"{manifest.DisplayName} \n" +
                $"Prix $ {place.VehicleInfo.Price} \n" +
                $"Coffre: {place.VehicleInfo.InventoryWeight} \n" +
                $"Vitesse Max: {Math.Ceiling(manifest.MaxSpeed * (manifest.MaxSpeed / 20) * 2)} km/h \n" +
                $"Acceleration: {manifest.MaxAcceleration} \n" +
                $"Places: {manifest.MaxNumberOfPassengers + 1} \n";

                place.TextLabelId = GameMode.Instance.Streamer.AddEntityTextLabel(str, place.Location.Pos + new Vector3(0,0,1f),1, 180,255,255,255);
                    //await MP.TextLabels.NewAsync(place.Location.Pos + new Vector3(0, 0, 1.0f), str, 1, Color.FromArgb(180, 255, 255, 255), 15);
            }
        }

        public async Task RentCar(VehicleRentPlace vehicleplace, Entities.Players.PlayerHandler ph)
        {
            //await vehicleplace.TextLabel.DestroyAsync();
            GameMode.Instance.Streamer.DestroyEntityLabel(vehicleplace.TextLabelId);

            //GameMode.Instance.Economy.CaissePublique += Economy.CalculPriceTaxe(vehicleplace.VehicleInfo.Price, GameMode.Instance.Economy.Taxe_Market);

            var veh = vehicleplace.VehicleHandler;
            veh.Vehicle.SetSyncedMetaData("VehicleRent", DateTime.Now.ToString());
            vehicleplace.VehicleHandler.SpawnVeh = true;
            vehicleplace.VehicleHandler.SetOwner(ph);
            vehicleplace.VehicleHandler.FreezePosition = (false);
            vehicleplace.VehicleHandler.Vehicle.ResetData("RentShop");
            ph.ListVehicleKey.Add(Models.VehicleKey.GenerateVehicleKey(vehicleplace.VehicleHandler));
            await ph.Client.SendNotificationSuccess($"Vous avez loué un(e) {vehicleplace.VehicleHandler.VehicleManifest.DisplayName}");
            VehicleRentPlaces.Find(c => c.VehicleHandler == vehicleplace.VehicleHandler).VehicleHandler = null;

            Utils.Utils.Delay((int)TimeSpan.FromHours(2).TotalMilliseconds, true, async () =>
            {
                await veh.Delete();
            });
        }
    }

}
