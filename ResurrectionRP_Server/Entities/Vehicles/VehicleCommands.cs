using AltV.Net.Data;
using AltV.Net.Enums;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils.Enums;
using System.Numerics;
using System.Threading.Tasks;
using VehicleInfoLoader.Data;
using ResurrectionRP_Server.Database;

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
            VehicleData vehicleData = null;

            foreach (VehicleData veh in VehiclesManager.GetAllVehicles())
            {
                if (veh.Plate.ToUpper() == plate)
                {
                    vehicleData = veh;
                    break;
                }
            }

            if (vehicleData == null)
            {
                player.SendNotificationError("Véhicule inconnu");
                return;
            }

            PlayerHandler owner = await PlayerManager.GetPlayerHandlerDatabase(vehicleData.OwnerID);
            player.SendChatMessage($"Immatriculation : {vehicleData.Plate}");
            VehicleManifest manifest = VehicleInfoLoader.VehicleInfoLoader.Get(vehicleData.Model);
            player.SendChatMessage($"Modèle : {manifest.LocalizedName} ({vehicleData.Model})");

            if (owner != null)
                player.SendChatMessage($"Propriétaire : {owner.Identite.Name} ({vehicleData.OwnerID})");
            else
                player.SendChatMessage("Propriétaire : Aucun");

            player.SendChatMessage($"Verrouillé : {vehicleData.LockState.ToString()}");
            player.SendChatMessage($"Dernier chauffeur : {vehicleData.LastDriver}");
            player.SendChatMessage($"Dernier usage : {vehicleData.LastUse}");

            if (vehicleData.ParkingName != string.Empty)
                player.SendChatMessage($"Parking : {vehicleData.ParkingName}");

            Vector3 position = vehicleData.Location.Pos;
            //string dim = vehicleData.Dimension.ToString();
            player.SendChatMessage($"Position : X: {position.X}, Y: {position.Y}, Z: {position.Z}");
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
            VehicleHandler vehicle = VehiclesManager.GetVehicleDataWithPlate(plate)?.Vehicle;

            if (vehicle == null)
            {
                player.SendNotificationError("Véhicule inconnu");
                return;
            }
            else if (!vehicle.VehicleData.IsInPound && !vehicle.VehicleData.IsParked)
            {
                player.SendNotificationError("Véhicule déjà dans le monde");
                return;
            }
            //if (GameMode.Instance.PoundManager.PoundVehicleList.Find(c => c.Plate == vehicle.Plate) != null)
            else if (vehicle.VehicleData.IsInPound)
            {
                player.SendNotificationError("Véhicule à fourrière");
                return;
            }

            foreach (Parking parking in Parking.ParkingList)
            {
                if (parking.ListVehicleStored.Find(c => c.Plate == vehicle.VehicleData.Plate) != null)
                {
                    player.SendNotificationError("Véhicule dans un parking");
                    return;
                }
            }

            //vehicle.SpawnVehicle();
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
                player.SetPlayerIntoVehicle(vehicle);
                ph.ListVehicleKey.Add(new VehicleKey(manifest.DisplayName, vehicle.NumberplateText));
                //LogManager.Log($"~r~[ADMIN]~w~ {client.Name} a spawn le véhicule {_vehs.Model} {_vehs.Plate}");
            }
            else
                player.SendNotificationError("Il y a une erreur avec le véhicule demandé.");
        }
    }
}
