using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils.Enums;
using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using SaltyServer;
using ResurrectionRP_Server.Entities.Players.Data;

namespace ResurrectionRP_Server.Entities.Players
{
    [BsonIgnoreExtraElements]
    public partial class PlayerHandler
    {
        #region Private static properties
        public static ConcurrentDictionary<IPlayer, PlayerHandler> PlayerHandlerList = new ConcurrentDictionary<IPlayer, PlayerHandler>();
        #endregion

        #region Fields and properties
        [BsonId]
        public string PID { get; set; }

        [BsonIgnore]
        public IPlayer Client { get; private set; }

        [BsonIgnore]
        public bool IsOnProgress { get; set; }

        [BsonIgnore]
        public DateTime LastUpdate { get; set; }

        [BsonIgnore]
        public VehicleHandler Vehicle { get; set; }

        private AdminRank _adminrank;

        public AdminRank StaffRank
        {
            get => _adminrank;
            set
            {
                _adminrank = value;

                if (Client != null && Client.Exists)
                    Client.EmitLocked("SetRank", _adminrank);
            }
        }
        public Identite Identite { get; set; }
        public int TimeSpent { get; set; }
        
        public List<VehicleKey> ListVehicleKey { get; private set; }
                = new List<VehicleKey>();

        public List<License> Licenses { get; set; }
            = new List<License>();

        [BsonIgnore]
        public Clothings Clothing { get; set; }
        public Animation[] AnimSettings { get; set; }
            = new Animation[9];
        public string IP { get; set; }

        [BsonIgnore]
        public bool IsOnline { get; set; }
            = false;

        public Location Location { get; set; }
            = new Location(new Vector3(), new Vector3()); // Default spawn

        public PlayerCustomization Character { get; set; }
        
        public Inventory.Inventory PocketInventory { get; set; } = new Inventory.Inventory(6, 4);

        [BsonIgnore]
        public Inventory.Inventory BagInventory { get; set; }

        public OutfitInventory OutfitInventory { get; set; } = new Inventory.OutfitInventory();
        public double Money { get; private set; }
        public Bank.BankAccount BankAccount { get; set; }
        public int Hunger { get; set; } = 100;
        public int Thirst { get; set; } = 100;
        public bool Jailed { get; private set; } = false;

