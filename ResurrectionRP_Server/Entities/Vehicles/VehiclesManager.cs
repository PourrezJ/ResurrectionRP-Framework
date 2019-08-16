using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        }
        #endregion

        #region Server Events
        private async Task OnPlayerEnterVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            await player.EmitAsync("OnPlayerEnterVehicle", vehicle.Id, Convert.ToInt32(seat), 50, 50);
        }

        private async Task OnPlayerLeaveVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            await player.EmitAsync("OnPlayerLeaveVehicle");
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
        public static VehicleHandler GetHandlerByVehicle(IVehicle vehicle)
        {
            try
            {
                if (vehicle != null && vehicle.Exists)
                {
                    if (vehicle.GetData("VehicleHandler", out object data))
                    {
                        return data as VehicleHandler;
                    }

                    if (GameMode.Instance.VehicleManager.VehicleHandlerList.TryGetValue(vehicle, out VehicleHandler value))
                        return value;
                }

            }
            catch (Exception ex)
            {
                Alt.Server.LogInfo($"GetVehicleByVehicle with plate {vehicle.GetNumberplateTextAsync()}: " + ex);
            }
            return null;
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

        public static bool IsPlateUnique(string plate) => !GameMode.Instance.PlateList.Exists(x => x == plate);
        #endregion
    }
}
