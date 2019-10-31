using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Factions;
using ResurrectionRP_Server.Factions.Model;
using ResurrectionRP_Server.Houses;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Society;
using ResurrectionRP_Server.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            Chat.RegisterCmd("setfuel", SetFuel);
            Chat.RegisterCmd("repair", Repair);
            Chat.RegisterCmd("wheel", Wheel);
            Chat.RegisterCmd("doorstate", DoorState);
            Chat.RegisterCmd("additem", AddItem);
            Chat.RegisterCmd("wipe", Wipe);
            Chat.RegisterCmd("sobre", Sobre);
        }

        private void Sobre(IPlayer player, string[] args)
        {
            var ph = player.GetPlayerHandler();

            if (ph == null)
                return;

            ph.Alcohol = 0;
            ph.UpdateFull();
            player.SendNotificationSuccess("Commande effectuée!");
        }

        private async Task Wipe(IPlayer player, string[] arguments)
        {
            try
            {
                var ph = player.GetPlayerHandler();

                if (ph.StaffRank <= AdminRank.Moderator)
                    return;

                var socialclub = arguments[0].ToString().ToLower();
                PlayerHandler phWipe = PlayerManager.GetPlayerBySCN(socialclub);

                if (phWipe != null && phWipe.Client != null && phWipe.Client.Exists)
                    await phWipe.Client.KickAsync("Wipe en cours...");

                phWipe = await PlayerManager.GetPlayerHandlerDatabase(socialclub);

                if (phWipe == null)
                    return;

                if (player.Exists && player != phWipe.Client)
                    player.SendChatMessage($"Wipe de {phWipe.Identite.Name} en cours");

                foreach (House house in HouseManager.Houses)
                {
                    if (house.Owner == phWipe.PID)
                    {
                        house.SetOwner(string.Empty);

                        if (house.Parking != null)
                            house.Parking.ListVehicleStored = new List<ParkedCar>();

                        house.UpdateInBackground();
                    }
                }

                IEnumerable<VehicleHandler> vehicles = VehiclesManager.GetAllVehicles().ToArray();

                foreach (VehicleHandler vehicle in vehicles)
                {
                    if (vehicle.OwnerID != phWipe.PID)
                        continue;

                    foreach (Parking parking in Parking.ParkingList)
                    {
                        bool saveNeeded = false;

                        foreach (ParkedCar parkedCar in parking.ListVehicleStored)
                        {
                            if (parkedCar.Plate == vehicle.Plate)
                            {
                                parking.ListVehicleStored.Remove(parkedCar);
                                saveNeeded = true;
                            }
                        }

                        if (saveNeeded)
                            await parking.OnSaveNeeded();
                    }

                    await vehicle.DeleteAsync(true);
                }

                foreach (var faction in FactionManager.FactionList)
                {
                    if (faction.FactionPlayerList.ContainsKey(phWipe.PID))
                    {
                        faction.FactionPlayerList.Remove(phWipe.PID, out FactionPlayer useless);
                        faction.UpdateInBackground();
                    }
                }

                foreach (var society in SocietyManager.SocietyList)
                {
                    if (society.Employees.ContainsKey(phWipe.PID))
                    {
                        society.Employees.TryRemove(phWipe.PID, out _);
                        society.UpdateInBackground();
                    }
                }

                foreach (var business in Loader.BusinessesManager.BusinessesList)
                {
                    if (business.Employees.ContainsKey(phWipe.PID))
                    {
                        business.Employees.Remove(phWipe.PID);
                        business.UpdateInBackground();
                    }
                }

                var result = await Database.MongoDB.Delete<PlayerHandler>("players", phWipe.PID);

                if (player.Exists && player != phWipe.Client && result.DeletedCount != 0)
                    player.SendChatMessage($"Wipe de {phWipe.Identite.Name} terminé!");
            }
            catch (Exception ex)
            {

                Alt.Server.LogError("(PlayerCommands.Wipe) " + ex.Message);
            }
        }

        public void AddItem(IPlayer player, string[] arguments = null)
        {
            try
            {
                PlayerHandler ph = player.GetPlayerHandler();

                if (ph?.StaffRank < AdminRank.Moderator)
                    return;

                string command = "";

                for (int i = 0; i < arguments.Length; i++)
                    command += $" {arguments[i]}";

                command = command.ToLower();

                string[] infos = command.Split(new[] { " x" }, StringSplitOptions.RemoveEmptyEntries);
                int number = (Convert.ToInt32(infos[1]) != 0) ? Convert.ToInt32(infos[1]) : 1;
                string itemName = infos[0].Remove(0, 1);
                Item item = LoadItem.ItemsList.Find(x => x.name.ToLower() == itemName);

                if (item != null && ph != null)
                {
                    if (ph.AddItem(item, number))
                        player.SendNotificationSuccess($"Vous avez ajouté {number} {item.name}");
                    else
                        player.SendNotificationError($"Vous n'avez pas la place dans votre inventaire pour {item.name}");
                }
                else
                    player.SendNotificationError("Item inconnu.");
            }
            catch (Exception ex)
            {
                player.SendNotification(ex.ToString());
            }
        }

        private void TpCoord(IPlayer player, string[] args)
        {
            try
            {
                if (player.GetPlayerHandler().StaffRank <= AdminRank.Player)
                    return;

                player.Position = new Position(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
            }
            catch
            {
                player.SendChatMessage("Erreur dans la saisie des coordonnées");
            }
        }

        public void Cls(IPlayer player, string[] args)
        {
            player.EmitLocked("EmptyChat");
        }

        public void Refuel(IPlayer player, string[] args)
        {
            if (!player.IsInVehicle)
            {
                player.DisplaySubtitle("Vous devez être dans un véhicule", 5000);
                return;
            }

            if (player.Vehicle.GetVehicleHandler() != null)
                player.Vehicle.GetVehicleHandler().Fuel = player.Vehicle.GetVehicleHandler().FuelMax;

            player.DisplaySubtitle("Essence restaurée", 5000);
        }

        public void SetFuel(IPlayer player, string[] args)
        {
            if (!player.IsInVehicle)
            {
                player.DisplaySubtitle("Vous devez être dans un véhicule", 5000);
                return;
            }

            if (player.Vehicle.GetVehicleHandler() != null && double.TryParse(args[0], out double fuel))
                player.Vehicle.GetVehicleHandler().Fuel = (float)Math.Min(fuel, player.Vehicle.GetVehicleHandler().FuelMax);

            player.SendNotificationSuccess("Quantité d'essence mise à jour");
        }

        public void Repair(IPlayer player, string[] args)
        {
            if (!player.IsInVehicle)
            {
                player.DisplaySubtitle("Vous devez être dans un véhicule", 5000);
                return;
            }

            player.Vehicle.GetVehicleHandler().Repair(player);
            player.DisplaySubtitle("Vehicule réparé", 5000);
        }

        private void Cloth(IPlayer player, object[] args)
        {
            player.SetCloth((Models.ClothSlot)Convert.ToInt32(args[0]), (int)args[1], (int)args[2], (int)args[3]);
        }

        public void Wheel(IPlayer player, string[] args)
        {
            if (!player.IsInVehicle)
            {
                player.DisplaySubtitle("Vous devez être dans un véhicule", 5000);
                return;
            }

            player.SendChatMessage($"Wheel health: {player.Vehicle.GetWheelHealth(0)}");
            player.SendChatMessage($"Wheel HasTire: {player.Vehicle.DoesWheelHasTire(0)}");
            player.SendChatMessage($"Wheel Burst: {player.Vehicle.IsWheelBurst(0)}");
        }

        public void DoorState(IPlayer player, string[] args)
        {
            if (!player.IsInVehicle)
            {
                player.DisplaySubtitle("Vous devez être dans un véhicule", 5000);
                return;
            }

            player.EmitLocked("SetDoorState", player.Vehicle, int.Parse(args[0]), int.Parse(args[1]), bool.Parse(args[2]));
        }

        public void NeonState(IPlayer player, string[] args)
        {
            if (!player.IsInVehicle)
            {
                player.DisplaySubtitle("Vous devez être dans un véhicule", 5000);
                return;
            }

            player.Vehicle.GetVehicleHandler().NeonState = new Tuple<bool, bool, bool, bool>(bool.Parse(args[0]), bool.Parse(args[0]), bool.Parse(args[0]), bool.Parse(args[0]));
        }
    }
}
