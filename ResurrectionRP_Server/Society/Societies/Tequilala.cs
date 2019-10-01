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
        public Tequilala(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }

        public List<Door> Doors { get; private set; }

        public override void Init()
        {
            

            var PortExt = Door.CreateDoor(993120320, new Vector3(-564.3921f, 276.5233f, 83.13618f), true);
            var PortInt = Door.CreateDoor(unchecked((int)3668283177), new Vector3(-560.3441f, 291.9776f, 82.17625f), true);
            var PortArr = Door.CreateDoor(993120320, new Vector3(-561.966f, 293.679f, 87.62682f), true);

            Doors = new List<Door>()
            {
                PortExt,
                PortInt,
                PortArr
            };

            foreach (var door in Doors)
                door.Interact = OpenCelluleDoor;

            base.Init();
        }

        private void OpenCelluleDoor(IPlayer client, Door door)
        {
            if (IsEmployee(client))
            {
                XMenu xmenu = new XMenu("ID_Door");
                xmenu.SetData("Door", door);

                XMenuItem item = new XMenuItem($"{((door.Locked) ? "Ouvrir" : "Fermer")} la porte", "", icon: (door.Locked) ? XMenuItemIcons.DOOR_CLOSED_SOLID : XMenuItemIcons.DOOR_OPEN_SOLID);
                item.OnMenuItemCallback = OnDoorCall;
                xmenu.Add(item);

                xmenu.OpenXMenu(client);
            }
        }

        private static void OnDoorCall(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            Door door = menu.GetData("Door");
            if (door != null)
            {
                door.SetDoorLockState(!door.Locked);
            }

            XMenuManager.XMenuManager.CloseMenu(client);
        }
    }
}
