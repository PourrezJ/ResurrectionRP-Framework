using ResurrectionRP_Server.Models;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Society.Societies.WildCustom
{
    public partial class WildCustom : Garage
    {
        #region Constructor
        public WildCustom(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }
        #endregion

        #region Init
        public override void Init()
        {
            MenuBanner = Banner.Garage;
            Type = GarageType.Car;
            BlackListCategories = new int[] { 5, 6, 7, 10, 13, 14, 15, 16, 17, 18, 19, 20, 21 };
            Data = new GarageData(_esthetiqueModList, _performanceModList);
            PnjLocation = new Location(new Vector3(106.0419f, 6627.597f, 31.78723f), new Vector3(0, 0, 237.60875f));
            WorkZonePosition = new Vector3(111.3728f, 6625.725f, 31.78725f);

            base.Init();
        }
        #endregion
    }
}
