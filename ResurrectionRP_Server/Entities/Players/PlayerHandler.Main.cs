﻿using System;
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
using RPGInventoryManager = ResurrectionRP_Server.Inventory.RPGInventoryManager;

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
        
        public Inventory.Inventory PocketInventory { get; set; } = new Inventory.Inventory(6, 4);

        [BsonIgnore]
        public Inventory.Inventory BagInventory { get; set; }

        public Inventory.OutfitInventory OutfitInventory { get; set; } = new Inventory.OutfitInventory();
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
        public Data.PlayerSync PlayerSync
        {
            get
            {
                if (playerSync == null)
                    playerSync = new Data.PlayerSync();
                return playerSync;
            }
            set => playerSync = value;
        }
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

                    OutfitInventory.Slots[11] = new ItemStack(new ClothItem(ItemID.Shoes, "Chaussure", "", new Models.ClothData((Character.Gender == 0) ? (byte)1 : (byte)3, 0, 0), 0, true, false, false, true, false, 0, classes: "shoes", icon: "shoes"), 1, 11);
                    OutfitInventory.Slots[9] = new ItemStack(new ClothItem(ItemID.Pant, "Pantalon", "", new Models.ClothData(0, 0, 0), 0, true, false, false, true, false, 0, classes: "pants", icon: "pants"), 1, 9);
                    OutfitInventory.Slots[5] = new ItemStack(new ClothItem(ItemID.Jacket, "Resurrection", "", new Models.ClothData(0, 0, 0), 0, true, false, false, true, false, 0, classes: "jacket", icon: "jacket"), 1, 9);
                    //OutfitInventory.Slots[13] = new ItemStack(new BagItem(ItemID.Bag, "Backpack", "", new Models.ClothData(1, 0, 0), new Inventory(25, 20, InventoryType.Bag),0, true, false, false, true, false, 0, classes: "backpack", icon: "backpack"), 1, 9);
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

        #region Misc
        public async Task UpdateHungerThirst(int hunger = -1, int thirst = -1)
        {
            if (!Client.Exists)
                return;
            Thirst = (thirst == -1) ? Thirst : thirst;
            Hunger = (hunger == -1) ? Hunger : hunger;
            await  Client?.EmitAsync("UpdateHungerThirst", Hunger, Thirst);
            await UpdatePlayerInfo();
        }
        #endregion


        #region Inventory
        public async Task UpdateClothing()
        {
            Clothing = new Models.Clothings(Client);

            for (int i = 0; i < OutfitInventory.Slots.Length; i++)
            {
                try
                {
                    ClothItem cloth = null;
                    var clothSlot = OutfitInventory.Slots[i];

                    if (OutfitInventory.Slots[i] != null && OutfitInventory.Slots[i].Item != null)
                        cloth = (OutfitInventory.Slots[i].Item) as ClothItem;

                    switch (i)
                    {
                        case 0: // glasses
                            Clothing.Glasses = (cloth != null) ? new Models.PropData(cloth.Clothing.Drawable, cloth.Clothing.Texture) : (Character.Gender == 0) ? new Models.PropData(14, 0) : new Models.PropData(13, 0);
                            break;

                        case 1: // cap
                            Clothing.Hats = (cloth != null) ? new Models.PropData(cloth.Clothing.Drawable, cloth.Clothing.Texture) : (Character.Gender == 0) ? new Models.PropData(121, 0) : new Models.PropData(120, 0);
                            break;

                        case 2: // necklace
                            Clothing.Accessory = (cloth != null) ? cloth.Clothing : new Models.ClothData();
                            break;

                        case 3: // mask
                            Clothing.Mask = (cloth != null) ? cloth.Clothing : new Models.ClothData();
                            break;

                        case 4: // earring
                            Clothing.Ears = (cloth != null) ? new Models.PropData(cloth.Clothing.Drawable, cloth.Clothing.Texture) : (Character.Gender == 0) ? new Models.PropData(33, 0) : new Models.PropData(12, 0);
                            break;

                        case 5: // jacket
                            if (cloth != null)
                            {
                                Clothing.Tops = cloth.Clothing;

                                int torso = 0;

                                if (Character.Gender == 0)
                                    torso = Loader.ClothingLoader.ClothingsMaleTopsList.DrawablesList[cloth.Clothing.Drawable].Torso[0];
                                else
                                    torso = Loader.ClothingLoader.ClothingsFemaleTopsList.DrawablesList[cloth.Clothing.Drawable].Torso[0];

                                Clothing.Torso = new Models.ClothData((byte)torso, 0, 0);
                            }
                            else
                            {
                                Clothing.Tops = new Models.ClothData(15, 0, 0);
                                Clothing.Torso = new Models.ClothData(15, 0, 0);
                            }
                            break;

                        case 6: // watch
                            if (cloth != null) Clothing.Watches = new Models.PropData(cloth.Clothing.Drawable, cloth.Clothing.Texture);
                            break;

                        case 7: // shirt
                            Clothing.Undershirt = (cloth != null) ? cloth.Clothing : new Models.ClothData(15, 0, 0);
                            break;

                        case 8: // bracelet
                            if (cloth != null) Clothing.Bracelets = new Models.PropData(cloth.Clothing.Drawable, cloth.Clothing.Texture);
                            break;

                        case 9: // pants
                            Clothing.Legs = (cloth != null) ? cloth.Clothing : (Character.Gender == 0) ? new Models.ClothData(14, 0, 0) : new Models.ClothData(15, 0, 0);
                            break;

                        case 10: // gloves
                            /*
                            Clothing.Torso = (cloth != null) ? cloth.Clothing : (Character.Gender == 0) ?
                                    new Models.ClothData((byte)ClothingLoader.ClothingsMaleTopsList.DrawablesList[Clothing.Tops.Drawable].Torso[0], 0, 0) :
                                    new Models.ClothData((byte)ClothingLoader.ClothingsFemaleTopsList.DrawablesList[Clothing.Tops.Drawable].Torso[0], 0, 0);*/
                            break;

                        case 11: // shoes
                            Clothing.Feet = (cloth != null) ? cloth.Clothing : (Character.Gender == 0) ? new Models.ClothData(34, 0, 0) : new Models.ClothData(35, 0, 0);
                            break;

                        case 12: // kevlar
                            Clothing.BodyArmor = (cloth != null) ? cloth.Clothing : new Models.ClothData();
                            break;

                        case 13: // backpack
                            if (clothSlot?.Item != null)
                            {
                                var backpack = (clothSlot.Item) as Items.BagItem;
                                if (backpack.InventoryBag != null)
                                {
                                    BagInventory = backpack.InventoryBag;
                                    Clothing.Bags = backpack.Clothing;
                                }
                            }
                            else
                            {
                                BagInventory = null;
                                Clothing.Bags = new Models.ClothData();
                            }
                            break;

                        case 14: // phone
                            if (clothSlot?.Item != null)
                            {
                                var phone = clothSlot.Item as Items.PhoneItem;
                                if (phone != null)
                                    PhoneSelected = phone.PhoneHandler;
                            }
                            break;

                        case 15:
                            if (clothSlot?.Item != null)
                            {
                                var radio = clothSlot.Item as RadioItem;
                                if (radio != null)
                                    RadioSelected = radio.Radio;
                                else
                                    RadioSelected = null;
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Client.GetData("SocialClub", out string social);
                    Alt.Server.LogError($"UpdateClothing ID: {i} player: {social}" +ex);
                }
            }
            await Clothing.UpdatePlayerClothing();
        }

        public bool PocketIsFull() => PocketInventory.IsFull();

        public bool InventoryIsFull(double itemSize = 0)
        {
            if (BagInventory != null)
            {
                if (BagInventory.IsFull(itemSize) && PocketInventory.IsFull(itemSize)) return true;
                else return false;
            }
            else
            {
                if (PocketInventory.IsFull(itemSize)) return true;
                else return false;
            }
        }

        public async Task<bool> AddItem(Models.Item item, int quantity = 1)
        {

            if (PocketInventory.AddItem(item, quantity))
            {
                if (RPGInventoryManager.HasInventoryOpen(this.Client))
                {
                    var rpg = RPGInventoryManager.GetRPGInventory(this.Client);
                    if (rpg != null)
                        RPGInventoryManager.Refresh(this.Client, rpg);
                }
                await item.OnPlayerGetItem(this.Client);

                return true;
            }
            else if (BagInventory != null && BagInventory.AddItem(item, quantity))
            {
                if (RPGInventoryManager.HasInventoryOpen(this.Client))
                {
                    var rpg = RPGInventoryManager.GetRPGInventory(this.Client);
                    if (rpg != null)
                        RPGInventoryManager.Refresh(this.Client, rpg);
                }
                await item.OnPlayerGetItem(this.Client);
                return true;
            }
            else return false;
        }

        public bool HasItemID(Models.InventoryData.ItemID id)
        {
            if (PocketInventory.HasItemID(id)) return true;
            else if (BagInventory != null && BagInventory.HasItemID(id)) return true;
            else return false;
        }

        public List<Models.ItemStack> GetAllItems()
        {

            List<Models.ItemStack> _stacks = new List<Models.ItemStack>();

            foreach (Models.ItemStack stack in PocketInventory.InventoryList)
            {
                _stacks.Add(stack);
            }

            if (BagInventory != null)
            {
                foreach (Models.ItemStack stack in BagInventory.InventoryList)
                {
                    _stacks.Add(stack);
                }
            }

            return _stacks;
        }

        public bool DeleteItem(int slot, string inventoryType, int quantity)
        {
            switch (inventoryType)
            {
                case Utils.Enums.InventoryTypes.Pocket:
                    return PocketInventory.Delete(slot, quantity);

                case Utils.Enums.InventoryTypes.Bag:
                    return BagInventory.Delete(slot, quantity);

                case Utils.Enums.InventoryTypes.Outfit:
                    return OutfitInventory.Delete(slot, quantity);
            }
            return false;
        }

        public bool DeleteOneItemWithID(Models.InventoryData.ItemID itemID)
        {
            if (PocketInventory.DeleteAll(itemID, 1) == 1)
                return true;
            if (BagInventory != null && BagInventory.DeleteAll(itemID, 1) == 1)
                return true;
            if (OutfitInventory != null && OutfitInventory.Delete(itemID, 1))
                return true;
            return false;
        }

        public bool DeleteAllItem(Models.InventoryData.ItemID itemID, int quantite = 1)
        {
            int pocketCount = PocketInventory.CountItem(itemID);
            var bagCount = 0;
            if (BagInventory != null)
                bagCount = BagInventory.CountItem(itemID);


            if (pocketCount + bagCount >= quantite)
            {
                if (pocketCount >= quantite)
                    PocketInventory.DeleteAll(itemID, quantite);
                else
                {
                    PocketInventory.DeleteAll(itemID, pocketCount);
                    if (BagInventory != null)
                        BagInventory.DeleteAll(itemID, quantite - pocketCount);
                }

                return true;
            }
            return false;
        }

        public int CountItem(Models.InventoryData.ItemID itemid)
        {
            int somme = 0;
            somme += PocketInventory.CountItem(itemid);

            if (BagInventory != null)
            {
                somme += BagInventory.CountItem(itemid);
            }

            return somme;
        }

        public int CountItem(Models.Item item)
        {
            int somme = 0;
            somme += PocketInventory.CountItem(item);

            if (BagInventory != null)
            {
                somme += BagInventory.CountItem(item);
            }

            return somme;
        }

        public Dictionary<string, Models.ItemStack[]> GetStacksItems(Models.InventoryData.ItemID itemID)
        {
            Dictionary<string, Models.ItemStack[]> items = new Dictionary<string, Models.ItemStack[]>();

            var pocket = PocketInventory.FindAllItemWithType(itemID);
            if (pocket != null && pocket.Length > 0)
                items.Add(Utils.Enums.InventoryTypes.Pocket, pocket);

            if (BagInventory != null)
            {
                var bag = BagInventory.FindAllItemWithType(itemID);
                if (bag != null && bag.Length > 0)
                    items.Add(Utils.Enums.InventoryTypes.Bag, bag);
            }

            if (OutfitInventory != null)
            {
                var outfit = OutfitInventory.FindAllItemWithType(itemID);
                if (outfit != null && outfit.Length > 0)
                    items.Add(Utils.Enums.InventoryTypes.Outfit, outfit);
            }
            return items;
        }

        #endregion
    }
}
