using AlternateLife.RageMP.Net.Interfaces;
using System.Threading.Tasks;
using Flags = ResurrectionRP.Server.AnimationFlags;

namespace ResurrectionRP.Server
{
    class CrateTools : Item
    {
        public CrateTools(ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, int itemPrice = 0, bool isDockable = false, string type = "crateTool", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
        }

        public static async Task RepairVehicle(IPlayer player, IVehicle vehicle)
        {
            if (!player.Exists || !vehicle.Exists)
                return;

            var vehicleHandler = vehicle.GetVehicleHandler();
            if (vehicleHandler == null)
                return;

            await player.NotifyAsync("Appuyer sur la touche du numpad 0 pour arrêter l'action.");
            player.SetData("VehicleRepair", 0);
            var rot = await vehicle.GetRotationAsync();
            var pos = (await vehicle.GetPositionAsync()).Forward(rot.Z, 2.9f);
            await player.SetPositionAsync(pos);
 
            await player.SetRotationAsync(rot + new System.Numerics.Vector3(0,0,180));
            await vehicleHandler.SetDoorState(DoorID.DoorHood, DoorState.DoorOpen);

            player.GetPlayerHandler()?.PlayAnimation("mini@repair", "fixing_a_ped", 4, -8, -1, (Flags.Loop | Flags.AllowPlayerControl));
            await player.CallAsync("LaunchProgressBar", 60000);
            await player.Freeze(true);
            await Task.Delay(60000);
            await player.Freeze(false);
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
            await player.SendNotificationSuccess("Vous avez réparé le moteur du véhicule provisoirement");
        }

        public static async Task StopRepair(IPlayer player)
        {
            await player.StopAnimationAsync();
            await player.CallAsync("StopProgressBar");
            player.ResetData("VehicleRepair");
            await player.Freeze(false);
        }
    }
}
