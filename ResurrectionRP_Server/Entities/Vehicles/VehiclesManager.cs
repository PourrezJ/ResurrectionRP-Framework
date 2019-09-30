using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Driver;
using ResurrectionRP_Server.Entities.Players;
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
        // Use ConcurrentDictionary as ther eis no concurrent list
        private static ConcurrentDictionary<string, VehicleHandler> _vehicleHandlers = new ConcurrentDictionary<string, VehicleHandler>();
        public static ConcurrentDictionary<IVehicle, VehicleHandler> VehicleHandlerList { get; } = new ConcurrentDictionary<IVehicle, VehicleHandler>();

        #endregion

        #region Constructor
        public VehiclesManager()
        {
            AltAsync.OnPlayerEnterVehicle += OnPlayerEnterVehicle;
            AltAsync.OnPlayerLeaveVehicle += OnPlayerLeaveVehicle;
            AltAsync.OnPlayerChangeVehicleSeat += OnPlayerChangeVehicleSeat;

            AltAsync.OnClient("LockUnlockVehicle", LockUnlockVehicle);
            AltAsync.OnClient("UpdateTrailer", UpdateTrailerState);
        }
        #endregion

        #region Server Events
        private static Task OnPlayerEnterVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            PlayerHandler ph = player.GetPlayerHandler();
            VehicleHandler vh = vehicle.GetVehicleHandler();

            if (ph != null && vh != null)
            {
                if (seat == 1)
                    vh.LastDriver = ph.Identite.Name;

                ph.Vehicle = vh;
                ph.UpdateFull();
                player.EmitLocked("OnPlayerEnterVehicle", vehicle, Convert.ToInt32(seat), vh.Fuel, vh.FuelMax, vh.Milage, vh.FuelConsumption);
            }

            return Task.CompletedTask;
        }

        private static Task OnPlayerChangeVehicleSeat(IVehicle vehicle, IPlayer player, byte oldSeat, byte newSeat)
        {
            PlayerHandler ph = player.GetPlayerHandler();
            VehicleHandler vh = vehicle.GetVehicleHandler();

            if (newSeat == 1 && vh != null)
            {
                vh.LastDriver = ph.Identite.Name;
                vh.UpdateInBackground();
            }

            return Task.CompletedTask;
        }

        public static Task UpdateTrailerState(IPlayer player, object[] args)
        {
            if (!player.Exists)
                return Task.CompletedTask;

            if (args[0] == null || args[1] == null)
                return Task.CompletedTask;

            if (GameMode.Instance.IsDebug)
                Alt.Server.LogInfo("VehicleManager | Update trailer state for " + player.GetPlayerHandler().PID + " to " + args[1] + " for " + ((IVehicle)(args[0])).NumberplateText );

            VehicleHandler veh = ((IVehicle)args[0]).GetVehicleHandler();
           
            veh.hasTrailer = (bool)args[1];

            if (args[2] != null)
            {
                veh.Trailer = (IEntity)args[2];
                ((IVehicle)veh.Trailer).GetVehicleHandler().UpdateInBackground();
            }
            else
            {
                ((IVehicle)veh.Trailer).GetVehicleHandler().UpdateInBackground();
                veh.Trailer = null;
            }

            return Task.CompletedTask;
        }

        private static async Task LockUnlockVehicle(IPlayer player, object[] args)
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
                var receverList = vehicle.GetPlayersInRange(5f);

                foreach (IPlayer recever in receverList)
                {
                    if (!recever.Exists)
                        continue;

                    await recever.PlaySoundFromEntity(veh.Vehicle, 0, "5_SEC_WARNING", "HUD_MINI_GAME_SOUNDSET");
                }

                veh.UpdateInBackground();
            }
        }

        private static Task OnPlayerLeaveVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            if (!player.Exists || !vehicle.Exists)
                return Task.CompletedTask;

            VehicleHandler vh = vehicle.GetVehicleHandler();
            PlayerHandler ph = player.GetPlayerHandler();

            if (ph != null)
            {
                ph.Vehicle = null;
                ph.UpdateFull();
            }

            if (vh != null)
                vh.UpdateInBackground();

            return Task.CompletedTask;
        }
        #endregion

        #region Database
        public static async Task LoadAllVehicles()
        {
            Alt.Server.LogInfo("--- Start loading all vehicles in database ---");
            List<VehicleHandler> vehicles = await Database.MongoDB.GetCollectionSafe<VehicleHandler>("vehicles").AsQueryable().ToListAsync();

            if (GameMode.Instance.AutoPound)
            {
                //GameMode.Instance.PoundManager.PoundVehicleList.AddRange(vehicleList);
                await GameMode.Instance.Save();
            }
            else
            {
                await AltAsync.Do(() =>
                {
                    foreach (VehicleHandler vehicle in vehicles)
                    {
                        _vehicleHandlers.TryAdd(vehicle.Plate, vehicle);

                        if (vehicle.IsParked || vehicle.IsInPound)
                            continue;

                        vehicle.SpawnVehicle();
                    }
                });
            }

            Alt.Server.LogInfo($"--- Finish loading all vehicles in database: {_vehicleHandlers.Count} ---");
        }
        #endregion

        #region Methods
        public static async Task<VehicleHandler> SpawnVehicleAsync(string socialClubName, uint model, Vector3 position, Vector3 rotation, int primaryColor = 0, int secondaryColor = 0,
        float fuel = 100, float fuelMax = 100, string plate = null, bool engineStatus = false, bool locked = true,
        IPlayer client = null, ConcurrentDictionary<byte, byte> mods = null, int[] neon = null, bool spawnVeh = false, uint dimension = (uint)short.MaxValue, Inventory.Inventory inventory = null, bool freeze = false, byte dirt = 0, float health = 1000)
        {
            if (model == 0)
                return null;

            VehicleHandler veh = new VehicleHandler(socialClubName, model, position, rotation, (byte)primaryColor, (byte)secondaryColor, fuel, fuelMax, plate, engineStatus, locked, client, mods, neon, spawnVeh, (short)dimension, inventory, freeze, dirt, health);
            _vehicleHandlers.TryAdd(veh.Plate, veh);
            await veh.SpawnVehicleAsync(new Models.Location(position, rotation));
            return veh;
        }

        public static VehicleHandler SpawnVehicle(string socialClubName, uint model, Vector3 position, Vector3 rotation, int primaryColor = 0, int secondaryColor = 0,
        float fuel = 100, float fuelMax = 100, string plate = null, bool engineStatus = false, bool locked = true,
        IPlayer client = null, ConcurrentDictionary<byte, byte> mods = null, int[] neon = null, bool spawnVeh = false, uint dimension = (uint)short.MaxValue, Inventory.Inventory inventory = null, bool freeze = false, byte dirt = 0, float health = 1000)
        {
            if (model == 0)
                return null;

            VehicleHandler veh = new VehicleHandler(socialClubName, model, position, rotation, (byte)primaryColor, (byte)secondaryColor, fuel, fuelMax, plate, engineStatus, locked, client, mods, neon, spawnVeh, (short)dimension, inventory, freeze, dirt, health);
            _vehicleHandlers.TryAdd(veh.Plate, veh);
            veh.SpawnVehicle(new Models.Location(position, rotation));
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

        private static bool IsPlateUnique(string plate) => !_vehicleHandlers.ContainsKey(plate);

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

        public static IVehicle GetNearestVehicle(Vector3 position, float distance = 3.0f, short dimension = short.MaxValue)
        {
            // BUG v752 : La liste des véhicules renvoie des véhicules supprimés
            // ICollection<IVehicle> vehs = Alt.GetAllVehicles();
            ICollection<IVehicle> vehs = GetAllVehiclesInGame();
            IVehicle nearest = null;

            foreach (IVehicle veh in vehs)
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

        public static async Task<IVehicle> GetNearestVehicleAsync(Vector3 position, float distance = 3.0f, short dimension = short.MaxValue)
        {
            // BUG v752 : La liste des véhicules renvoie des véhicules supprimés
            // ICollection<IVehicle> vehs = Alt.GetAllVehicles();
            ICollection<IVehicle> vehs = GetAllVehiclesInGame();
            IVehicle nearest = null;

            await AltAsync.Do(() =>
            {
                foreach (IVehicle veh in vehs)
                {
                    if (!veh.Exists || veh.Dimension != dimension || position.DistanceTo2D(veh.Position) > distance)
                        continue;
                    else if (nearest == null)
                        nearest = veh;
                    else if (position.DistanceTo2D(veh.Position) < position.DistanceTo(nearest.Position))
                        nearest = veh;
                }
            });

            return nearest;
        }

        public static ICollection<VehicleHandler> GetAllVehicles()
        {
            return _vehicleHandlers.Values;
        }

        public static ICollection<IVehicle> GetAllVehiclesInGame()
        {
            return VehicleHandlerList.Select(v => v.Value.Vehicle).ToArray();
        }

        public static IVehicle GetVehicleByPlate(string plate)
        {
            foreach (KeyValuePair<IVehicle, VehicleHandler> entity in VehicleHandlerList)
            {
                if (entity.Value.Plate == plate)
                    return entity.Key;
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

        public static VehicleHandler GetVehicleHandler(IVehicle vehicle)
        {
            VehicleHandlerList.TryGetValue(vehicle, out VehicleHandler vh);
            return vh;
        }

        public static VehicleHandler GetVehicleHandler(string plate)
        {
            _vehicleHandlers.TryGetValue(plate, out VehicleHandler vh);
            return vh;
        }

        public static IVehicle GetVehicleWithPlate(string Plate)
        {
            foreach (KeyValuePair<IVehicle, VehicleHandler> veh in VehicleHandlerList)
            {
                if (veh.Value.Plate == Plate)
                    return veh.Key;
            }

            return null;
        }

        public static void UpdateVehiclesMilageAndFuel()
        {
            foreach (IVehicle vehicle in GetAllVehiclesInGame())
            {
                if (vehicle.Exists && vehicle.EngineOn)
                    vehicle.GetVehicleHandler()?.UpdateMilageAndFuel();
            }
        }
        #endregion
    }
}
