using AltV.Net;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using ResurrectionRP_Server.XMenuManager;
using System.Threading.Tasks;
namespace ResurrectionRP_Server.Society.Societies
{
    public class Unicorn : Society
    {
        // private Teleport.Teleport Teleport;
        public Door Principal = null;
        public Door Bureau1 = null;
        public Door Bureau2 = null;

        public Unicorn(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Models.Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }

        public override async Task Init()
        {
            Principal = Door.CreateDoor(-1116041313, new Vector3(128.7443f, -1298.621f, 29.23274f), true);
            Bureau1 = Door.CreateDoor(668467214, new Vector3(95.84587f, -1285.645f, 29.26877f), true);
            Bureau2 = Door.CreateDoor(-626684119, new Vector3(99.5032f, -1292.784f, 29.26877f), true);

            List<Teleport.TeleportEtage> etages = new List<Teleport.TeleportEtage>()
            {
                new Teleport.TeleportEtage() { Name = "Sorti arrière", Location = new Location(new Vector3(132.4223f, -1287.319f, 29.27378f), new Vector3(0, 0, 28.89318f))}
            };
            //v759 Impossible de créer un teleporteur ??? Erreur c#
            //Teleport = await Teleport.CreateTeleport(new Location(new Vector3(133.1923f, -1293.724f, 29.26952f), new Vector3(0, 0, 121.3312f)), etages, menutitle: "Porte");


            Principal.Interact = OpenDoor;
            Bureau1.Interact = OpenDoor;
            Bureau2.Interact = OpenDoor;

            ServicePos = new Vector3(93.30592f, -1292.083f, 29.26874f);

            await base.Init();
        }

        #region Doors
        private void OpenDoor(IPlayer client, Door door)
        {
            if (this.IsEmployee(client) || Owner == client.GetSocialClub())
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
        #endregion
    }
}
