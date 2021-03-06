﻿using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System;
using System.Linq;

namespace ResurrectionRP_Server.Inventory
{
    [BsonIgnoreExtraElements]
    public partial class Inventory
    {
        #region Variables
        public ItemStack[] InventoryList;
        public int MaxSize { get; set; }
        public int MaxSlot { get; set; }
        [BsonIgnore, JsonIgnore]
        public bool Locked { get; set; }
        #endregion

        #region Constructor
        public Inventory(int maxSize, int maxSlot)
        {
            InventoryList = new ItemStack[maxSlot];
            MaxSize = maxSize;
            MaxSlot = maxSlot;
        }

        public ItemStack this[int x]
        {
            get => InventoryList[x];
            set
            {
                if (value.Item != null)
                {
                    lock (InventoryList)
                    {
                        InventoryList[x] = value;
                    }   
                }
            }
        }
        #endregion

        #region Method
        public ItemStack[] FindAllItemWithType(ItemID itemID)
        {
            return Array.FindAll(InventoryList, x => x?.Item.id == itemID);
        }

        public double CurrentSize()
        {
            double currentSize = 0;

            foreach (ItemStack itemStack in InventoryList)
            {
                if (itemStack != null && itemStack.Item != null)
                    currentSize += itemStack.Item.weight * itemStack.Quantity;
            }

            return currentSize;
        }

        public bool IsFull(double itemsize = 0)
        {
            if (CurrentSize() + itemsize <= MaxSize)
                return false;

            return true;
        }

        public bool IsEmpty()
        {
            foreach (ItemStack itemStack in InventoryList)
            {
                if (itemStack != null)
                    return false;
            }

            return true;
        }

        public bool AddItem(IPlayer client, Item item, int quantity = 1, bool message = true)
        {
            if (AddItem(item, quantity))
            {
                if (message)
                    client.EmitLocked("Display_Help", "Vous venez d'ajouter " + quantity + " " + item.name + " dans l'inventaire", 10000);

                client.GetPlayerHandler()?.UpdateFull();
                return true;
            }

            return false;
        }

        public bool AddItem(Item item, int quantity)
        {
            if(item.isStackable)
                return AddItem(item, quantity, out int slot);
            else {
                for(int i = 0; i < quantity; i++) {
                    if(!AddItem(item, 1, out int slots))
                        return false;
                }
                return true;
            }
        }

        public bool AddItem(Item item, int quantity, out int slot)
        {
            slot = -1;

            if (CurrentSize() + (item.weight * quantity) > MaxSize)
                return false;

            lock (InventoryList)
            {
                if (InventoryList.Any(x => x?.Item.id == item.id) && item.isStackable)
                {
                    ItemStack itemStack = InventoryList.First(x => x?.Item.id == item.id);
                    itemStack.Quantity += quantity;
                }
                else
                {
                    slot = GetEmptySlot();

                    if (slot == -1)
                        return false;

                    InventoryList[slot] = new ItemStack(item, quantity, slot);
                }
            }

            return true;
        }

        public int GetEmptySlot()
        {
            lock (InventoryList)
            {
                for (int i = 0; i < InventoryList.Length; i++)
                {
                    if (InventoryList[i] == null) return i;
                }
            }

            return -1;
        }

        public void Clear() => new Inventory(MaxSize, MaxSlot);

        public void Clear(int newsize, int maxSlot) => new Inventory(newsize, maxSlot);

        public bool Delete(ItemStack itemStack, int quantity = 1)
        {
            try
            {
                lock (InventoryList)
                {
                    if (itemStack.Quantity < quantity)
                        return false;
                    else if (itemStack.Quantity > quantity)
                        itemStack.Quantity -= quantity;
                    else
                        InventoryList[GetSlotIndexUseStack(itemStack)] = null;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("Inventory Delete: " + ex);
                return false;
            }
        }

        public bool Delete(int slot, int quantity = 1)
        {
            try
            {
                lock (InventoryList)
                {
                    var itemStack = InventoryList[slot];

                    if (itemStack.Quantity > quantity)
                        itemStack.Quantity -= quantity;
                    else
                    {
                        itemStack.Quantity = 0;
                        InventoryList[slot] = null;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("Inventory Delete: " + ex);
                return false;
            }
        }

        public int DeleteAll(ItemID itemID, int quantityNeeded)
        {
            var value = quantityNeeded;
            lock (InventoryList)
            {
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
            } 

            return quantityNeeded - value; // valeur supprimer
        }

        public Item GetItem(ItemID itemID)
        {
            ItemStack item = InventoryList.ToList().Find(p => p?.Item.id == itemID);
            if (item == null)
                return null;
            return item?.Item;
        }

        public bool HasItemID(ItemID id)
        {
            lock (InventoryList)
            {
                if (InventoryList.Any(x => x?.Item.id == id))
                    return true;

                return false;
            }
        }

        public bool HasItem(Item item)
        {
            lock (InventoryList)
            {
                if (InventoryList.Any(x => x?.Item.id == item?.id))
                    return true;

                return false;
            }
        }

        public int CountItem(Item item)
        {
            int count = 0;
            lock (InventoryList)
            {
                foreach (ItemStack invItem in InventoryList)
                {
                    if (invItem != null && invItem.Item.id == item.id)
                        count += invItem.Quantity;
                }
            }

            return count;
        }

        public int CountItem(ItemID itemID)
        {
            int count = 0;

            lock (InventoryList)
            {
                foreach (ItemStack invItem in InventoryList)
                {
                    if (invItem != null && invItem.Item.id == itemID)
                        count += invItem.Quantity;
                }
            }

            return count;
        }

        public int GetSlotIndexUseStack(ItemStack stack)
        {
            lock (InventoryList)
            {
                for (int i = 0; i < InventoryList.Length; i++)
                {
                    if (InventoryList[i] == stack)
                        return i;
                }
            }

            return -1;
        }
        #endregion
    }
}