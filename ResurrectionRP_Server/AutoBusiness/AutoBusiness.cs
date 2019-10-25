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
using System.Linq;
using ResurrectionRP_Server.Entities.Peds;

namespace ResurrectionRP_Server.AutoBusiness
{
    public static class AutoBusinessManager
    {
        #region Static fields
        public static List<AutoBusiness> AutoBusinessList = new List<AutoBusiness>();
        #endregion

        #region Init
        public static void InitAll()
        {
            var validator_type = typeof(AutoBusiness);

            var sub_validator_types =
                validator_type
                .Assembly
                .DefinedTypes
                .Where(x => validator_type.IsAssignableFrom(x) && x != validator_type)
                .ToList();


            foreach (var sub_validator_type in sub_validator_types)
            {
                if (Activator.CreateInstance(sub_validator_type) is AutoBusiness farm)
                {
                    farm.Init();
                    AutoBusinessList.Add(farm);
                }
            }
        }
        #endregion

    }
    public partial class AutoBusiness
    {

        public string name = "Not Defined";
        public string desc = "Not Defined";

        public Vector3 pedPosition;
        public float pedHeading;
        public PedModel pedModel;

        public int blipSprite = 0;
        public BlipColor blipColor;
        public Blip blip = null;

        private Entities.Peds.Ped ped;

        public ConcurrentDictionary<Item, int> sellItems = new ConcurrentDictionary<Item, int>();
        public ConcurrentDictionary<Item, int> buyItems = new ConcurrentDictionary<Item, int>();
        public ConcurrentDictionary<ItemStack, ItemStack> tradeItems = new ConcurrentDictionary<ItemStack, ItemStack>();
        


        public void Init()
        {

            this.ped = Entities.Peds.Ped.CreateNPC(pedModel, pedPosition, pedHeading);
            ped.NpcInteractCallBack = PrimaryInteract;
            ped.NpcSecInteractCallBack = SecondaryInteract;

            if (blipSprite != 0)
                BlipsManager.CreateBlip(name, pedPosition, blipColor, blipSprite, 1, true);
        }

        protected virtual void PrimaryInteract(IPlayer client, Ped npc) =>
            OpenMainMenu(client);

        protected virtual void SecondaryInteract(IPlayer client, Ped npc) =>
            OpenMainMenu(client);

    }
}
