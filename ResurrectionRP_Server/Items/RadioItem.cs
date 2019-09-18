using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Models;
using RadLib = ResurrectionRP_Server.Radio;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Items
{
    public class RadioItem : Item
    {
        private RadLib.Radio _radio = null;
        public RadLib.Radio Radio
        {
            get
            {
                if (_radio == null) _radio = new RadLib.Radio();
                return _radio;
            }
            set => _radio = value;
        }

        public RadioItem(Models.InventoryData.ItemID id, string name, string description, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "radio", string icon = "talky", string classes = "radio") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
        }

        public override async Task Use(IPlayer c, string inventoryType, int slot)
        {
            await RadLib.RadioManager.OpenRadio(c, Radio);
            await MenuManager.CloseMenu(c);
        }

        public override Task OnPlayerGetItem(IPlayer player)
        {
            Radio.Owner = player;
            return base.OnPlayerGetItem(player);
        }
    }
}