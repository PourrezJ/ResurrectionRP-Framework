﻿using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using ResurrectionRP_Server.Bank;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Business
{
    [BsonKnownTypes(typeof(Market), typeof(TattoosStore), typeof(Barber.BarberStore), typeof(WeaponsShop), typeof(ClothingStore), typeof(PawnShop), typeof(DigitalDeen), typeof(PropsStore), typeof(HeadShop))]
    public partial class Business
    {
        #region Fields
        [JsonIgnore]
        public BsonObjectId _id;

        public string BusinnessName = "Undefined Name";
        public Location Location;

        public string Owner { get; set; } = null;

        [BsonIgnore, JsonIgnore]
        public Entities.Blips.Blips Blip;
        public uint BlipSprite { get; private set; }


        [BsonIgnore, JsonIgnore]
        protected Ped Ped;

        [BsonRepresentation(BsonType.Int64, AllowOverflow = true)]
        public PedModel PedHash { get; private set; }

        public int MaxEmployee { get; set; } = 5;
        public bool CanEmploy = false;
        public bool Buyable = false; // Commerce achetable?
        public bool OnSale = false;
        public Dictionary<string, string> Employees = new Dictionary<string, string>();
        public BankAccount BankAccount;
        public int BusinessPrice = 150000;
        public bool Resell = false;
        public DateTime Inactivity = DateTime.Now;
        public Inventory.Inventory Inventory = new Inventory.Inventory(500, 40);
        #endregion

        #region Constructor
        public Business(string businnessName, Location location, uint blipSprite, int inventoryMax, PedModel pedhash = 0, string owner = null, bool buyable = true, bool onsale = true)
        {
            BusinnessName = businnessName;
            Location = location;
            BlipSprite = blipSprite;
            PedHash = pedhash;
            Owner = owner;
            Buyable = buyable;
            OnSale = onsale;

            Inventory = new Inventory.Inventory(inventoryMax, 40);
            BankAccount = new BankAccount(AccountType.Business, BankAccount.GenerateNewAccountNumber(), 0);
            BankAccount.Owner = this;
        }
        #endregion

        #region Loader
        public virtual void Init()
        {
            if (PedHash != 0)
            {
                Ped ped = Ped.CreateNPC(PedHash, Location.Pos, Location.Rot.Z);
                ped.NpcInteractCallBack = OnNpcFirstInteract; // E
                ped.NpcSecInteractCallBack = OnNpcSecondaryInteract; // W
                Ped = ped;
            }
            Blip = Entities.Blips.BlipsManager.CreateBlip(((!OnSale && Owner != null) || !Buyable) ? BusinnessName : "[A VENDRE] " + BusinnessName, Location.Pos, ((Owner == null || OnSale) && Buyable) ? 35 : 2, (int)BlipSprite);
            BankAccount.Owner = this;
            Loader.BusinessesManager.BusinessesList.Add(this);
        }
        #endregion

        #region Methods
        public bool HasOwner()
        {
            if (Owner == null || string.IsNullOrEmpty(Owner) || Owner == "")
                return false;
            else
                return true;
        }

        public bool IsEmployee(IPlayer client)
            => Employees.ContainsKey (client.GetSocialClub());

        public bool IsOwner(IPlayer client)
            => client.GetSocialClub() == Owner;

        public static bool CanIHaveABusiness(string owner) => (Loader.BusinessesManager.BusinessesList.Find(x => x.Owner == owner) == null || (PlayerManager.GetPlayerBySCN(owner)).StaffRank >= StaffRank.Moderator) ? true : false;
        #endregion

        #region Events
        public void OnNpcFirstInteract(IPlayer client, Ped npc = null)
        {
            OpenMenu(client, npc);
        }

        public void OnNpcSecondaryInteract(IPlayer client, Ped npc = null)
        {
            Menu menu = new Menu("ID_SellMenu", BusinnessName, "Administration du magasin", backCloseMenu: true);
            OpenSellMenu(client, menu);
        }

        public virtual void OpenMenu(IPlayer client, Ped npc) { }
        #endregion
    }
}
