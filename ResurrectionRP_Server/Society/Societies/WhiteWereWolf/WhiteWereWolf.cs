using System.Collections.Generic;
using System.Numerics;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;

namespace ResurrectionRP_Server.Society.Societies.WhiteWereWolf
{
    public partial class WhiteWereWolf : Garage
    {
        #region Constructor
        public WhiteWereWolf(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }
        #endregion

        #region Init
        public override void Init()
        {
            MenuBanner = Banner.ClubHouseMod;
            Type = GarageType.Bike;
            BlackListCategories = new int[] { 10, 13, 14, 15, 16, 17, 18, 19, 20, 21 };
            Data = new GarageData(_esthetiqueModList, _performanceModList);
            PnjLocation = new Location(new Vector3(974.9861f, -111.0525f, 74.35313f), new Vector3(0, 0, 239.715f));
            WorkZonePosition = new Vector3(970.89f, -115.2172f, 74.35314f);

            Doors = new List<Door>()
            {
                Door.CreateDoor(747286790, new Vector3(984.9756f, -94.93642f, 74.84788f), true),
                Door.CreateDoor(190770132, new Vector3(981.4236f, -102.6262f, 74.84506f), true)
            };

            foreach (Door door in Doors)
                door.Interact = OpenDoor;

            base.Init();
        }
        #endregion
    }
}
