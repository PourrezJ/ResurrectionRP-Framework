﻿using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Factions;
using ResurrectionRP_Server.Houses;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Society;
using ResurrectionRP_Server.Utils.Enums;
using System;
using System.Numerics;
using System.Threading.Tasks;
using VehicleInfoLoader.Data;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public class VehicleCommands
    {
        public VehicleCommands()
        {
            Chat.RegisterCmd("vehicleinfo", VehicleInfo);
        }

        private async Task VehicleInfo(IPlayer player, string[] args)
        {
            if (player == null || !player.Exists)
                return;

            if (args == null || args.Length == 0)
            {
                player.SendNotificationError("Vous devez indiquer la plaque d'immatriculation du véhicule");
                return;
            }

            PlayerHandler ph = player.GetPlayerHandler();

            if (ph == null || player.GetPlayerHandler().StaffRank <= AdminRank.Player)
                return;

            string plate = args[0].ToUpper();
            VehicleHandler vehicle = null;

            foreach (VehicleHandler veh in VehiclesManager.GetAllVehicles())
            {
                if (veh.Plate.ToUpper() == plate)
                {
                    vehicle = veh;
                    break;
                }
            }

            if (vehicle == null)
            {
                player.SendNotificationError("Véhicule inconnu");
                return;
            }

            PlayerHandler owner = await PlayerManager.GetPlayerHandlerDatabase(vehicle.OwnerID);
            player.SendChatMessage($"Immatriculation : {vehicle.Plate}");
            VehicleManifest manifest = VehicleInfoLoader.VehicleInfoLoader.Get(vehicle.Model);
            player.SendChatMessage($"Modèle : {manifest.LocalizedName} ({vehicle.Model})");

            if (owner != null)
                player.SendChatMessage($"Propriétaire : {owner.Identite.Name} ({vehicle.OwnerID})");
            else
                player.SendChatMessage("Propriétaire : Aucun");

            player.SendChatMessage($"Verrouillé : {vehicle.IsLocked()}");
            player.SendChatMessage($"Dernier chauffeur : {vehicle.LastDriver}");
            player.SendChatMessage($"Dernier usage : {vehicle.LastUse}");

            if (vehicle.ParkingName != string.Empty)
                player.SendChatMessage($"Parking : {vehicle.ParkingName}");

            Vector3 position = vehicle.Location.Pos;
            string dim = vehicle.Dimension == short.MaxValue ? "-1" : vehicle.Dimension.ToString();
            player.SendChatMessage($"Position : X: {position.X}, Y: {position.Y}, Z: {position.Z}, Dim: {dim}");
        }
    }
}