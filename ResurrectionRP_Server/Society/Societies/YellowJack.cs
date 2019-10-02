using AltV.Net.Elements.Entities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.XMenuManager;


namespace ResurrectionRP_Server.Society.Societies
{
    public class YellowJack : Society
    {
        public YellowJack(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }

        public List<Door> Doors { get; private set; }

        public override void Init()
        {
            var port = Door.CreateDoor(4007304890, new Vector3(1991.106f, 3053.105f, 47.36529f), true);
            port.Interact = OpenCelluleDoor;

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
