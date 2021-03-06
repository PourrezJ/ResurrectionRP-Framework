﻿using AltV.Net.Elements.Entities;
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

        public RadioItem()
        {
        }

        public override void Use(IPlayer c, string inventoryType, int slot)
        {
            RadLib.RadioManager.OpenRadio(c, Radio);
            MenuManager.CloseMenu(c);
        }

        public override void OnPlayerGetItem(IPlayer player)
        {
            Radio.Owner = player;
            base.OnPlayerGetItem(player);
        }
    }
}