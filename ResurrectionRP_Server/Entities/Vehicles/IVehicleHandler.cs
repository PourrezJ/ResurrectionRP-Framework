using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Database;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.XMenuManager;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public interface IVehicleHandler
    {
        VehicleHandler.PlayerEnterVehicle OnPlayerEnterVehicle { get; set; }
        VehicleHandler.PlayerQuitVehicle OnPlayerQuitVehicle { get; set; }
        IPlayer Owner { get; }
        bool SpawnVeh { get; set; }
        VehicleData VehicleData { get; set; }
        bool WasTeleported { get; set; }

        void AddFuel(float fuel);
        void ApplyDamage();
        Task<bool> DeleteAsync(bool perm = false);
        VehicleDoorState GetDoorState(VehicleDoor door);
        XMenuItemIconDesc GetXMenuIconDoor(VehicleDoorState state);
        bool HaveTowVehicle();
        void InsertVehicle();
        bool IsLocked();
        bool LockUnlock(IPlayer client);
        void OpenXtremMenu(IPlayer client);
        Task<bool> RemoveInDatabase();
        void Repair(IPlayer player);
        void SetDoorState(IPlayer player, VehicleDoor door, VehicleDoorState state);
        void SetNeonState(bool state);
        void SetOwner(IPlayer player);
        void SetOwner(PlayerHandler player);
        Task TowVehicle(IVehicle vehicle);
        Task<IVehicle> UnTowVehicle(Location position);
        void UpdateInBackground(bool updateLastUse = true, bool immediatePropertiesUpdate = false);
    }
}