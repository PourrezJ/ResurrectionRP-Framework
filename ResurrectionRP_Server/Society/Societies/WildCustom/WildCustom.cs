﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net.Async;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Models;


namespace ResurrectionRP_Server.Society.Societies.WildCustom
{
    public partial class WildCustom : Society
    {
        #region Variables
        private Societies.Bennys.GarageType _garageType = Societies.Bennys.GarageType.Car;
        private IColShape _workZone;
        private IVehicle _vehicleBench;
        public List<Door> Doors { get; private set; }
        #endregion

        #region Constructor
        public WildCustom(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }
        #endregion

        #region Events
        private async Task OnInteractWithPnj(IPlayer client, Entities.Peds.Ped npc)
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

            if (colShape == _workZone)
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

            Location pnjPos = new Location(new Vector3(106.0419f, 6627.597f, 31.78723f), new Vector3(0, 0, 237.60875f));
            var npc = await Entities.Peds.Ped.CreateNPC(PedModel.Benny,Streamer.Data.PedType.Human, pnjPos.Pos, pnjPos.Rot.Z, (uint)GameMode.GlobalDimension);
            npc.NpcInteractCallBack = OnInteractWithPnj;

            Vector3 workZonePos = new Vector3(111.3728f, 6625.725f, 31.78725f);
            _workZone = Alt.CreateColShapeCylinder(new Vector3(workZonePos.X, workZonePos.Y, workZonePos.Z - 1), 10, 5);

            _garageType = Societies.Bennys.GarageType.Car;
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