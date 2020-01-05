using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP.Entities.Vehicles.Data;
using ResurrectionRP_Server.Models;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public partial class VehicleHandler : Vehicle
    {
        public VehicleHandler(IntPtr nativePointer, ushort id) : base(nativePointer, id)
        {
        }

        public VehicleHandler(uint model, Position position, Rotation rotation) : base(model, position, rotation)
        {
        }
        #region Methods
        public VehicleDoorState GetDoorState(VehicleDoor door) => VehicleData.Doors[(byte)door];

        public void SetDoorState(IPlayer player, VehicleDoor door, VehicleDoorState state)
        {
            VehicleData.Doors[(byte)door] = state;

            if (!Exists)
                return;

            this.SetDoorStateFix(player, door, state, false);
            //Vehicle.SetDoorState(door, state);
        }

        public bool HaveTowVehicle() => VehicleData.TowTruck != null;

        public async Task TowVehicle(IVehicle vehicle)
        {
            if (await this.GetModelAsync() != (int)VehicleModel.Flatbed && !HaveTowVehicle())
                return;

            VehicleData.TowTruck = new TowTruck(vehicle.NumberplateText, new Vector3(0, -2, 1), vehicle, this);
            UpdateInBackground();
        }

        public async Task<IVehicle> UnTowVehicle(Location position)
        {
            if (Model != (int)VehicleModel.Flatbed && VehicleData.TowTruck == null) return null;

            VehicleHandler vehicle = VehiclesManager.GetVehicleByPlate(VehicleData.TowTruck.VehPlate) as VehicleHandler;
            VehicleData.TowTruck = null;

            await vehicle.SetPositionAsync(position.Pos);
            await vehicle.SetRotationAsync(position.Rot);
            vehicle.UpdateInBackground(false);
            UpdateInBackground();
            return vehicle;
        }

        public void SetNeonState(bool state)
        {
            VehicleData.NeonState = new Tuple<bool, bool, bool, bool>(state, state, state, state);
        }


        #endregion
    }
}
