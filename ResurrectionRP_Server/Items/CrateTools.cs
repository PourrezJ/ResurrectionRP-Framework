using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Models;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Items
{
    class CrateTools : Item
    {
        public CrateTools(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, int itemPrice = 0, bool isDockable = false, string type = "crateTool", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
        }

        public static Task RepairVehicle(IPlayer player, IVehicle vehicle)
        {

            return Task.CompletedTask;
/*            if (!player.Exists || !vehicle.Exists)
                return;

            var vehicleHandler = vehicle.GetVehicleHandler();
            if (vehicleHandler == null)
                return;

            await player.NotifyAsync("Appuyer sur la touche du numpad 0 pour arrêter l'action.");
            player.SetData("VehicleRepair", 0);
            var rot = vehicle.Rotation;
            var pos = (vehicle.GetVehicleVector()).Forward(rot.Yaw, 2.9f);
            player.SetPosition(pos.X, pos.Y, pos.Z);
 
            player.SetRotation(rot + new System.Numerics.Vector3(0,0,180));
            //await vehicleHandler.SetDoorState(Entities.Vehicles.DoorID.DoorHood, Entities.Vehicles.DoorState.DoorOpen);

            player.GetPlayerHandler()?.PlayAnimation("mini@repair", "fixing_a_ped", 4, -8, -1, (Flags.Loop | Flags.AllowPlayerControl));
            player.Emit("LaunchProgressBar", 60000);
            player.Emit("togglePlayerControl", true);
            await Task.Delay(60000);
            player.Emit("togglePlayerControl", false);
            await player.StopAnimationAsync();
            if (!player.HasData("VehicleRepair"))
            {
                await StopRepair(player);
                return;
            }

            if (vehicleHandler.VehicleSync.EngineHealth < 0)
                vehicleHandler.VehicleSync.EngineHealth = 100;
            else if (vehicleHandler.VehicleSync.EngineHealth > 0 && vehicleHandler.VehicleSync.EngineHealth < 100)
                vehicleHandler.VehicleSync.EngineHealth += 100;

            if (vehicleHandler.VehicleSync.BodyHealth < 0)
                vehicleHandler.VehicleSync.BodyHealth = 200;
            else if (vehicleHandler.VehicleSync.BodyHealth > 0 && vehicleHandler.VehicleSync.BodyHealth < 200)
                vehicleHandler.VehicleSync.BodyHealth += 200;

            await vehicleHandler.UpdateSync();
            await vehicleHandler.Update();
            await player.SendNotificationSuccess("Vous avez réparé le moteur du véhicule provisoirement");*/
        }
/*
        public static async Task StopRepair(IPlayer player)
        {
            await player.StopAnimationAsync();
            await player.CallAsync("StopProgressBar");
            player.ResetData("VehicleRepair");
            await player.Freeze(false);
        }*/
    }
}
