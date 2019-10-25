using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using AltV;
using AltV.Net;
using AltV.Net.Enums;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using System.Numerics;

namespace ResurrectionRP_Server.AutoBusiness
{
    public partial class AutoBusiness
    {

        public string name = "Not Defined";
        public string desc = "Not Defined";

        public int BlipSprite = 0;
        public Blip Blip = null;

        private Entities.Peds.Ped Ped;

        public ConcurrentDictionary<Item, int> sellItems = null;
        public ConcurrentDictionary<Item, int> buyItems = null;
        public ConcurrentDictionary<Item, int> tradeItems = null;
        



        public AutoBusiness(string name, string desc, Vector3 pedPosition, float pedHeading,PedModel pedModel, int blipSprite = 0, BlipColor blipColor = BlipColor.LightGreen2)
        {
            this.name = name;
            this.desc = desc;

            this.Ped = Entities.Peds.Ped.CreateNPC(pedModel, pedPosition, pedHeading);

            if (blipSprite != 0)
                BlipsManager.CreateBlip(name, pedPosition, blipColor, blipSprite, 1, true);
        }

    }
}
