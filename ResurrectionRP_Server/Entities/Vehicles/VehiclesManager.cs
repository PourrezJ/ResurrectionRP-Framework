using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using System;
using System.Collections.Concurrent;
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
        private Task OnPlayerEnterVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            throw new NotImplementedException();
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
        #endregion
    }
}
