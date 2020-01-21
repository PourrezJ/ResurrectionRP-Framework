using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Models;

namespace ResurrectionRP_Server.Society.Societies
{
    public class BurgerShot : Society
    {
        public BurgerShot(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }
    }
}
