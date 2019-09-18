using AltV.Net;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Models;
using Flags = ResurrectionRP_Server.Utils.Enums.AnimationFlags;
using System.Linq;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Items
{
    public class LockPick : Item
    {
        public LockPick(Models.InventoryData.ItemID id, string name, string description, double weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, double itemPrice = 0, string type = "item", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
        }
        /**
        public static async Task LockPickVehicle(IPlayer player, IVehicle vehicle, Inventory.Inventory inventory)
        {
            if (player.HasData("VehicleLockPicking"))
            {
                await StopLockPicking(player);
                return;
            }

            if ((await vehicle.GetPositionAsync()).DistanceTo2D(await player.GetPositionAsync()) > 2f)
            {
                await player.SendNotificationError("Vous êtes trop loin du véhicule");
                return;
            }
            await player.NotifyAsync("Appuyer sur la touche du numpad 0 pour arrêter l'action.");
            
            player.SetData("VehicleLockPicking", 0);
            player.GetPlayerHandler()?.PlayAnimation("anim@amb@clubhouse@tutorial@bkr_tut_ig3@", "machinic_loop_mechandplayer", 4, -8, -1, (Flags.Loop | Flags.AllowPlayerControl));
            await player.CallAsync("LaunchProgressBar", 60000);
            await player.Freeze(true);
            await Task.Delay(60000);
            await player.Freeze(false);
            if (!player.HasData("VehicleLockPicking"))
            {
                await StopLockPicking(player);
                return;
            }

            if (Utils.RandomNumber(0,4) == 0)
            {
                await player.SendNotificationSuccess("Vous avez réussi a ouvrir le véhicule");
                await vehicle.GetVehicleHandler()?.LockUnlock(player);

            }
            else
            {
                await vehicle.GetVehicleHandler().StartAlarm();
                await player.SendNotificationError("Vous avez cassez votre lock-pick");
                inventory.DeleteAll(ItemID.LockPick, 1);
            }
            await StopLockPicking(player);
        }

        public static async Task StopLockPicking(IPlayer player)
        {
            await player.StopAnimationAsync();
            await player.CallAsync("StopProgressBar");
            player.ResetData("VehicleLockPicking");
            await player.Freeze(false);
        }**/
    }
}
