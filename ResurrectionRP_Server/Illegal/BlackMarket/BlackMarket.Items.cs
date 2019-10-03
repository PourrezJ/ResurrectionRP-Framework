using AltV.Net.Data;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server.Illegal
{
    public partial class BlackMarket
    {
        private static List<Models.Item> IllegalItems = new List<Models.Item>()
        {
            new Items.Weapons(ItemID.Weapon, "Double Action Revolver", "", 3, hash: WeaponHash.DoubleAction, itemPrice: 40000),
        };
    }
}
