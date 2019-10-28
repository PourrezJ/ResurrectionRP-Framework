using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.XMenuManager;

namespace ResurrectionRP_Server.Society.Societies
{
    public class Tequilala : Society
    {
        #region Constructor
        public Tequilala(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }
        #endregion

        #region Init
        public override void Init()
        {
            Doors = new List<Door>()
            {
                Door.CreateDoor(993120320, new Vector3(-564.3921f, 276.5233f, 83.13618f), true),
                Door.CreateDoor(993120320, new Vector3(-561.966f, 293.679f, 87.62682f), true)
            };

            foreach (var door in Doors)
                door.Interact = OpenDoor;

            base.Init();
        }
        #endregion
    }
}
