using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using System.Collections.Generic;
using System.Numerics;

namespace ResurrectionRP_Server.Society.Societies
{
    public class YellowJack : Society
    {
        #region Constructor
        public YellowJack(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }
        #endregion

        #region Init
        public override void Init()
        {
            Doors = new List<Door>() { Door.CreateDoor(4007304890, new Vector3(1991.106f, 3053.105f, 47.36529f), true) };
            Doors[0].Interact = OpenDoor;

            base.Init();
        }
        #endregion
    }
}
