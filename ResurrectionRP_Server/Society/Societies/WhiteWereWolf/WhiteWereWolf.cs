using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using AltV.Net.Enums;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.XMenuManager;
using ResurrectionRP_Server.Society.Societies.Bennys;
using ResurrectionRP_Server.Utils;

namespace ResurrectionRP_Server.Society.Societies.WhiteWereWolf
{
    public partial class WhiteWereWolf : Garage
    {
        #region Fields
        private List<Door> _doors;
        #endregion

        #region Constructor
        public WhiteWereWolf(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }
        #endregion

        #region Init
        public override async Task Init()
        {
            MenuBanner = Banner.ClubHouseMod;
            Type = GarageType.Bike;
            BlackListCategories = new int[] { 10, 13, 14, 15, 16, 17, 18, 19, 20, 21 };
            Data = new GarageData(_esthetiqueModList, _performanceModList);
            PnjLocation = new Location(new Vector3(974.9861f, -111.0525f, 74.35313f), new Vector3(0, 0, 239.715f));
            WorkZonePosition = new Vector3(970.89f, -115.2172f, 74.35314f);

            var PortInt = Door.CreateDoor(747286790, new Vector3(984.9756f, -94.93642f, 74.84788f), true);
            var PortExt = Door.CreateDoor(190770132, new Vector3(981.4236f, -102.6262f, 74.84506f), true);

            _doors = new List<Door>()
            {
                PortExt,
                PortInt
            };

            foreach (Door door in _doors)
                door.Interact = OpenDoor;

            await base.Init();
        }
        #endregion

        #region Methods
        private void OpenDoor(IPlayer client, Door door)
        {
            if (IsEmployee(client))
            {
                XMenu xmenu = new XMenu("ID_Door");
                xmenu.SetData("Door", door);

                XMenuItem item = new XMenuItem($"{(door.Locked ? "Ouvrir" : "Fermer")} la porte", "", icon: door.Locked ? XMenuItemIcons.DOOR_CLOSED_SOLID : XMenuItemIcons.DOOR_OPEN_SOLID);
                item.OnMenuItemCallback = DoorCallback;
                xmenu.Add(item);

                xmenu.OpenXMenu(client);
            }
        }

        private static void DoorCallback(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            Door door = menu.GetData("Door");

            if (door != null)
                 door.SetDoorLockState(!door.Locked);

            XMenuManager.XMenuManager.CloseMenu(client);
        }
        #endregion
    }
}
