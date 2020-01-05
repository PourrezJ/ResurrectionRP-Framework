using AltV.Net.Elements.Entities;
using AltV.Net;
using AltV.Net.Async;
using ResurrectionRP_Server.Entities.Vehicles;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System;
using AltV.Net.Enums;
using ResurrectionRP_Server.Utils;
using System.Linq;

namespace ResurrectionRP_Server
{
    public static class VehicleExtensions
    {
        public static VehicleHandler GetVehicleHandler(this IVehicle vehicle)
        {
            return vehicle as VehicleHandler;
        }

        public static Vector3 GetVehicleVector(this IVehicle vehicle)
        {
            var pos = vehicle.GetPosition();
            return new Vector3(pos.X, pos.Y, pos.Z);
        }

        public static List<IVehicle> GetVehiclesInRange(this IVehicle client, int Range)
        {
            List<IVehicle> endup = new List<IVehicle>();

            var vehs = Alt.GetAllVehicles();
            lock (vehs)
            {
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
                    endup.Add(veh);
            }

            return endup;
        }

        public static void ResetData(this IVehicle vehicle, string key)
        {
            vehicle.SetData(key, null);
        }

        public static void Freeze(this IVehicle vehicle, bool freeze)
        {
            if (!vehicle.Exists)
                return;

            vehicle.SetSyncedMetaData("IsFreezed", freeze);
        }

        public static async Task FreezeAsync(this IVehicle vehicle, bool freeze)
        {
            if (!vehicle.Exists)
                return;

            await vehicle.SetSyncedMetaDataAsync("IsFreezed", freeze);
        }

        public static void Invincible(this IVehicle vehicle, bool invincible)
        {
            if (!vehicle.Exists)
                return;

            vehicle.SetSyncedMetaData("IsInvincible", invincible);
        }

        public static async Task InvincibleAsync(this IVehicle vehicle, bool invincible)
        {
            if (!await vehicle.ExistsAsync())
                return;

            await vehicle.SetSyncedMetaDataAsync("IsInvincible", invincible);
        }

        public static void SetDoorStateFix(this IVehicle vehicle, IPlayer client, VehicleDoor door, VehicleDoorState state, bool direct)
        {
            if (!vehicle.Exists || client == null || !client.Exists)
                return;

            client.EmitLocked("SetDoorState", vehicle, (int)door, (int)state, direct);
        }

        public static async Task SetPlayerIntoVehicle(this IVehicle client, IPlayer target)
        {
            await target.EmitAsync("SetPlayerIntoVehicle", client, -1);
        }

        public static async Task TryPutPlayerInVehicle(this IVehicle client, IPlayer target)
        {
            await target.EmitAsync("TrySetPlayerIntoVehicle", client);
        }

        public static bool HasData( this IVehicle vehicle, string dataName)
        {
            if (vehicle.GetData(dataName, out string test))
                return true;

            return false;
        }
    }
}
