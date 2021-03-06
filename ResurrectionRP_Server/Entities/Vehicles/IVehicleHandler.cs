﻿using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.XMenuManager;
using VehicleInfoLoader.Data;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public interface IVehicleHandler
    {
        VehicleHandler.PlayerEnterVehicle OnPlayerEnterVehicle { get; set; }
        VehicleHandler.PlayerQuitVehicle OnPlayerQuitVehicle { get; set; }
        IPlayer Owner { get; }
        bool SpawnVeh { get; set; }
        VehicleData VehicleData { get; set; }
        VehicleManifest VehicleManifest { get; set; }
        bool WasTeleported { get; set; }

        void AddFuel(float fuel);
        void ApplyDamage();
        Task<bool> DeleteAsync(bool perm = false);
        VehicleDoorState GetDoorState(VehicleDoor door);
        XMenuItemIconDesc GetXMenuIconDoor(VehicleDoorState state);
        bool HaveTowVehicle();
        bool IsLocked();
        bool LockUnlock(IPlayer client);
        void OpenXtremMenu(IPlayer client);
        Task<bool> RemoveInDatabase();
        void Repair(IPlayer player);
        void SetDoorState(IPlayer player, VehicleDoor door, VehicleDoorState state);
        void SetNeonState(bool state);
        void SetOwner(IPlayer player);
        void SetOwner(PlayerHandler player);
        void TowVehicle(IVehicle vehicle);
        IVehicle UnTowVehicle(Location position);
        void UpdateInBackground(bool updateLastUse = true, bool immediatePropertiesUpdate = false);
    }
}