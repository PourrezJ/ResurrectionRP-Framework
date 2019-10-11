using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using AltV.Net;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using Inventory = ResurrectionRP_Server.Inventory.Inventory;


namespace ResurrectionRP_Server.Inventory
{

    public class RPGInventoryOutfit
    {
        public RPGOutfitSlots[] NamedSlots = new RPGOutfitSlots[18];
        public List<RPGInventoryItem> RPGInventoryItems = new List<RPGInventoryItem>();
        public double CurrentSize = 0;
        public double MaxSize = 999;
        public int Slots = 18;
    }

    public class RPGInventory
    {
        public List<RPGInventoryItem> RPGInventoryItems = new List<RPGInventoryItem>();
        public double CurrentSize;
        public double MaxSize;
        public int Slots;
        public bool IsMarket;
    }

    public class RPGInventoryMenu
    {
        #region Private fields
        private Dictionary<string, object> _data = new Dictionary<string, object>();
        #endregion

        #region Public properties
        public string Id { get; set; }
        public bool Give { get; set; }

        public RPGInventory PocketsItems = new RPGInventory();
        public RPGInventory BagItems;
        public RPGInventory DistantItems;
        public RPGInventoryOutfit OutfitItems = new RPGInventoryOutfit();

        public IPlayer DistantPlayer { get; set; }
        [JsonIgnore]
        public Inventory Inventory;
        [JsonIgnore]
        public Inventory Bag;
        [JsonIgnore]
        public Inventory Distant;
        [JsonIgnore]
        public OutfitInventory Outfit;

        [JsonIgnore]
        public OnCloseDelegate OnOpen { get; set; }
        [JsonIgnore]
        public OnCloseDelegate OnClose { get; set; }
        [JsonIgnore]
        public OnMoveDelegate OnMove { get; set; }
        [JsonIgnore]
        public OnPriceChangeDelegate PriceChange { get; set; }
        #endregion

        #region Public Delegate
        public delegate void OnOpenDelegate(IPlayer client, RPGInventoryMenu menu);
        public delegate void OnCloseDelegate(IPlayer client, RPGInventoryMenu menu);
        public delegate void OnMoveDelegate(IPlayer client, RPGInventoryMenu menu);
        public delegate void OnPriceChangeDelegate(IPlayer client, RPGInventoryMenu menu, Models.ItemStack stack, int price);
        #endregion

        public RPGInventoryMenu(Inventory inventory, OutfitInventory outfitInventory = null, Inventory bag = null, Inventory distant = null, bool ismarket = false, IPlayer distantPlayer = null)
        {
            try
            {
                Inventory = inventory;
                Bag = bag;
                Distant = distant;
                Outfit = outfitInventory;
                DistantPlayer = distantPlayer;

                PocketsItems.CurrentSize = inventory.CurrentSize();
                PocketsItems.MaxSize = inventory.MaxSize;
                PocketsItems.Slots = inventory.MaxSlot;

                for (int i = 0; i < inventory.InventoryList.Length; i++)
                {
                    if (inventory.InventoryList[i] != null && inventory.InventoryList[i].Item != null)
                        PocketsItems.RPGInventoryItems.Add(new RPGInventoryItem(inventory.InventoryList[i], Utils.Enums.InventoryTypes.Pocket, i));
                }

                if (Bag != null)
                {
                    BagItems = new RPGInventory();
                    BagItems.CurrentSize = Bag.CurrentSize();
                    BagItems.MaxSize = Bag.MaxSize;
                    BagItems.Slots = Bag.MaxSlot;
                    for (int i = 0; i < Bag.InventoryList.Length; i++)
                    {
                        if (Bag.InventoryList[i] != null && Bag.InventoryList[i].Item != null)
                            BagItems.RPGInventoryItems.Add(new RPGInventoryItem(Bag.InventoryList[i], Utils.Enums.InventoryTypes.Bag, i));
                    }
                }

                if (Distant != null)
                {
                    DistantItems = new RPGInventory();
                    DistantItems.CurrentSize = Distant.CurrentSize();
                    DistantItems.MaxSize = Distant.MaxSize;
                    DistantItems.Slots = Distant.MaxSlot;
                    DistantItems.IsMarket = ismarket;

                    for (int i = 0; i < Distant.InventoryList.Length; i++)
                    {
                        if (Distant.InventoryList[i] != null && Distant.InventoryList[i].Item != null)
                            DistantItems.RPGInventoryItems.Add(new RPGInventoryItem(Distant.InventoryList[i], Utils.Enums.InventoryTypes.Distant, i));
                    }
                }

                if (Outfit != null)
                {
                    OutfitItems = new RPGInventoryOutfit();
                    OutfitItems.NamedSlots = new RPGOutfitSlots[18];

                    for (int i = 0; i < Outfit.Slots.Length; i++)
                    {
                        if (Outfit.Slots[i] != null)
                        {
                            OutfitItems.NamedSlots[i] = new RPGOutfitSlots(i + 1, (Outfit.Slots[i].Item != null) ? Outfit.Slots[i].Item.name : "", OutfitInventory.OutfitClasses[i], true);
                            OutfitItems.RPGInventoryItems.Add(new RPGInventoryItem(Outfit.Slots[i], Utils.Enums.InventoryTypes.Outfit, i));
                        }
                        else
                        {
                            OutfitItems.NamedSlots[i] = new RPGOutfitSlots(i + 1, "", OutfitInventory.OutfitClasses[i], true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("RPGInventoryMenu" + ex);
            }
        }

        public void OpenMenu(IPlayer client)
        {
            if (OnOpen != null)
                OnOpen(client, this);
            RPGInventoryManager.OpenMenu(client, this);
        }

        public void CloseMenu(IPlayer client)
        {
            OnClose?.Invoke(client, this);
            RPGInventoryManager.CloseMenu(client, this);
        }
    }
}
