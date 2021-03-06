﻿using ResurrectionRP_Server.Models;
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
        #region Constructor
        public BlackStreetNation(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }
        #endregion

        #region Init
        public override void Init()
        {
            List<TeleportEtage> etages = new List<TeleportEtage>()
            {
                new TeleportEtage() { Name = "Bar", Location = new Location(new Vector3(-1385.479f, -606.451f, 30.31957f), new Vector3(0, 0, 130.858f))},
                new TeleportEtage() { Name = "Sortie arrière", Location = new Location(new Vector3(-1368.322f, -647.4513f, 28.69429f), new Vector3(0, 0, 124.4904f))}
            };

            Teleport.Teleport.CreateTeleport(new Location(new Vector3(-1386.159f, -627.3551f, 30.81957f), new Vector3(0, 0, 309.1539f)), etages, new Vector3(1, 1, 0.2f), menutitle: "Porte");

            Doors = new List<Door>()
            {
                Door.CreateDoor(3478499199, new Vector3(-1387.809f, -586.5994f, 30.21479f), true),
                Door.CreateDoor(2182616413, new Vector3(-1388.825f, -587.3669f, 30.2216f), true)
            };

            foreach (Door door in Doors)
                door.Interact = OpenDoor;

            base.Init();
        }
        #endregion
    }
}
