using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Driver;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;
using ResurrectionRP_Server.Database;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public static class VehiclesManager
    {
        #region Fields
        private static DateTime _nextLoop = DateTime.Now;

        private static ConcurrentDictionary<string, VehicleData> _vehicleHandlers = new ConcurrentDictionary<string, VehicleData>();

        #endregion

        #region Constructor
        public static void Init()
        {
            Alt.OnPlayerEnterVehicle += OnPlayerEnterVehicle;
            Alt.OnPlayerLeaveVehicle += OnPlayerLeaveVehicle;
            Alt.OnPlayerChangeVehicleSeat += OnPlayerChangeVehicleSeat;

            Alt.OnClient("LockUnlockVehicle", LockUnlockVehicle);
            Alt.OnClient("UpdateTrailer", UpdateTrailerState);
        }
        #endregion

        #region Event handlers
        private static void OnPlayerEnterVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            PlayerHandler ph = player.GetPlayerHandler();
            VehicleHandler vh = vehicle.GetVehicleHandler();

            if (ph != null && vh != null)
            {
                if (seat == 1)
                    vh.VehicleData.LastDriver = ph.Identite.Name;

                ph.Vehicle = vh;
                ph.UpdateFull();
                player.EmitLocked("OnPlayerEnterVehicle", vehicle, Convert.ToInt32(seat), vh.VehicleData.Fuel, vh.VehicleData.FuelMax, vh.VehicleData.Milage, vh.VehicleData.FuelConsumption);
            }
        }

        private static void OnPlayerChangeVehicleSeat(IVehicle vehicle, IPlayer player, byte oldSeat, byte newSeat)
        {
            PlayerHandler ph = player.GetPlayerHandler();
            VehicleHandler vh = vehicle.GetVehicleHandler();

            if (newSeat == 1 && vh != null)
            {
                vh.VehicleData.LastDriver = ph.Identite.Name;
                vh.UpdateInBackground();
            }
        }

        public static void UpdateTrailerState(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return;

            if (args[0] == null || args[1] == null)
                return;

            if (GameMode.IsDebug)
                Alt.Server.LogInfo("VehicleManager | Update trailer state for " + player.GetPlayerHandler().PID + " to " + args[1] + " for " + ((IVehicle)(args[0])).NumberplateText);

            VehicleHandler veh = ((IVehicle)args[0])?.GetVehicleHandler();

            if (veh == null)
                return;

            veh.HasTrailer = (bool)args[1];

            if (args[2] != null)
            {
                veh.Trailer = (IEntity)args[2];
                ((IVehicle)veh.Trailer)?.GetVehicleHandler().UpdateInBackground();
            }
            else
            {
                ((IVehicle)veh.Trailer)?.GetVehicleHandler().UpdateInBackground();
                veh.Trailer = null;
            }
        }

        private static void LockUnlockVehicle(IPlayer player, object[] args)
        {
            if (GameMode.IsDebug)
                Alt.Server.LogColored("~b~VehicleManager ~w~| " + player.GetSocialClub() + " is trying to lock/unlock a car");

            var vehicle = args[0] as IVehicle;

            if (vehicle != null && !vehicle.Exists)
                return;

            VehicleHandler veh = vehicle.GetVehicleHandler();

            if (veh == null)
                return;

            if (veh.LockUnlock(player))
            {
                var receverList = vehicle.GetPlayersInRange(5f);

                foreach (IPlayer recever in receverList)
                {
                    if (!recever.Exists)
                        continue;

                    recever.PlaySoundFromEntity(veh, 0, "5_SEC_WARNING", "HUD_MINI_GAME_SOUNDSET");
                }

                veh.UpdateInBackground();
            }
        }

        private static void OnPlayerLeaveVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            if (!player.Exists || !vehicle.Exists)
                return;

            VehicleHandler vh = vehicle.GetVehicleHandler();
            PlayerHandler ph = player.GetPlayerHandler();

            if (ph != null)
            {
                ph.Vehicle = null;
                ph.UpdateFull();
            }

            if (vh != null)
                vh.UpdateInBackground();
        }
        #endregion

        #region Loop
        public static void OnTick()
        {
            if (_nextLoop > DateTime.Now)
                return;

            var vehicles = Alt.GetAllVehicles();

            TimeSpan expireTime = TimeSpan.FromDays(3);

            lock (vehicles)
            {
                foreach (Vehicle veh in vehicles)
                {
                    if (!veh.Exists)
                        continue;

                    if (!(veh is VehicleHandler))
                        continue;

                     VehicleHandler vehicle = veh.GetVehicleHandler();

                    if (vehicle.EngineOn)
                    {
                        vehicle.VehicleData.UpdateMilageAndFuel();
                        var currentRot = vehicle.Rotation;

                        if (vehicle.VehicleManifest.VehicleClass != 15 && vehicle.VehicleManifest.VehicleClass != 16 && currentRot.Pitch >= 1.2)
                        {
                            vehicle.EngineOn = false;

                            if (vehicle.Driver != null)
                            {
                                vehicle.VehicleData.LastUse = DateTime.Now;
                                vehicle.Driver.SendNotification("Le moteur vient de caler.");
                            }
                        }
                    }

                    if (vehicle.SpawnVeh)
                        continue;

                    // Mise en fourrière auto
                    TimeSpan timeSinceLastUse = DateTime.Now - vehicle.VehicleData.LastUse;

                    if (timeSinceLastUse >= expireTime)
                        Task.Run(async () => await Pound.AddVehicleInPoundAsync(vehicle.VehicleData));
                }
            }

            _nextLoop = _nextLoop.AddMilliseconds(1000);
        }
        #endregion

        #region Database
        public static void LoadAllVehicles()
        {
            Alt.Server.LogInfo("--- Start loading all vehicles in database ---");
            var vehicles = Database.MongoDB.GetCollectionSafe<VehicleData>("vehicles").AsQueryable();

            if (GameMode.Instance.AutoPound)
            {
                //GameMode.Instance.PoundManager.PoundVehicleList.AddRange(vehicleList);
                Task.Run(()=> GameMode.Instance.Save());
            }
            else
            {
                foreach (VehicleData vd in vehicles)
                {
                    if (_vehicleHandlers.TryAdd(vd.Plate, vd))
                    {
                        if (vd.IsParked || vd.IsInPound)
                            continue;
                        vd.SpawnVehicle();
                    }
                }
            }

            Alt.Server.LogInfo($"--- Finish loading all vehicles in database: {_vehicleHandlers.Count} ---");
        }
        #endregion

        #region Methods

        public static VehicleHandler SpawnVehicle(string socialClubName, uint model, Vector3 position, Vector3 rotation, int primaryColor = 0, int secondaryColor = 0,
        float fuel = 100, float fuelMax = 100, string plate = null, bool engineStatus = false, bool locked = true,
        IPlayer client = null, ConcurrentDictionary<byte, byte> mods = null, int[] neon = null, bool spawnVeh = false, uint dimension = (uint)GameMode.GlobalDimension, Inventory.Inventory inventory = null, bool freeze = false, byte dirt = 0, float health = 1000)
        {
            if (model == 0)
                return null;

            VehicleHandler veh = new VehicleHandler(socialClubName, model, position, rotation, (byte)primaryColor, (byte)secondaryColor, fuel, fuelMax, plate, engineStatus, locked, client, mods, neon, spawnVeh, (short)dimension, inventory, freeze, dirt, health);
            _vehicleHandlers.TryAdd(veh.NumberplateText, veh.VehicleData);

            //veh.SpawnVehicle(new Models.Location(position, rotation));
            return veh;
        }

        public static bool IsVehicleInSpawn(Models.Location location, float distance = 4, short dimension = GameMode.GlobalDimension) =>
            IsVehicleInSpawn(location.Pos, distance, dimension);

        public static bool IsVehicleInSpawn(Vector3 location, float distance = 4, short dimension = GameMode.GlobalDimension)
        {
            var vehHandler = GetNearestVehicle(location, distance, dimension);

            if (vehHandler != null)
                return true;

            return false;
        }

        private static bool IsPlateUnique(string plate)
        {
            var vehicles = Alt.GetAllVehicles();

            lock (vehicles)
            {
                if (vehicles.Any(p => p.NumberplateText == plate))
                    return false;
                else
                    return true;
            }
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
                    stringChars[i] = chars[random.Next(chars.Length)];

                generatedPlate = new string(stringChars);
            }
            while (!IsPlateUnique(generatedPlate));

            return generatedPlate;
        }

        public static IVehicle GetNearestVehicle(Vector3 position, float distance = 3.0f, short dimension = GameMode.GlobalDimension)
        {
            var vehs = GetNearestsVehicles(position, distance, dimension);

            if (vehs == null)
                return null;
            else
                return vehs[0];
        }

        public static List<IVehicle> GetNearestsVehicles(Vector3 position, float distance = 3.0f, short dimension = GameMode.GlobalDimension)
        {
            ICollection<IVehicle> vehs = Alt.GetAllVehicles();

            List<IVehicle> nearest = null;

            lock (vehs)
            {
                foreach (IVehicle veh in vehs)
                {
                    if (!veh.Exists || veh.Dimension != dimension || position.DistanceTo2D(veh.Position) > distance)
                        continue;

                    nearest.Add(veh);
                }

            }

            return nearest;
        }

        public static async Task<IVehicle> GetNearestVehicleAsync(Vector3 position, float distance = 3.0f, short dimension = GameMode.GlobalDimension)
        {
            IVehicle nearest = null;

            await AltAsync.Do(() =>
            {
                nearest = GetNearestVehicle(position, distance, dimension);
            });

            return nearest;
        }
        
        public static ICollection<VehicleData> GetAllVehicles()
        {
            return _vehicleHandlers.Values;
        }

        public static VehicleData GetVehicleDataWithPlate(string plate)
        {
            foreach (var entity in _vehicleHandlers)
            {
                if (entity.Value.Plate == plate)
                    return entity.Value;
            }
            return null;
        }

        public static async Task DeleteVehicleFromAllParkings(string Plate)
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

                if (saveNeeded && parking.OnSaveNeeded != null)
                    await parking.OnSaveNeeded.Invoke();
            }
        }
        /*
        public static void DeleteVehicleHandler(VehicleHandler vehicle)
        {
            bool result = _vehicleHandlers.TryRemove(vehicle.Plate, out _);
        }*/

        public static ICollection<VehicleHandler> GetAllInGameVehicleHandler()
        {
            var vehicles = Alt.GetAllVehicles().ToArray();
            ICollection<VehicleHandler> vhlist = null;
            lock (vehicles)
            {
                vhlist = Array.ConvertAll(vehicles, e => e.GetVehicleHandler());
            }
            return vhlist;
        }
        #endregion
    }
}
