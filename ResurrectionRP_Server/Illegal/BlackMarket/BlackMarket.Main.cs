﻿using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Models;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ResurrectionRP_Server.Illegal
{
    public partial class BlackMarket : IllegalSystem
    {
        #region Field
        public DateTime NextRefreshMarketPos;
        public int CurrentLocation;
        private readonly Location[] _pnjPos = new Location[2]
        {
            new Location(new Vector3(-151.0022f, 933.9956f, 235.69006f), new Vector3(0,0,0)),
            new Location(new Vector3(-156.0022f, 925.74f, 235.65006f), new Vector3(0,0,0))
        };

        public Ped BlackMPed { get; private set; }

        public List<string> WeedLabsOwned = new List<string>();
        public List<string> WeedDealerOwned = new List<string>();
        #endregion

        #region CTOR
        public BlackMarket()
        {

        }
        #endregion

        public override void Load()
        {
            if (NextRefreshMarketPos < DateTime.Now || NextRefreshMarketPos == new DateTime())
            {
                NextRefreshMarketPos = DateTime.UtcNow.AddDays(7);
                CurrentLocation = Utils.Utils.RandomNumber(0, _pnjPos.Length);
            }

            if (_pnjPos.Length > 0)
            {
                var loc = _pnjPos[CurrentLocation];

                BlackMPed = Ped.CreateNPC(AltV.Net.Enums.PedModel.WeaponExpertMale01, loc.Pos, loc.Rot.Z, GameMode.GlobalDimension);
                AltV.Net.Alt.Server.LogInfo($"Current Black Market pos: {loc.Pos.X} {loc.Pos.Y} {loc.Pos.Z}");
                BlackMPed.NpcInteractCallBack += OnBlackMInteract;
            }

            Utils.Utils.SetInterval(() =>
            {
                // Check si milicien dans la zone
                // GOTO ResetLabs ou ResetDealer
            }, 30000);
            base.Load();
        }

        public override void OnPlayerConnected(IPlayer client)
        {
            if (WeedLabsOwned.Contains(client.GetSocialClub()))
            {
                if (IllegalManager.WeedBusiness.LabEnter != null)
                    client.CreateBlip(140, IllegalManager.WeedBusiness.LabEnter.Pos, "Laboratoire de Canabis", 1, 25, 255, true);
            }

            if (WeedDealerOwned.Contains(client.GetSocialClub()))
            {
                if (IllegalManager.WeedBusiness.DealerLocations != null)
                    client.CreateBlip(140, IllegalManager.WeedBusiness.DealerLocations[IllegalManager.WeedBusiness.CurrentPos].Pos, "Dealer de Canabis", 1, 25, 255, true);
            }

            base.OnPlayerConnected(client);
        }

        public override void OnPlayerDisconnected(IPlayer client)
        {
            base.OnPlayerDisconnected(client);
        }

        //public void ResetLabs()
        //{

        //}

        //public void ResetDealer()
        //{

        //}
    }
}
