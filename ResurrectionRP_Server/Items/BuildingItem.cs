using AltV.Net.Elements.Entities;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Items
{
    class BuildingItem : Models.Item
    {
        public int Hash = 0;

        public BuildingItem(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "building", string icon = "unknown-item", string classes = "basic", int modelhash = 0) : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            Hash = modelhash;
        }

        public override Task Give(IPlayer sender, IPlayer recever, int quantite)
        {
            return base.Give(sender, recever, quantite);
        }

        public override async Task Use(IPlayer Client, string inventoryType, int slot)
        {
            //await MenuManager.CloseMenu(Client);
            if (Hash != 0)
            {
                //await BuildingManager.CreateBuilding(Client, Hash, new Location(Vector3Extensions.Backward(new Vector3(Client.Position.X, Client.Position.Y, Client.Position.Z - 1f), Client.Rotation.Z, 3f), Client.Rotation), id);
                //PlayerManager.GetPlayerByClient(Client)?.PocketInventory?.Delete(slot);
            }
        }
    }
}
