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

        #region Event handlers
        private void OnInteractWithPnj(IPlayer client, Entities.Peds. Ped npc)
        {
            if (_vehicleBench != null)
                OpenMainMenu(client, _vehicleBench);
            else
                client.SendNotificationError("Aucun véhicule devant l'établi.");
        }

        public override async void OnPlayerEnterServiceColshape(IColShape colShape, IPlayer client)
        {
            if (!client.Exists)
                return;

            if (colShape == workZone)
            {
                if (await client.IsInVehicleAsync())
                    _vehicleBench = await client.GetVehicleAsync();
            }

            base.OnPlayerEnterServiceColshape(colShape, client);
        }
        #endregion

        #region Methods
        public override async Task Init()
        {
            await base.Init();

            var PortInt = Door.CreateDoor(747286790, new Vector3(984.9756f, -94.93642f, 74.84788f), true);
            var PortExt = Door.CreateDoor(190770132, new Vector3(981.4236f, -102.6262f, 74.84506f), true);

            Doors = new List<Door>()
            {
                PortExt,
                PortInt
            };

            foreach (var door in Doors)
                door.Interact = OpenCelluleDoor;

            Location pnjPos = new Location(new Vector3(974.9861f, -111.0525f, 74.35313f), new Vector3(0, 0, 239.715f));
            var npc = Entities.Peds.Ped.CreateNPC(PedModel.Benny, pnjPos.Pos, pnjPos.Rot.Z, GameMode.GlobalDimension);
            npc.NpcInteractCallBack = OnInteractWithPnj;

            Vector3 workZonePos = new Vector3(970.89f, -115.2172f, 74.35314f);
            workZone = Alt.CreateColShapeCylinder(new Vector3(workZonePos.X, workZonePos.Y, workZonePos.Z - 1), 10, 5);


            _garageType = GarageType.Bike;
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

        private void HornPreview(IVehicle vehicle, byte horn)
        {
            foreach (IPlayer client in vehicle.GetPlayersInRange(10f))
            {
                if (!client.Exists)
                    continue;

                client.EmitLocked("HornPreview", vehicle, horn - 1, true);
            }
        }

        private void HornStop(IVehicle vehicle, byte horn)
        {
            foreach (IPlayer client in vehicle.GetPlayersInRange(58f))
            {
                if (!client.Exists)
                    continue;

                client.EmitLocked("HornPreview", vehicle, horn - 1, false);
            }
        }
        #endregion
    }
}
