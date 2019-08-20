using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Items
{
    class GasJerrycan : Models.Item
    {

        public GasJerrycan(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "gasjerrycan", string icon = "unknown-item", string classes = "basic") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
        }

        public override async Task Use(IPlayer client, string inventoryType, int slot)
        {
            var vehs = client.GetVehiclesInRange(5);
            Entities.Vehicles.VehicleHandler _vehicle = Entities.Vehicles.VehiclesManager.GetHandlerByVehicle(vehs.FirstOrDefault());
            if (_vehicle != null)
            {
                _vehicle.VehicleSync.Fuel += 20;
                await client.SendNotificationSuccess("Vous avez remis de l'essence dans le véhicule");
            }
        }
    }
}
