using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.NetworkingEntity;
using MongoDB.Driver;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public class VehiclesManager
    {
        #region Fields
        private ConcurrentDictionary<IVehicle, VehicleHandler> _VehicleHandlerList = new ConcurrentDictionary<IVehicle, VehicleHandler>();
        public ConcurrentDictionary<IVehicle, VehicleHandler> VehicleHandlerList
        {
            get
            {
                if (_VehicleHandlerList == null) _VehicleHandlerList = new ConcurrentDictionary<IVehicle, VehicleHandler>();
                return _VehicleHandlerList;
            }
            set => _VehicleHandlerList = value;
        }
        #endregion

        #region Ctor
        public VehiclesManager()
        {
            AltAsync.OnPlayerEnterVehicle += OnPlayerEnterVehicle;
            AltAsync.OnPlayerLeaveVehicle += OnPlayerLeaveVehicle;

            AltAsync.OnClient("LockUnlockVehicle", LockUnlockVehicle);
            AltAsync.OnClient("OpenXtremVehicle", OpenXtremVehicle);
            AltAsync.OnClient("updateFuelAndMilage", updateFuelAndMilage);
        }
        #endregion

        #region Server Events
        private Task updateFuelAndMilage(IPlayer client, object[] args)
        {
            var veh = (IVehicle)args[0];
            float fuel = float.Parse(args[1].ToString());
            float mile = float.Parse(args[2].ToString());
            var vehh = VehiclesManager.GetVehicleHandler(veh);
            vehh.Milage = mile;
            vehh.SetFuel(fuel);
            return Task.CompletedTask;
        }
        private async Task OnPlayerEnterVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            PlayerHandler ph = player.GetPlayerHandler();
            VehicleHandler vh = vehicle.GetVehicleHandler();
            if (ph != null)
            {
                await ph.Update();
                await player.EmitAsync("OnPlayerEnterVehicle", vehicle.Id, Convert.ToInt32(seat), vh.Fuel, vh.FuelMax, vh.Milage );
            }
        }

        public static Task OpenXtremVehicle(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return Task.CompletedTask;

            return client.GetNearestVehicleHandler()?.OpenXtremMenu(client);
        }

        private async Task LockUnlockVehicle(IPlayer player, object[] args)
        {
            if (GameMode.Instance.IsDebug)
                Alt.Server.LogColored("~b~VehicleManager ~w~| " + player.GetSocialClub() + " is trying to lock/unlock a car");

            var vehicle = args[0] as IVehicle;

            if (!vehicle.Exists)
                return;

            VehicleHandler veh = vehicle.GetVehicleHandler();

            if (veh == null)
                return;
            
            if (await veh.LockUnlock(player))
            {
                var receverList =  vehicle.GetPlayersInRange(5f);

                foreach (IPlayer recever in receverList)
                {
                    if (!recever.Exists)
                        continue;

                    await recever.PlaySoundFromEntity(veh.Vehicle, 0, "5_SEC_WARNING", "HUD_MINI_GAME_SOUNDSET");
                }

                veh.Update();
            }
        }

        private async Task OnPlayerLeaveVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            VehicleHandler vh = vehicle.GetVehicleHandler();
            PlayerHandler ph = player.GetPlayerHandler();

            if (vh != null)
                vh.Update();

            if (ph != null)
            {
                await ph.Update();
                await player.EmitAsync("OnPlayerLeaveVehicle", vehicle);
            }
        }
        #endregion

        #region Database
        public async Task LoadAllVehiclesActive()
        {
            Alt.Server.LogInfo("--- Start loading all vehicle in database ---");
            var vehicleList = await Database.MongoDB.GetCollectionSafe<VehicleHandler>("vehicles").AsQueryable().ToListAsync();

            if (GameMode.Instance.ModeAutoFourriere)
            {
                //GameMode.Instance.PoundManager.PoundVehicleList.AddRange(vehicleList);
                await GameMode.Instance.Save();
            }
            else
            {
                List<string> checkedPlate = new List<string>(); // for check if the vehicle is duplicated
                for (int i = 0; i < vehicleList.Count; i++)
                {
                    var vehicle = vehicleList[i];

                    if (vehicle == null)
                        continue;
                    if (vehicle.IsParked)
                        continue;
                    if (vehicle.IsInPound)
                        continue;

                    //await DeleteVehicleInAllParking(vehicle.Plate);
                    //DeleteVehicleInPound(vehicle.Plate);

                    if (!checkedPlate.Contains(vehicle.Plate))
                    {

                        checkedPlate.Add(vehicle.Plate);
                        await vehicle.SpawnVehicle();
                    }
                    else
                    {
                        vehicleList.Remove(vehicle);
                        i--;
                        Alt.Server.LogInfo($"Vehicle duplicated plate: {vehicle.Plate} Owner: {vehicle.OwnerID} ");
                    }
                }
            }

            Alt.Server.LogInfo($"--- Finish loading all vehicle in database: {vehicleList.Count} ---");
        }

        #endregion

        #region Methods
        public static async Task<VehicleHandler> SpawnVehicle(string socialClubName, uint model, Vector3 position, Vector3 rotation, int primaryColor = 0, int secondaryColor = 0,
float fuel = 100, float fuelMax = 100, string plate = null, bool engineStatus = false, bool locked = true,
IPlayer client = null, ConcurrentDictionary<int, int> mods = null, int[] neon = null, bool spawnVeh = false, uint dimension = (uint)short.MaxValue, Inventory.Inventory inventory = null, bool freeze = false, byte dirt = 0, float health = 1000)
        {
            if (model == 0)
                return null;

            VehicleHandler veh = new VehicleHandler(socialClubName, model, position, rotation, (byte)primaryColor, (byte)secondaryColor, fuel, fuelMax, plate, engineStatus, locked, client, mods, neon, spawnVeh, (short)dimension, inventory, freeze, dirt, health);
            await veh.SpawnVehicle(new Models.Location(position, rotation));

            if (!veh.SpawnVeh && IsPlateUnique(veh.Plate))
            {
                //await veh.InsertVehicle();
                GameMode.Instance.PlateList.Add(veh.Plate);
            }

            return veh;
        }
        public static bool IsVehicleInSpawn(Models.Location location, float distance = 4, short dimension = short.MaxValue) =>
            IsVehicleInSpawn(location.Pos, distance, dimension);

        public static bool IsVehicleInSpawn(Vector3 location, float distance = 4, short dimension = short.MaxValue)
        {
            var vehHandler = GetNearestVehicle(location, distance, dimension);

            if (vehHandler != null)
                return true;

            return false;
        }

        public static string GenerateRandomPlate()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] stringChars = new char[8];
            Random random = new Random();
            string generatedPlate = "";

            do
            {
                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }
                return generatedPlate = new string(stringChars);
            } while (!IsPlateUnique(generatedPlate));
        }
        public static async Task<IVehicle> GetNearestVehicle(IPlayer client, float distance = 3.0f, short dimension = short.MaxValue) =>
            GetNearestVehicle(await client.GetPositionAsync(), distance, dimension);

        public static IVehicle GetNearestVehicle(Vector3 position, float distance = 3.0f, short dimension = short.MaxValue)
        {
            // BUG v752 : La liste des véhicules renvoie des véhicules supprimés
            // ICollection<IVehicle> vehs = Alt.GetAllVehicles();
            ICollection<IVehicle> vehs = GetAllVehicles();
            IVehicle nearest = null;

            foreach(IVehicle veh in vehs)
            {
                if (!veh.Exists || veh.Dimension != dimension || position.DistanceTo2D(veh.Position) > distance)
                    continue;
                else if (nearest == null)
                    nearest = veh;
                else if (position.DistanceTo2D(veh.Position) < position.DistanceTo(nearest.Position))
                    nearest = veh;
            }

            return nearest;
        }

        public static ICollection<IVehicle> GetAllVehicles()
        {
            return GameMode.Instance.VehicleManager.VehicleHandlerList.Select(v => v.Value.Vehicle).ToArray();
        }

        public static bool IsPlateUnique(string plate) => !GameMode.Instance.PlateList.Exists(x => x == plate);

        public IVehicle GetVehicleByPlate(string plate)
        {
            foreach(KeyValuePair<IVehicle, VehicleHandler> entity in this.VehicleHandlerList)
            {
                if (entity.Value.Plate == plate)
                    return entity.Key; 
            }
            return null;
        }

        public static async Task DeleteVehicleFromAllParkings(string Plate )
        {
            foreach (Models.Parking parking in Models.Parking.ParkingList)
            {
                bool saveNeeded = false;
                lock (parking.ListVehicleStored)
                {
                    if (!parking.ListVehicleStored.Exists(p => p.Plate == Plate))
                        continue;

                    parking.ListVehicleStored.RemoveAll(p => p.Plate == Plate);
                    saveNeeded = true;
                }
                if(saveNeeded)
                    await parking.OnSaveNeeded.Invoke();
            }
        }

        public static VehicleHandler GetVehicleHandler(IVehicle vehicle)
        {
            GameMode.Instance.VehicleManager.VehicleHandlerList.TryGetValue(vehicle, out VehicleHandler vh);
            return vh;
        }

        public static VehicleHandler GetVehicleHandlerWithPlate(string Plate)
        {
            foreach(KeyValuePair<IVehicle, VehicleHandler> veh in GameMode.Instance.VehicleManager.VehicleHandlerList)
            {
                if (veh.Value.Plate == Plate)
                    return veh.Value;
            }

            return null;
        }

        public static IVehicle GetVehicleWithPlate(string Plate)
        {
            foreach(KeyValuePair<IVehicle, VehicleHandler> veh in GameMode.Instance.VehicleManager.VehicleHandlerList)
            {
                if (veh.Value.Plate == Plate)
                    return veh.Key;
            }

            return null;
        }
        #endregion
    }
}
