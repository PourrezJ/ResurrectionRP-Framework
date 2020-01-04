using AltV.Net.Data;
using AltV.Net.Enums;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils.Enums;
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
            Chat.RegisterCmd("vehiclespawn", VehicleSpawn);
            Chat.RegisterCmd("vehicle", Vehicle);
        }

        public static async Task VehicleInfo(IPlayer player, string[] args)
        {
            if (player == null || !player.Exists)
                return;

            PlayerHandler ph = player.GetPlayerHandler();

            if (ph == null || player.GetPlayerHandler().StaffRank < StaffRank.Moderator)
                return;

            if (args == null || args.Length == 0)
            {
                player.SendNotificationError("Vous devez indiquer la plaque d'immatriculation du véhicule");
                return;
            }

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
            string dim = vehicle.Dimension.ToString();
            player.SendChatMessage($"Position : X: {position.X}, Y: {position.Y}, Z: {position.Z}, Dim: {dim}");
        }

        private void VehicleSpawn(IPlayer player, string[] args)
        {
            if (player == null || !player.Exists)
                return;

            PlayerHandler ph = player.GetPlayerHandler();

            if (ph == null || player.GetPlayerHandler().StaffRank <= StaffRank.Player)
                return;

            if (args == null || args.Length == 0)
            {
                player.SendNotificationError("Vous devez indiquer la plaque d'immatriculation du véhicule");
                return;
            }

            string plate = args[0].ToUpper();
            VehicleHandler vehicle = VehiclesManager.GetVehicleHandler(plate);

            if (vehicle == null)
            {
                player.SendNotificationError("Véhicule inconnu");
                return;
            }
            else if (!vehicle.IsInPound && !vehicle.IsParked)
            {
                player.SendNotificationError("Véhicule déjà dans le monde");
                return;
            }
            //if (GameMode.Instance.PoundManager.PoundVehicleList.Find(c => c.Plate == vehicle.Plate) != null)
            else if (vehicle.IsInPound)
            {
                player.SendNotificationError("Véhicule à fourrière");
                return;
            }

            foreach (Parking parking in Parking.ParkingList)
            {
                if (parking.ListVehicleStored.Find(c => c.Plate == vehicle.Plate) != null)
                {
                    player.SendNotificationError("Véhicule dans un parking");
                    return;
                }
            }

            vehicle.SpawnVehicle();
            player.SendNotificationSuccess("Véhicule spawn");
        }

        private void Vehicle(IPlayer player, string[] args)
        {
            if (player == null || !player.Exists)
                return;

            PlayerHandler ph = player.GetPlayerHandler();

            if (ph == null || player.GetPlayerHandler().StaffRank <= StaffRank.Player)
                return;

            if (args == null || args.Length == 0)
            {
                player.SendNotificationError("Vous devez indiquer le hash du véhicule");
                return;
            }

            if (!uint.TryParse(args[0].ToString(), out uint hash))
            {
                player.SendNotificationError("Hash invalide");
                return;
            }

            VehicleManifest manifest = VehicleInfoLoader.VehicleInfoLoader.Get(hash);

            if (manifest == null)
            {
                player.SendNotificationError($"véhicule inconnu : {hash}");
                return;
            }

            Rotation rot = player.Rotation;
            VehicleHandler vehicle = VehiclesManager.SpawnVehicle(player.GetSocialClub(), hash, player.Position, new Rotation(rot.Pitch, rot.Roll, -rot.Yaw), fuel: 100, fuelMax: 100, spawnVeh: true);

            if (vehicle != null)
            {
                vehicle.LockState = VehicleLockState.Unlocked;
                player.SetPlayerIntoVehicle(vehicle.Vehicle);
                ph.ListVehicleKey.Add(new VehicleKey(manifest.DisplayName, vehicle.Plate));
                //LogManager.Log($"~r~[ADMIN]~w~ {client.Name} a spawn le véhicule {_vehs.Model} {_vehs.Plate}");
            }
            else
                player.SendNotificationError("Il y a une erreur avec le véhicule demandé.");
        }
    }
}
