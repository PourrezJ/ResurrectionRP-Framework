using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Models;
using AltV.Net.Enums;
using AltV.Net;
using AltV.Net.Async;

namespace ResurrectionRP_Server.Society.Societies.Bennys
{
    public partial class Bennys : Society
    {
        #region Variables
        private GarageType _garageType = GarageType.Car;
        private IColShape _workZone;
        private IVehicle _vehicleBench;
        #endregion

        #region Constructor
        public Bennys(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Models.Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }
        #endregion

        #region Events
        private async Task OnInteractWithPnj(IPlayer client, Entities.Peds.Ped npc)
        {
            if (_vehicleBench != null)
                await OpenMainMenu(client, _vehicleBench);
            else
                client.SendNotificationError("Aucun véhicule devant l'établi.");
        }

        public override async Task OnPlayerEnterColshape(IColShape colShape, IPlayer client)
        {
            if (!client.Exists)
                return;

            if (colShape == _workZone)
            {
                if (client.IsInVehicle)
                    _vehicleBench = client.Vehicle;
            }

            await base.OnPlayerEnterColshape(colShape, client);
        }
        #endregion

        #region Methods
        public override async Task Load()
        {
            Location pnjPos = new Location(new Vector3(-227.6015f, -1327.772f, 30.89038f), new Vector3(0, 0, 239.715f));
            var npc = Entities.Peds.Ped.CreateNPC(PedModel.Benny,Streamer.Data.PedType.Human ,pnjPos.Pos, pnjPos.Rot.Z, (uint)GameMode.GlobalDimension);
            npc.NpcInteractCallBack = OnInteractWithPnj;

            Vector3 workZonePos = new Vector3(-222.3765f, -1329.64f, 30.46614f);
            _workZone =  Alt.CreateColShapeCylinder(new Vector3(workZonePos.X, workZonePos.Y, workZonePos.Z - 1), 10, 5);
            _garageType = GarageType.Car;

            await base.Load();
        }

        private async Task PreviewKlaxon(IVehicle vehicle)
        {
            foreach (IPlayer client in vehicle.GetPlayersInRange(10f))
            {
                if (!client.Exists)
                    continue;

                await client.EmitAsync("VehicleSync_KlaxonPreview", vehicle.Id);
            }
        }

        private async Task StopKlaxon(IVehicle vehicle)
        {
            foreach (IPlayer client in vehicle.GetPlayersInRange(58f))
            {
                if (!client.Exists)
                    continue;

                await client.EmitAsync("VehicleSync_KlaxonPreview", vehicle.Id, false);
            }
        }
        #endregion
    }
}
