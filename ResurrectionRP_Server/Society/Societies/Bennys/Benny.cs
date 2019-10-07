using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.XMenuManager;
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
        public override void Init()
        {
            MenuBanner = Banner.SuperMod;
            Type = GarageType.Car;
            BlackListCategories = new int[] { 4, 9, 10, 13, 14, 15, 16, 17, 18, 19, 20, 21 };
            Data = new GarageData(_esthetiqueModList, _performanceModList);
            PnjLocation = new Location(new Vector3(-227.6015f, -1327.772f, 30.89038f), new Vector3(0, 0, 239.715f));
            WorkZonePosition = new Vector3(-222.3765f, -1329.64f, 30.46614f);

            GarageDoor = Door.CreateDoor(3867468406, new Vector3(-207.4542f, - 1310.315f, 30.74239f), true);
            GarageDoor.Interact = OpenGarageDoor;
            base.Init();
        }
        #endregion
        private void OpenGarageDoor(IPlayer client, Door door)
        {
            if (IsEmployee(client))
            {
                XMenu xmenu = new XMenu("ID_Door");
                xmenu.SetData("Door", door);

                XMenuItem item = new XMenuItem($"{((door.Locked) ? "Ouvrir" : "Fermer")} la porte de garage", "", icon: (door.Locked) ? XMenuItemIcons.DOOR_CLOSED_SOLID : XMenuItemIcons.DOOR_OPEN_SOLID);
                item.OnMenuItemCallback = OnDoorCall;
                xmenu.Add(item);

                xmenu.OpenXMenu(client);
            }
        }

        private static void OnDoorCall(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            try
            {

            Door door = menu.GetData("Door");
            if (door != null)
            {
                door.SetDoorLockState(!door.Locked);
            }

            }
            catch (System.Exception ex)
            { 

                throw ex;
            }
            XMenuManager.XMenuManager.CloseMenu(client);
        }
    }

}
