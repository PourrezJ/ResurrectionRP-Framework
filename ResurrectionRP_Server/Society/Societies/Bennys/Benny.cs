using ResurrectionRP_Server.Models;
using System.Numerics;
using System.Threading.Tasks;

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
        public override async Task Init()
        {
            MenuBanner = Banner.SuperMod;
            Type = GarageType.Car;
            BlackListCategories = new int[] { 4, 9, 10, 13, 14, 15, 16, 17, 18, 19, 20, 21 };
            Data = new GarageData(_esthetiqueModList, _performanceModList);
            PnjLocation = new Location(new Vector3(-227.6015f, -1327.772f, 30.89038f), new Vector3(0, 0, 239.715f));
            WorkZonePosition = new Vector3(-222.3765f, -1329.64f, 30.46614f);

            await base.Init();
        }
        #endregion
    }
}
