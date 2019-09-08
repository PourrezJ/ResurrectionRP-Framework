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
    public partial class WhiteWereWolf : Society
    {
        #region Variables
        private GarageType _garageType = GarageType.Bike;
        private IColShape workZone;
        private IVehicle _vehicleBench;
        public List<Door> Doors { get; private set; }
        #endregion

        #region Constructor
        public WhiteWereWolf(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }
        #endregion

        #region Events
        private async Task OnInteractWithPnj(IPlayer client, Entities.Peds. Ped npc)
        {
            if (_vehicleBench != null)
                await OpenMainMenu(client, _vehicleBench);
            else
                await client.SendNotificationError("Aucun véhicule devant l'établi.");
        }

        public override async Task OnEntityEnterColShape(IColShape colShape, IPlayer client)
        {
            if (!client.Exists)
                return;

            if (colShape == workZone)
            {
                if (await client.IsInVehicleAsync())
                    _vehicleBench = await client.GetVehicleAsync();
            }

            await base.OnEntityEnterColShape(colShape, client);
        }
        #endregion

        #region Methods
        public override async Task Load()
        {
            await base.Load();

            var PortInt = await Door.CreateDoor(747286790, new Vector3(984.9756f, -94.93642f, 74.84788f), true);
            var PortExt = await Door.CreateDoor(190770132, new Vector3(981.4236f, -102.6262f, 74.84506f), true);

            Doors = new List<Door>()
            {
                PortExt,
                PortInt
            };

            foreach (var door in Doors)
                door.Interact = OpenCelluleDoor;

            Location pnjPos = new Location(new Vector3(974.9861f, -111.0525f, 74.35313f), new Vector3(0, 0, 239.715f));
            var npc = await Entities.Peds. Ped.CreateNPC(PedModel.Benny,Streamer.Data.PedType.Human ,pnjPos.Pos, pnjPos.Rot.Z, (uint)GameMode.GlobalDimension);
            npc.NpcInteractCallBack = OnInteractWithPnj;

            Vector3 workZonePos = new Vector3(970.89f, -115.2172f, 74.35314f);
            workZone = Alt.CreateColShapeCylinder(new Vector3(workZonePos.X, workZonePos.Y, workZonePos.Z - 1), 10, 5);


            _garageType = GarageType.Bike;
        }

        private async Task OpenCelluleDoor(IPlayer client, Door door)
        {
            if (await IsEmployee(client))
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

        private async Task PreviewKlaxon(IVehicle vehicle)
        {
            foreach (IPlayer client in vehicle.GetPlayersInRange(8f))
            {
                if (!client.Exists)
                    continue;

                await client.EmitAsync("VehicleSync_KlaxonPreview", vehicle.Id);
            }
        }

        private async Task StopKlaxon(IVehicle vehicle)
        {
            foreach (IPlayer client in vehicle.GetPlayersInRange(8f))
            {
                if (!client.Exists)
                    continue;

                await client.EmitAsync("VehicleSync_KlaxonPreview", vehicle.Id, false);
            }
        }
        #endregion
    }
}
