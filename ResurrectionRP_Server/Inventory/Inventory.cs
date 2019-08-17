using System;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using AltV.Net.Elements.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;


namespace ResurrectionRP_Server.Inventory
{

    [BsonIgnoreExtraElements]
    public partial class Inventory
    {
        #region Variables
        public Models.ItemStack[] InventoryList;
        public int MaxSize { get; set; }
        public int MaxSlot { get; set; }
        [BsonIgnore, JsonIgnore]
        public bool Locked { get; set; }
        #endregion

        #region Constructor
        public Inventory(int maxSize, int maxSlot)
        {
            InventoryList = new Models.ItemStack[maxSlot];
            MaxSize = maxSize;
            MaxSlot = maxSlot;
        }

        public Models.ItemStack this[int x]
        {
            get { return InventoryList[x]; }
            set
            {
                if (value.Item != null)
                {
                    InventoryList[x] = value;
                }
            }
        }
        #endregion

        #region Method
        public Models.ItemStack[] FindAllItemWithType(Models.InventoryData.ItemID itemID)
        {
            return Array.FindAll(InventoryList, x => x?.Item.id == itemID);
        }

        public double CurrentSize()
        {
            double currentSize = 0;
            foreach (Models.ItemStack itemStack in InventoryList)
            {
                if (itemStack != null && itemStack.Item != null)
                    currentSize += (itemStack.Item.weight * itemStack.Quantity);
            }
            return (currentSize);
        }

        public bool IsFull(double itemsize = 0)
        {
            if (CurrentSize() + itemsize <= MaxSize)
                return false;
            return true;
        }

        public bool IsEmpty()
        {
            foreach (Models.ItemStack itemStack in InventoryList)
            {
                if (itemStack != null) return false;
            }
            return true;
        }

        public async Task<bool> AddItem(IPlayer client, Models.Item item, int quantity = 1, bool message = true)
        {
            if (AddItem(item, quantity))
            {
                if (message)
                    await client.NotifyAsync("Vous venez d'ajouter " + quantity + " " + item.name + " dans l'inventaire");
                await Entities.Players.PlayerManager.GetPlayerByClient(client)?.UpdatePlayerInfo();
                return true;
            }
            return false;
        }

        public bool AddItem(Models.Item item, int quantity)
        {
            return AddItem(item, quantity, out int slot);
        }

        public bool AddItem(Models.Item item, int quantity, out int slot)
        {
            slot = -1;
            if (CurrentSize() + (item.weight * quantity) > MaxSize) return false;

            if (InventoryList.Any(x => x?.Item.id == item.id) && item.isStackable)
            {
                Models.ItemStack itemStack = InventoryList.First(x => x?.Item.id == item.id);
                itemStack.Quantity += quantity;
            }
            else
            {
                slot = GetEmptySlot();
                if (slot == -1)
                    return false;

                InventoryList[slot] = new Models.ItemStack(item, quantity, slot);
            }
            return true;
        }

        public int GetEmptySlot()
        {
            for (int i = 0; i < InventoryList.Length; i++)
            {
                if (InventoryList[i] == null) return i;
            }
            return -1;
        }

        public void Clear() => new Inventory(MaxSize, MaxSlot);
        public void Clear(int newsize, int maxSlot) => new Inventory(newsize, maxSlot);

        public bool Delete(Models.ItemStack itemStack, int quantity = 1)
        {
            try
            {
                if (itemStack.Quantity < quantity)
                    return false;
                else if (itemStack.Quantity > quantity)
                    itemStack.Quantity -= quantity;
                else
                    InventoryList[GetSlotIndexUseStack(itemStack)] = null;
                return true;
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("Inventory Delete" +  ex);
                return false;
            }
        }

        public bool Delete(int slot, int quantity = 1)
        {
            try
            {
                var itemStack = InventoryList[slot];
                if (itemStack.Quantity > quantity)
                    itemStack.Quantity -= quantity;
                else
                    InventoryList[slot] = null;
                return true;
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("Inventory Delete" +ex);
                return false;
            }
        }

