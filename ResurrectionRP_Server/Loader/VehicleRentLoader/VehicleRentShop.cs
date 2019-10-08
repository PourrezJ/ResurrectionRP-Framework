
using Newtonsoft.Json;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Loader.CarDealerLoader;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using VehicleInfoLoader.Data;

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
            //await MP.Blips.NewAsync(BlipSprite, BlipPosition, 1, 0, $"[Location] {Name}", 255, 10, true);
            Entities.Blips.BlipsManager.CreateBlip($"[Location] {Name}", BlipPosition, 0, BlipSprite);


            for (int i = 0; i < LocationList.Count; i++)
            {
                var _vehrent = new VehicleRentPlace(i, LocationList[i], this);
                Respawn(_vehrent);
                VehicleRentPlaces.Add(_vehrent);
            }

            // Boucle pour vérifié si place est vide.
            Utils.Utils.SetInterval(() =>
            {
                foreach (var place in VehicleRentPlaces)
                {
                    if (place.VehicleHandler == null && !VehiclesManager.IsVehicleInSpawn(place.Location.Pos, 4))
                        Respawn(place);
                }
            }, 30000);
        }

        public void Respawn(VehicleRentPlace place)
        {
            place.VehicleInfo = VehicleInfoList[Utils.Utils.RandomNumber(VehicleInfoList.Count)];
            place.VehicleInfo.Price = Convert.ToInt32(place.VehicleInfo.Price);
            Array colorArray = Enum.GetValues(typeof(Utils.Enums.VehicleColor));
            int color1 = (int)colorArray.GetValue(Utils.Utils.RandomNumber(colorArray.Length));
            int color2 = (int)colorArray.GetValue(Utils.Utils.RandomNumber(colorArray.Length));
            VehicleManifest manifest = VehicleInfoLoader.VehicleInfoLoader.Get((uint)place.VehicleInfo.VehicleHash);

            if (manifest != null)
            {
                place.VehicleHandler = VehiclesManager.SpawnVehicle("", place.VehicleInfo.VehicleHash, place.Location.Pos, place.Location.Rot.ConvertRotationToRadian(), color1, color2, spawnVeh: true, freeze: true, inventory: new Inventory.Inventory(place.VehicleInfo.InventoryWeight, 20));
                place.VehicleHandler.Vehicle.SetData("RentShop", place);
                place.VehicleHandler.Vehicle.Freeze(true);
                place.VehicleHandler.Vehicle.Invincible(true);
                string str = $"{manifest.DisplayName} \n" +
                $"Prix $ {place.VehicleInfo.Price} \n" +
                $"Coffre: {place.VehicleInfo.InventoryWeight} \n" +
                $"Vitesse Max: {Math.Ceiling(manifest.MaxSpeed * (manifest.MaxSpeed / 20) * 2)} km/h \n" +
                $"Acceleration: {manifest.MaxAcceleration} \n" +
                $"Places: {manifest.MaxNumberOfPassengers + 1} \n";

                place.TextLabelId = GameMode.Instance.Streamer.AddEntityTextLabel(str, place.Location.Pos + new Vector3(0,0,1f),1,255,255,255, 180);
            }
        }

        public void RentCar(VehicleRentPlace vehicleplace, Entities.Players.PlayerHandler ph)
        {
            vehicleplace.TextLabelId.Destroy();
            var veh = vehicleplace.VehicleHandler;
            veh.Vehicle.SetSyncedMetaData("VehicleRent", DateTime.Now.ToString());
            veh.Vehicle.Freeze(false);
            veh.Vehicle.Invincible(false);
            vehicleplace.VehicleHandler.SpawnVeh = true;
            vehicleplace.VehicleHandler.SetOwner(ph);
            vehicleplace.VehicleHandler.Vehicle.ResetData("RentShop");
            veh.LockState = AltV.Net.Enums.VehicleLockState.Unlocked;
            ph.ListVehicleKey.Add(VehicleKey.GenerateVehicleKey(vehicleplace.VehicleHandler));
            ph.Client.SendNotificationSuccess($"Vous avez loué un(e) {vehicleplace.VehicleHandler.VehicleManifest.DisplayName}");
            VehicleRentPlaces.Find(c => c.VehicleHandler == vehicleplace.VehicleHandler).VehicleHandler = null;

            Utils.Utils.SetInterval(() =>
            {
                if (!veh.Vehicle.Exists)
                    return;

                Task.Run(async ()=> await veh.DeleteAsync());
            }, (int)TimeSpan.FromHours(2).TotalMilliseconds);
        }
    }

}
