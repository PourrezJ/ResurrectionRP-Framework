using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using ResurrectionRP_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net;
using AltV.Net.Async;
using ResurrectionRP_Server.Bank;
using ResurrectionRP_Server.EventHandlers;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Utils.Enums;

namespace ResurrectionRP_Server.Businesses
{
    [BsonKnownTypes(typeof(Market), typeof(TattoosStore), typeof(Barber.BarberStore), typeof(WeaponsShop), typeof(ClothingStore), typeof(PawnShop), typeof(DigitalDeen), typeof(PropsStore))]
    public partial class Business
    {

        #region Variables
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
            Task.Run(async () =>
            {
                BankAccount = new BankAccount(AccountType.Business, await BankAccount.GenerateNewAccountNumber(), 0);
                BankAccount.Owner = this;
            });
        }
        #endregion

        #region Loader
        public virtual Task Init()
        {
            if (PedHash != 0)
            {
                Ped ped = Ped.CreateNPC(PedHash, Location.Pos, Location.Rot.Z);
                ped.NpcInteractCallBack = OnNpcFirstInteract; // E
                ped.NpcSecInteractCallBack = OnNpcSecondaryInteract; // W
                Ped = ped;
            }

            Blip = Entities.Blips.BlipsManager.CreateBlip(BusinnessName, Location.Pos, (Owner == null || OnSale) ? 35 : 2, (int)BlipSprite);

            if (Employees == null)
                Employees = new Dictionary<string, string>();

            BankAccount.Owner = this;
            GameMode.Instance.BusinessesManager.BusinessesList.Add(this);
            return Task.CompletedTask;
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

        public static bool CanIHaveABusiness(string owner) => (GameMode.Instance.BusinessesManager.BusinessesList.Find(x => x.Owner == owner) == null || (PlayerManager.GetPlayerBySCN(owner)).StaffRank >= AdminRank.Moderator) ? true : false;
        #endregion

        #region Events
        public async Task OnNpcFirstInteract(IPlayer client, Ped npc = null)
        {
            await OpenMenu(client, npc);
        }

        public async Task OnNpcSecondaryInteract(IPlayer client, Ped npc = null)
        {
            Menu menu = new Menu("ID_SellMenu", BusinnessName, "Administration du magasin", backCloseMenu: true);
            await OpenSellMenu(client, menu);
        }

        public virtual Task OpenMenu(IPlayer client, Ped npc) => Task.CompletedTask;
        #endregion
    }
}
