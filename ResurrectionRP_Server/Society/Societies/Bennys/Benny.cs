using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using System.Collections.Generic;
using System.Numerics;

namespace ResurrectionRP_Server.Society.Societies.Bennys
{
    public partial class Bennys : Garage
    {
        #region Constructor
        public Bennys(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }
        #endregion

        #region Init
        public override void Init()
        {
            MenuBanner = Banner.SuperMod;
            Type = GarageType.Car;
            BlackListCategories = new int[] { 4, 9, 10, 13, 14, 15, 16, 17, 18, 19, 20, 21 };
            Data = new GarageData(_esthetiqueModList, _performanceModList);
            PnjLocation = new Location(new Vector3(-227.6015f, -1327.772f, 30.89038f), new Vector3(0, 0, 239.715f));
            WorkZonePosition = new Vector3(-222.3765f, -1329.64f, 30.46614f);

            Doors = new List<Door>() { Door.CreateDoor(3867468406, new Vector3(-207.4542f, -1310.315f, 30.74239f), true) };
            Doors[0].Interact = OpenDoor;

            base.Init();
        }
        #endregion
    }
}
