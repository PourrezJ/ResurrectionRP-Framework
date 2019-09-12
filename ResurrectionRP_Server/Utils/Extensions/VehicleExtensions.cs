using AltV.Net.Elements.Entities;
using AltV.Net;
using AltV.Net.Async;
using ResurrectionRP_Server.Entities.Vehicles;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System;

namespace ResurrectionRP_Server
{
    public static class VehicleExtensions
    {
        public static VehicleHandler GetVehicleHandler(this IVehicle vehicle)
        {
            return VehiclesManager.GetVehicleHandlerWithPlate(vehicle.NumberplateText);
        }
        public static Vector3 GetVehicleVector(this IVehicle vehicle)
        {
            var pos = vehicle.GetPosition();
            return new Vector3(pos.X, pos.Y, pos.Z);
        }

        public static List<IVehicle> GetVehiclesInRange(this IVehicle client, int Range)
        {
            // BUG v752 : La liste des véhicules renvoie des véhicules supprimés
            // var vehs = Alt.GetAllVehicles();
            var vehs = Entities.Vehicles.VehiclesManager.GetAllVehicles();

            List<IVehicle> endup = new List<IVehicle>();
            var position = client.GetPosition();
            Vector3 osition = new Vector3(position.X, position.Y, position.Z);
            foreach (IVehicle veh in vehs)
            {
                if (!veh.Exists)
                    continue;
                var vehpos = veh.GetPosition();
                if (osition.DistanceTo2D(new Vector3(vehpos.X, vehpos.Y, vehpos.Z)) <= Range)
                {
                    endup.Add(veh);
                }
            }
            return endup;
        }
        public static List<IPlayer> GetPlayersInRange(this IVehicle client, float Range)
        {
            var vehs = Alt.GetAllPlayers();
            List<IPlayer> endup = new List<IPlayer>();
            var position = client.GetPosition();
            Vector3 osition = new Vector3(position.X, position.Y, position.Z);
            foreach (IPlayer veh in vehs)
            {
                if (!veh.Exists)
                    continue;
                var vehpos = veh.GetPosition();
                if (osition.DistanceTo2D(new Vector3(vehpos.X, vehpos.Y, vehpos.Z)) <= Range)
                {
                    endup.Add(veh);
                }
            }
            return endup;
        }

        public static void ResetData(this IVehicle client, string key)
        {
            client.SetData(key, null);
        }

        public static async Task SetPlayerIntoVehicle(this IVehicle client, IPlayer target)
        {
            await target.EmitAsync("SetPlayerIntoVehicle", client, -1);
        }

        public static async Task TryPutPlayerInVehicle(this IVehicle client, IPlayer target)
        {
            await target.EmitAsync("TrySetPlayerIntoVehicle", client);
        }

        public static VehicleHandler GetHandlerByVehicle(IVehicle vehicle)
        {
            try
            {
                if (vehicle != null && vehicle.Exists)
                {
                    if (vehicle.GetData("VehicleHandler", out object data))
                        return data as VehicleHandler;

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
        public static bool HasData( this IVehicle vehicle, string dataName)
        {
            if (vehicle.GetData(dataName, out string test) && test != null)
                return true;

            return false;
        }

        public static Task RepairAsync(this IVehicle vehicle)
        {
            // TODO
            return Task.CompletedTask;
        }
        public static async Task SetModAsync(this IVehicle vehicle, int type, int mod)
        {
            if (mod < 0)
                Alt.Server.LogError("VehicleExtension: " + "Mod type must not be negativ ! It's a byte !");
            await vehicle.SetModAsync((byte)type, (byte)mod);
        }
    }

}