        private ushort _health = 200;
        public ushort Health
        {
            get => _health;
            set
            {
                if (Client != null)
                    Client.Health = value;

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

        private Radio.Radio radioSelected;
        [BsonIgnore]
        public Radio.Radio RadioSelected
        {
            get
            {
                if (OutfitInventory.Slots[15] == null)
                    return null;

                var radio = OutfitInventory.Slots[15].Item as Items.RadioItem;
                if (radio != null)
                    return radio.Radio;
                return null;
            }
            set => radioSelected = value;
        }

        private Phone.Phone phoneSelected;
        [BsonIgnore]
        public Phone.Phone PhoneSelected
        {
            get
            {
                if (OutfitInventory.Slots[14] == null)
                    return null;

                var phone = OutfitInventory.Slots[14].Item as Items.PhoneItem;
                if (phone != null)
                    return phone.PhoneHandler;
                return null;
            }
            set => phoneSelected = value;
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

        #region Methods

        #region Load
        public async Task LoadPlayer(IPlayer client, bool firstspawn = false)
        {
            Client = client;
            client.SetData("PlayerHandler", this);

            if (PlayerHandlerList.TryAdd(client, this))
            {
                if (BankAccount == null)
                    BankAccount = new Bank.BankAccount(Bank.AccountType.Personal, await Bank.BankAccount.GenerateNewAccountNumber(), PlayerManager.StartBankMoney);
  
                BankAccount.Owner = this;

                if (firstspawn)
                {
                    PocketInventory.AddItem(Inventory.Inventory.ItemByID(Models.InventoryData.ItemID.JambonBeurre), 1);
                    PocketInventory.AddItem(Inventory.Inventory.ItemByID(Models.InventoryData.ItemID.Eau), 1);

                    OutfitInventory.Slots[11] = new ItemStack(new ClothItem(Models.InventoryData.ItemID.Shoes, "Chaussure", "", new ClothData((Character.Gender == 0) ? 1 : 3, 0, 0), 0, true, false, false, true, false, 0, classes: "shoes", icon: "shoes"), 1, 11);
                    OutfitInventory.Slots[9] = new ItemStack(new ClothItem(Models.InventoryData.ItemID.Pant, "Pantalon", "", new ClothData(0, 0, 0), 0, true, false, false, true, false, 0, classes: "pants", icon: "pants"), 1, 9);
                    OutfitInventory.Slots[5] = new ItemStack(new ClothItem(Models.InventoryData.ItemID.Jacket, "Resurrection", "", new ClothData(0, 0, 0), 0, true, false, false, true, false, 0, classes: "jacket", icon: "jacket"), 1, 9);
                    //OutfitInventory.Slots[13] = new ItemStack(new BagItem(ItemID.Bag, "Backpack", "", new ClothData(1, 0, 0), new Inventory(25, 20, InventoryType.Bag),0, true, false, false, true, false, 0, classes: "backpack", icon: "backpack"), 1, 9);
                    
                    AddMoney(PlayerManager.StartMoney);
                    Location = GameMode.FirstSpawn;
                }
                
                var inventoriesPhones = GetStacksItems(Models.InventoryData.ItemID.Phone);

                if (inventoriesPhones.Count > 0)
                {
                    foreach (var stacks in inventoriesPhones)
                    {
                        foreach (var phone in stacks.Value)
                        {
                            var phoneItem = phone.Item as Items.PhoneItem;
                            if (phoneItem != null)
                                Phone.Phone.AddPhoneInList(Client, phoneItem.PhoneHandler);
                        }
                    }
                }
                
                await AltAsync.Do( () =>
                {
                    IP = Client.Ip;
                    IsOnline = true;

                    Client.Emit
                    (
                        Events.PlayerInitialised,
                        (int)StaffRank,
                        Identite.Name,
                        Convert.ToSingle(Money),
                        Thirst,
                        Hunger,
                        JsonConvert.SerializeObject(GameMode.Instance.Time),
                        GameMode.Instance.WeatherManager.Actual_weather.ToString(),
                        GameMode.Instance.WeatherManager.Wind,
                        GameMode.Instance.WeatherManager.WindDirection,
                        GameMode.Instance.IsDebug,
                        Location.Pos.ConvertToVector3Serialized()
                    );

                    Client.Spawn(Location.Pos, 0);
                    Client.ApplyCharacter();
                    Client.Dimension = GameMode.GlobalDimension;

                    if (Health <= 100)
                    {
                        Client.Health = 0;
                        Client.Emit("ONU_PlayerDeath", WeaponHash.AdvancedRifle);

                        PlayerManager.DeadPlayers.Add(new DeadPlayer(Client, null, (uint)WeaponHash.AdvancedRifle));
                    }

                    Client.SetSyncedMetaData(SaltyShared.SharedData.Voice_TeamSpeakName, Voice.CreateTeamSpeakName());
                    Client.SetSyncedMetaData(SaltyShared.SharedData.Voice_VoiceRange, "Parler");

                    Client.SetSyncedMetaData("WalkingStyle", PlayerSync.WalkingAnim);
                    Client.SetSyncedMetaData("FacialAnim", PlayerSync.MoodAnim);
                    Client.SetSyncedMetaData("Crounch", PlayerSync.Crounch);

                    Client.Emit(SaltyShared.Event.Voice_Initialize, Voice.ServerUniqueIdentifier, Voice.RequiredUpdateBranch, Voice.MinimumPluginVersion, Voice.SoundPack, Voice.IngameChannel, Voice.IngameChannelPassword);
                    Client.Emit("FadeIn", 3000);

                    UpdateClothing();

                    if (PlayerSync.IsCuff)
                        SetCuff(true);

                    GameMode.Instance.Streamer.LoadStreamPlayer(client);
                    GameMode.Instance.DoorManager.OnPlayerConnected(client);
                    Houses.HouseManager.OnPlayerConnected(client);
                });
                
                
                //await GameMode.Instance.IllegalManager.OnPlayerConnected(client);

                await Task.Delay(500);

                if (firstspawn)
                    UpdateFull();

                OnKeyPressedAsync += OnKeyPressedCallbackAsync;
                OnKeyReleasedAsync += OnKeyReleasedCallbackAsync;

                OnKeyPressed += OnKeyPressedCallback;
            }
            else
            {
                client.SendNotificationError("Erreur avec votre personnage.");
                //await client.FadeIn(0);
                await client.KickAsync("Une erreur s'est produite");
            }
        }

        #endregion

        #region Money
        public void AddMoney(double somme)
        {
            if (somme < 0)
                return;

            Money += somme;

            if (Client != null && Client.Exists)
                Client.EmitLocked(Events.UpdateMoneyHUD, Convert.ToSingle(Money));

            UpdateFull();
        }

        public bool HasMoney(double somme)
        {
            if (somme < 0)
                return false;

            if (Money >= somme)
            {
                Money -= somme;

                if (Client != null && Client.Exists)
                    Client.EmitLocked(Events.UpdateMoneyHUD, Convert.ToSingle(Money));

                UpdateFull();
                return true;
            }

            return false;
        }

        public async Task<bool> HasBankMoney(double somme, string reason)
        {
            if (somme < 0)
                return false;

            if (BankAccount.Balance >= somme)
            {
                await BankAccount.GetBankMoney(somme, reason);
                return true;
            }

            return false;
        }
        #endregion

        #endregion

        #region Misc
        public void UpdateHungerThirst(int hunger = -1, int thirst = -1)
        {
            Thirst = (thirst == -1) ? Thirst : thirst;
            Hunger = (hunger == -1) ? Hunger : hunger;
            UpdateFull();

            if (Client != null && Client.Exists)
                Client.EmitLocked("UpdateHungerThirst", Hunger, Thirst);
        }

        public bool HasOpenMenu()
        {
            if (MenuManager.HasOpenMenu(Client))
                return true;

            if (Phone.PhoneManager.HasOpenPhone(Client, out Phone.Phone phone))
                return true;

            if (RPGInventoryManager.HasInventoryOpen(Client))
                return true;

            return false;
        }
        #endregion

        #region Inventory
        public void UpdateClothing()
        {
            Clothing = new Clothings(Client);

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
                            Clothing.Glasses = (cloth != null) ? new PropData(cloth.Clothing.Drawable, cloth.Clothing.Texture) : (Character.Gender == 0) ? new PropData(14, 0) : new PropData(13, 0);
                            break;

                        case 1: // cap
                            Clothing.Hats = (cloth != null) ? new PropData(cloth.Clothing.Drawable, cloth.Clothing.Texture) : (Character.Gender == 0) ? new PropData(121, 0) : new PropData(120, 0);
                            break;

                        case 2: // necklace
                            Clothing.Accessory = (cloth != null) ? cloth.Clothing : new ClothData();
                            break;

                        case 3: // mask
                            Clothing.Mask = (cloth != null) ? cloth.Clothing : new ClothData();
                            break;

                        case 4: // earring
                            Clothing.Ears = (cloth != null) ? new PropData(cloth.Clothing.Drawable, cloth.Clothing.Texture) : (Character.Gender == 0) ? new PropData(33, 0) : new PropData(12, 0);
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

                                Clothing.Torso = new ClothData(torso, 0, 0);
                            }
                            else
                            {
                                Clothing.Tops = new ClothData(15, 0, 0);
                                Clothing.Torso = new ClothData(15, 0, 0);
                            }
                            break;

                        case 6: // watch
                            if (cloth != null) Clothing.Watches = new PropData(cloth.Clothing.Drawable, cloth.Clothing.Texture);
                            break;

                        case 7: // shirt
                            Clothing.Undershirt = (cloth != null) ? cloth.Clothing : new ClothData(15, 0, 0);
                            break;

                        case 8: // bracelet
                            if (cloth != null) Clothing.Bracelets = new PropData(cloth.Clothing.Drawable, cloth.Clothing.Texture);
                            break;

                        case 9: // pants
                            Clothing.Legs = (cloth != null) ? cloth.Clothing : (Character.Gender == 0) ? new ClothData(14, 0, 0) : new ClothData(15, 0, 0);
                            break;

                        case 10: // gloves
                            /*
                            Clothing.Torso = (cloth != null) ? cloth.Clothing : (Character.Gender == 0) ?
                                    new ClothData((byte)ClothingLoader.ClothingsMaleTopsList.DrawablesList[Clothing.Tops.Drawable].Torso[0], 0, 0) :
                                    new ClothData((byte)ClothingLoader.ClothingsFemaleTopsList.DrawablesList[Clothing.Tops.Drawable].Torso[0], 0, 0);*/
                            break;

                        case 11: // shoes
                            Clothing.Feet = (cloth != null) ? cloth.Clothing : (Character.Gender == 0) ? new ClothData(34, 0, 0) : new ClothData(35, 0, 0);
                            break;

                        case 12: // kevlar
                            Clothing.BodyArmor = (cloth != null) ? cloth.Clothing : new ClothData();
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
                                Clothing.Bags = new ClothData();
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
                                var radio = clothSlot.Item as Items.RadioItem;
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
            Clothing.UpdatePlayerClothing();
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

        public bool AddItem(Item item, int quantity = 1)
        {
            if (PocketInventory.AddItem(item, quantity))
            {
                if (RPGInventoryManager.HasInventoryOpen(this.Client))
                {
                    var rpg = RPGInventoryManager.GetRPGInventory(this.Client);
                    if (rpg != null)
                       RPGInventoryManager.Refresh(this.Client, rpg);
                }

                item.OnPlayerGetItem(this.Client);
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

                item.OnPlayerGetItem(this.Client);
                return true;
            }
            else
                return false;
        }

        public bool HasItemID(Models.InventoryData.ItemID id)
        {
            if (PocketInventory.HasItemID(id)) 
                return true;
            else if (BagInventory != null && BagInventory.HasItemID(id)) 
                return true;
            else 
                return false;
        }

        public List<ItemStack> GetAllItems()
        {

            List<ItemStack> _stacks = new List<ItemStack>();

            foreach (ItemStack stack in PocketInventory.InventoryList)
            {
                _stacks.Add(stack);
            }

            if (BagInventory != null)
            {
                foreach (ItemStack stack in BagInventory.InventoryList)
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
            int bagCount = 0;

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

        public int CountItem(Item item)
        {
            int somme = 0;
            somme += PocketInventory.CountItem(item);

            if (BagInventory != null)
            {
                somme += BagInventory.CountItem(item);
            }

            return somme;
        }

        public Dictionary<string, ItemStack[]> GetStacksItems(Models.InventoryData.ItemID itemID)
        {
            Dictionary<string, ItemStack[]> items = new Dictionary<string, ItemStack[]>();

            if (PocketInventory != null)
            {
                var pocket = PocketInventory.FindAllItemWithType(itemID);

                if (pocket != null && pocket.Length > 0)
                    items.Add(Utils.Enums.InventoryTypes.Pocket, pocket);
            }

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

        #region Keys
        public void AddKey(VehicleHandler veh, string keyVehicleName = null)
        {
            if (keyVehicleName == null) ListVehicleKey.Add(new VehicleKey(veh.VehicleManifest.DisplayName, veh.Plate));
            else ListVehicleKey.Add(new VehicleKey(keyVehicleName, veh.Plate));
        }

        public bool HasKey(VehicleHandler veh)
        {
            if (ListVehicleKey.Exists(k => k.Plate == veh.Plate))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool HasKey(string plate)
        {
            if (ListVehicleKey.Exists(k => k.Plate == plate))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RemoveKey(VehicleHandler veh)
        {
            VehicleKey _key = ListVehicleKey.Find(k => k.Plate == veh.Plate);
            if (_key != null)
            {
                ListVehicleKey.Remove(_key);
            }
        }
        #endregion

        #region Licences
        public bool HasLicense(LicenseType type) =>
            Licenses.Exists(l => l.Type == type);
        #endregion
    }
}
