﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Utils.Enums;

namespace ResurrectionRP_Server.Entities.Players
{
    public class PlayerCommands
    {
        public PlayerCommands()
        {
            Chat.RegisterCmd("tp", TpCoord);
            Chat.RegisterCmd("cls", Cls);
            Chat.RegisterCmd("cloth", Cloth);
            Chat.RegisterCmd("refuel", Refuel);
            Chat.RegisterCmd("repair", Repair);
        }

        private async Task TpCoord(IPlayer player, string[] args)
        {
            try
            {
                if (player.GetPlayerHandler().StaffRank <= AdminRank.Player)
                    return;

                await player.SetPositionAsync(new Position(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2])));
            }
            catch
            {
                player.SendChatMessage("Erreur dans la saisie des coordonnées");
            }
        }
        public Task Cls(IPlayer player, string[] args)
        {
            player.EmitLocked("EmptyChat");
            return Task.CompletedTask;
        }
        public Task Refuel(IPlayer player, string[] args)
        {
            if(player.Vehicle == null)
            {
                player.DisplaySubtitle("Vous devez être dans un véhicule", 10000);
                return Task.CompletedTask;
            }
            player.Vehicle.GetVehicleHandler()?.SetFuel(player.Vehicle.GetVehicleHandler().FuelMax);
            player.DisplaySubtitle("Vehicule restoré", 5000);
            return Task.CompletedTask;
        }
        public async Task Repair(IPlayer player, string[] args)
        {
            if(player.Vehicle == null)
            {
                player.DisplaySubtitle("Vous devez être dans un véhicule", 10000);
                return ;
            }
            await player.Vehicle.RepairAsync();
            player.DisplaySubtitle("Vehicule restoré", 5000);
        }

        private Task Cloth(IPlayer player, object[] args)
        {
            player.SetCloth((Models.ClothSlot)Convert.ToInt32(args[0]), (int)args[1], (int)args[2], (int)args[3]);
            return Task.CompletedTask;
        }
    }
}
