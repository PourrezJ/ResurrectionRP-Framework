using AltV.Net.Elements.Entities;
using AltV.Net;
using AltV.Net.Async;
using ResurrectionRP_Server.Entities.Vehicles;
using System.Collections.Generic;
using System.Numerics;

namespace ResurrectionRP_Server
{
    public static class VehicleExtensions
    {
        public static VehicleHandler GetVehicleHandler(this IVehicle vehicle)
        {
            return VehiclesManager.GetHandlerByVehicle(vehicle);
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

        public static async void TryPutPlayerInVehicle(this IVehicle client, IPlayer target)
        {
            await target.EmitAsync("TrySetPlayerIntoVehicle", client);
        }

    }
}
