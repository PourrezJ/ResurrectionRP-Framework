using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Vehicles;
using System.Linq;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Models.InventoryData.Items
{
    class GasJerrycan : Item
    {

        public GasJerrycan(ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "gasjerrycan", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
        }

        public override async Task Use(IPlayer client, string inventoryType, int slot)
        {
            var vehs = await MP.Vehicles.GetInRangeAsync(await client.GetPositionAsync(), 5f);
            VehicleHandler _vehicle = VehicleManager.GetHandlerByVehicle(vehs.FirstOrDefault());
            if (_vehicle != null)
            {
                _vehicle.VehicleSync.Fuel += 20;
                await client.SendNotificationSuccess("Vous avez remis de l'essence dans le véhicule");
            }
        }
    }
}
 