        /*
        public bool Delete(ItemID itemID, int quantity)
        {
            if (InventoryList.Any(x => x.Item.id == itemID))
            {
                ItemStack itemStack = InventoryList.Last(x => x.Item.id == itemID);

                if (itemStack == null)
                    return false;

                if (Delete(itemStack.SlotIndex, quantity))
                    return true;
            }
            return false;
        }
        */

        public int DeleteAll(Models.InventoryData.ItemID itemID, int quantityNeeded)
        {
            var value = quantityNeeded;
            // needed = 5
            // firstStack = 3
            // secondStack = 7

            for (int i = 0; i < InventoryList.Length; i++)
            {
                if (InventoryList[i] != null)
                {
                    if (InventoryList[i].Item == null)
                        continue;

                    if (InventoryList[i].Item.id == itemID)
                    {
                        if (InventoryList[i].Quantity > value)
                        {
                            InventoryList[i].Quantity -= value;
                            return quantityNeeded;
                        }
                        else
                        {
                            value -= InventoryList[i].Quantity; //5 - 3
                            InventoryList[i] = null;
                        }
                    }
                }
            }
            return quantityNeeded - value; // valeur supprimer
        }

        /*
        public bool Delete(Item item, int quantity)
        {
            if (InventoryList.Any(x => x.Item.id == item.id))
            {
                ItemStack itemStack = InventoryList.Last(x => x.Item.id == item.id);

                if (itemStack == null)
                    return false;

                if (Delete(itemStack.SlotIndex, quantity))
                    return true;
            }
            return false;
        }

        public async Task<bool> Delete(IPlayer client, Item item, int quantity)
        {
            if (Delete(item, quantity))
            {
                await client.NotifyAsync("Vous venez de supprimer " + quantity + " " + item.name + " de votre inventaire");
                return true;
            }
            return false;
        }


        public async Task<bool> Delete(IPlayer client, Item item, int slot, int quantity)
        {
            if (Delete(slot, quantity))
            {
                await client.NotifyAsync("Vous venez de supprimer " + quantity + " " + item.name + " de votre inventaire");
                return true;
            }
            return false;
        }

        public int DeleteAll(ItemID itemID, int quantityNeeded)
        {
            var value = quantityNeeded;
            // needed = 5
            // firstStack = 3
            // secondStack = 7

            for (int i = 0; i < InventoryList.Length; i++)
            {
                if (InventoryList[i] != null)
                {
                    if (InventoryList[i].Item == null)
                        continue;

                    if (InventoryList[i].Item.id == itemID)
                    {
                        if (InventoryList[i].Quantity > value)
                        {
                            InventoryList[i].Quantity -= value;
                            return quantityNeeded;
                        }  
                        else
                        {
                            value -= InventoryList[i].Quantity; //5 - 3
                            InventoryList[i] = null;
                        }
                    }                        
                }
            }
            return quantityNeeded - value; // valeur supprimer
        }
        */

        public Models.Item FindItemID(Models.InventoryData.ItemID id)
        {
            foreach (Models.Item item in LoadItem.ItemsList)
            {
                if (item.id == id)
                    return item;
            }
            return null;
        }

        public static Item ItemByID(ItemID id)
        {
            var item = LoadItem.ItemsList.Find(i => i.id == id) ?? null;

            if (item == null)
                return null;

            return item.CloneItem();
        }

        public bool HasItemID(ItemID id)
        {
            if (InventoryList.Any(x => x?.Item.id == id))
            {
                return true;
            }
            return false;
        }

        public bool HasItem(Item item)
        {
            if (InventoryList.Any(x => x?.Item.id == item?.id))
            {
                return true;
            }
            return false;
        }

        public int CountItem(Item item)
        {
            int a = 0;
            foreach (ItemStack invItem in InventoryList)
            {
                if (invItem != null && invItem.Item.id == item.id) a += invItem.Quantity;
            }
            return a;
        }

        public int CountItem(ItemID itemID)
        {
            int a = 0;
            foreach (ItemStack invItem in InventoryList)
            {
                if (invItem != null && invItem.Item.id == itemID) a += invItem.Quantity;
            }
            return a;
        }

        public int GetSlotIndexUseStack(ItemStack stack)
        {
            for (int i = 0; i < InventoryList.Length; i++)
            {
                if (InventoryList[i] == stack)
                    return i;
            }
            return -1;
        }
        #endregion
    }
}
