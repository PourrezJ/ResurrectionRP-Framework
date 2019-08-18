using System;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Utils.Extensions;

namespace ResurrectionRP_Server.Entities.Players
{


    [BsonIgnoreExtraElements]
    public partial class PlayerHandler
    {
        #region Private static properties
        public static ConcurrentDictionary<IPlayer, PlayerHandler> PlayerHandlerList = new ConcurrentDictionary<IPlayer, PlayerHandler>();
        #endregion

        #region Variables
        [BsonId]
        public string PID { get; set; }

        [BsonIgnore]
        public IPlayer Client { get; private set; }

        [BsonIgnore]
        public bool IsOnProgress { get; set; }

        [BsonIgnore]
        public DateTime LastUpdate { get; set; }

        private Utils.Enums.AdminRank _adminrank;
        public Utils.Enums.AdminRank StaffRank
        {
            get => _adminrank;
            set
            {
                _adminrank = value;
                Client?.EmitAsync("SetRank", _adminrank);
            }
        }
        public Models.Identite Identite { get; set; }
        public int TimeSpent { get; set; }
        
        public List<Models.VehicleKey> ListVehicleKey { get; private set; }
                = new List<Models.VehicleKey>();

        public List<Models.License> Licenses { get; set; }
            = new List<Models.License>();

        [BsonIgnore]
        public Models.Clothings Clothing { get; set; }
        public Models.Animation[] AnimSettings { get; set; }
            = new Models.Animation[9];
        public string IP { get; set; }

        [BsonIgnore]
        public bool IsOnline { get; set; }
            = false;

        public Models.Location Location { get; set; }
            = new Models.Location(new Vector3(), new Vector3()); // Default spawn

        public Models.PlayerCustomization Character { get; set; }
        /**
        public Inventory PocketInventory { get; set; } = new Inventory(6, 4);

        [BsonIgnore]
        public Inventory BagInventory { get; set; }

        public OutfitInventory OutfitInventory { get; set; } = new OutfitInventory();**/
        public double Money { get; private set; }
        public Bank.BankAccount BankAccount { get; set; }
        public int Hunger { get; set; } = 100;
        public int Thirst { get; set; } = 100;
        public bool Jailed { get; private set; } = false;

        private float _health = 100;
        public float Health
        {
            get => _health;
            set
            {
                AltAsync.Do(() =>
                {
                    if (Client != null)
                        Client.SetHealthAsync((ushort)value);
                });

                _health = value;
            }
        }
        private Data.PlayerSync playerSync = null;
        #endregion

        #region Constructor
        public PlayerHandler(IPlayer client)
        {
            Client = client;
            
            AltAsync.Do(() =>
           {
               client.GetData("SocialClub", out string PID);
               this.PID = PID;
           });
        }

        #endregion

        #region Method

        #region Load

        public async Task LoadPlayer(IPlayer client, bool firstspawn = false)
        {
            Client = client;
            client.SetData("PlayerHandler", this);
            if (PlayerHandlerList.TryAdd(client, this))
            {
                if (BankAccount == null) BankAccount = new Bank.BankAccount(Bank.AccountType.Personnal, await Bank.BankAccount.GenerateNewAccountNumber(), PlayerManager.StartBankMoney);

                if (firstspawn)
                {
                    /**
                    PocketInventory.AddItem(Inventory.ItemByID(ItemID.JambonBeurre), 1);
                    PocketInventory.AddItem(Inventory.ItemByID(ItemID.Eau), 1);

                    OutfitInventory.Slots[11] = new ItemStack(new ClothItem(ItemID.Shoes, "Chaussure", "", new ClothData((Character.Gender == 0) ? (byte)1 : (byte)3, 0, 0), 0, true, false, false, true, false, 0, classes: "shoes", icon: "shoes"), 1, 11);
                    OutfitInventory.Slots[9] = new ItemStack(new ClothItem(ItemID.Pant, "Pantalon", "", new ClothData(0, 0, 0), 0, true, false, false, true, false, 0, classes: "pants", icon: "pants"), 1, 9);
                    OutfitInventory.Slots[5] = new ItemStack(new ClothItem(ItemID.Jacket, "Resurrection", "", new ClothData(0, 0, 0), 0, true, false, false, true, false, 0, classes: "jacket", icon: "jacket"), 1, 9);
                    //OutfitInventory.Slots[13] = new ItemStack(new BagItem(ItemID.Bag, "Backpack", "", new ClothData(1, 0, 0), new Inventory(25, 20, InventoryType.Bag),0, true, false, false, true, false, 0, classes: "backpack", icon: "backpack"), 1, 9);
                    **/
                    await AddMoney(PlayerManager.StartMoney);

                    Location = GameMode.FirstSpawn;
                }

                /*var inventoriesPhones = this.GetStacksItems(ItemID.Phone);

                if (inventoriesPhones.Count > 0)
                {
                    foreach (var stacks in inventoriesPhones)
                    {
                        foreach (var phone in stacks.Value)
                        {
                            var phoneItem = phone.Item as PhoneItem;
                            if (phoneItem != null)
                                Phone.AddPhoneInList(Client, phoneItem.PhoneHandler);
                        }
                    }
                }**/

                await AltAsync.Do( () =>
                {
                    this.IP = Client.Ip;
                    this.IsOnline = true;
                    
                    Client.Emit
                    (
                        Utils.Enums.Events.PlayerInitialised,
                        StaffRank,
                        Identite.Name,
                        Convert.ToSingle(Money),
                        Thirst,
                        Hunger,
                        JsonConvert.SerializeObject(AnimSettings),
                        JsonConvert.SerializeObject(GameMode.Instance.Time),
                        0,//GameMode.Instance.WeatherManager.Actual_weather,
                        0,//GameMode.Instance.WeatherManager.Wind,
                        0,//GameMode.Instance.WeatherManager.WindDirection,
                        GameMode.Instance.IsDebug,
                        JsonConvert.SerializeObject(Location)
                    );

                    Client.Spawn(Location.Pos, 0);
                    Character.ApplyCharacter(Client);
                    Client.Dimension = GameMode.Instance.GlobalDimension;
                    Client.Health = (ushort)(Health + 100);
                    Client.Emit("FadeIn", 0);
                });

               // await UpdateClothing();

                /**if (PlayerSync.IsCuff)
                    await SetCuff(true);

                PlayerSync.IsDead = (Health <= 0);

                await GameMode.Instance.HouseManager.OnPlayerConnected(client);
                await GameMode.Instance.DoorManager.OnPlayerConnected(client);
                await GameMode.Instance.PedManager.OnPlayerConnected(client);
                await GameMode.Instance.VoiceController.OnPlayerConnected(client);
                await GameMode.Instance.IllegalManager.OnPlayerConnected(client);
                **/
                await Task.Delay(500);

                if (firstspawn)
                    await Save();

                OnKeyPressed += OnKeyPressedCallback;
            }
            else
            {
                //await client.SendNotificationError("Erreur avec votre personnage.");
                //await client.FadeIn(0);
                await client.KickAsync("Une erreur s'est produite");
            }
        }
        #endregion

        #region Money
        public async Task AddMoney(double somme)
        {
            if (somme < 0) return;
            Money += somme;
            Client?.Emit(Utils.Enums.Events.UpdateMoneyHUD, Convert.ToSingle(Money));
            await UpdatePlayerInfo();
        }

        #endregion

        #endregion

        #region Inventory


        #endregion

        #region Methods

        #endregion
    }
}
