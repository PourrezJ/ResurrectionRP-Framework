using ResurrectionRP_Server.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Teleport;
using ResurrectionRP_Server.XMenuManager;

namespace ResurrectionRP_Server.Society.Societies
{
    public class BlackStreetNation : Society
    {
        public BlackStreetNation(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }

        public Door PorteDevant1 { get; set; }
        public Door PorteDevant2 { get; set; }

        public override async Task Init()
        {
            PorteDevant1 = Door.CreateDoor(3478499199, new Vector3(-1387.809f, -586.5994f, 30.21479f), true);
            PorteDevant2 = Door.CreateDoor(2182616413, new Vector3(-1388.825f, -587.3669f, 30.2216f), true);

            List<TeleportEtage> etages = new List<TeleportEtage>()
            {
                new TeleportEtage() { Name = "Bar", Location = new Location(new Vector3(-1385.479f, -606.451f, 30.31957f), new Vector3(0, 0, 130.858f))},
                new TeleportEtage() { Name = "Sortie arrière", Location = new Location(new Vector3(-1368.322f, -647.4513f, 28.69429f), new Vector3(0, 0, 124.4904f))}
            };

            Teleport.Teleport.CreateTeleport(new Location(new Vector3(-1386.159f, -627.3551f, 30.81957f), new Vector3(0, 0, 309.1539f)), etages, menutitle: "Porte");

            PorteDevant1.Interact = OpenDoor;
            PorteDevant2.Interact = OpenDoor;

            ServicePos = new Vector3(-1390.743f, -600.2302f, 30.31958f);

            await base.Init();
        }

        #region Doors
        private async Task OpenDoor(IPlayer client, Door door)
        {
            if (this.IsEmployee(client) || Owner == client.GetSocialClub())
            {
                XMenu xmenu = new XMenu("ID_Door");
                xmenu.SetData("Door", door);

                XMenuItem item = new XMenuItem($"{((door.Locked) ? "Ouvrir" : "Fermer")} la porte", "", icon: (door.Locked) ? XMenuItemIcons.DOOR_CLOSED_SOLID : XMenuItemIcons.DOOR_OPEN_SOLID);
                item.OnMenuItemCallback = OnDoorCall;
                xmenu.Add(item);

                await xmenu.OpenXMenu(client);
            }
        }

        private static async Task OnDoorCall(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            Door door = menu.GetData("Door");
            if (door != null)
            {
                door.SetDoorLockState(!door.Locked);
            }

            await XMenuManager.XMenuManager.CloseMenu(client);
        }
        #endregion
    }
}
