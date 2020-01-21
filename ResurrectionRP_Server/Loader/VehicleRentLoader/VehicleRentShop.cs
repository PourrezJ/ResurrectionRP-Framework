
using Newtonsoft.Json;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Loader.CarDealerLoader;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using VehicleInfoLoader.Data;
using ResurrectionRP_Server.Streamer.Data;
using ResurrectionRP_Server.Entities;

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

        public void Load()
        {
            Entities.Blips.BlipsManager.CreateBlip($"[Location] {Name}", BlipPosition, 0, BlipSprite);


            for (int i = 0; i < LocationList.Count; i++)
            {
                var _vehrent = new VehicleRentPlace(i, LocationList[i], this);
                Respawn(_vehrent);
                VehicleRentPlaces.Add(_vehrent);
            }

            // Boucle pour vérifié si place est vide.
            Utils.Util.SetInterval(() =>
            {
                foreach (var place in VehicleRentPlaces)
                {
                    AltV.Net.Async.AltAsync.Do(() =>
                    {
                        if (place.VehicleHandler == null && !VehiclesManager.IsVehicleInSpawn(place.Location.Pos, 4))
                            Respawn(place);
                    });
                }
            }, 30000);
        }

        public void Respawn(VehicleRentPlace place)
        {
            place.VehicleInfo = VehicleInfoList[Utils.Util.RandomNumber(VehicleInfoList.Count)];
            place.VehicleInfo.Price = Convert.ToInt32(place.VehicleInfo.Price);
            Array colorArray = Enum.GetValues(typeof(Utils.Enums.VehicleColor));
            int color1 = (int)colorArray.GetValue(Utils.Util.RandomNumber(colorArray.Length));
            int color2 = (int)colorArray.GetValue(Utils.Util.RandomNumber(colorArray.Length));
            VehicleManifest manifest = VehicleInfoLoader.VehicleInfoLoader.Get((uint)place.VehicleInfo.VehicleHash);

            if (manifest != null)
            {
                place.VehicleHandler = VehiclesManager.SpawnVehicle("", place.VehicleInfo.VehicleHash, place.Location.Pos, place.Location.Rot.ConvertRotationToRadian(), color1, color2, spawnVeh: true, freeze: true, inventory: new Inventory.Inventory(place.VehicleInfo.InventoryWeight, 20));
                place.VehicleHandler.SetData("RentShop", place);
                place.VehicleHandler.Freeze(true);
                place.VehicleHandler.Invincible(true);
                string str = $"{manifest.DisplayName} \n" +
                $"Prix $ {place.VehicleInfo.Price} \n" +
                $"Coffre: {place.VehicleInfo.InventoryWeight} \n" +
                $"Vitesse Max: {Math.Ceiling(manifest.MaxSpeed * (manifest.MaxSpeed / 20) * 2)} km/h \n" +
                $"Acceleration: {manifest.MaxAcceleration} \n" +
                $"Places: {manifest.MaxNumberOfPassengers + 1} \n";

                place.TextLabelId = TextLabel.CreateTextLabel(str, place.Location.Pos + new Vector3(0,0,1f), System.Drawing.Color.White);
            }
        }

        public void RentCar(VehicleRentPlace vehicleplace, Entities.Players.PlayerHandler ph)
        {
            Task.Run(async ()=> await vehicleplace.TextLabelId.Destroy());
            var veh = vehicleplace.VehicleHandler;
            veh.SetSyncedMetaData("VehicleRent", DateTime.Now.ToString());
            veh.Freeze(false);
            veh.Invincible(false);
            vehicleplace.VehicleHandler.SpawnVeh = true;
            vehicleplace.VehicleHandler.SetOwner(ph);
            vehicleplace.VehicleHandler.ResetData("RentShop");
            //veh.LockState = AltV.Net.Enums.VehicleLockState.Unlocked;
            ph.ListVehicleKey.Add(VehicleKey.GenerateVehicleKey(vehicleplace.VehicleHandler));
            ph.Client.SendNotificationSuccess($"Vous avez loué un(e) {vehicleplace.VehicleHandler.VehicleManifest.DisplayName}");

            if (ph.FirstSpawn)
            {
                ph.Client.SendNotificationTutorial("Pour déverrouillé votre véhicule, visez le et appuyer sur la touche U.");
            }

            VehicleRentPlaces.Find(c => c.VehicleHandler == vehicleplace.VehicleHandler).VehicleHandler = null;

            Utils.Util.SetInterval(() =>
            {
                if (!veh.Exists)
                    return;

                Task.Run(async ()=> await veh.DeleteAsync());
            }, (int)TimeSpan.FromHours(2).TotalMilliseconds);
        }
    }

}
