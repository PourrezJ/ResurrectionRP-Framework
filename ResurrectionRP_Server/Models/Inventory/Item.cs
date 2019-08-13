﻿using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Models
{/*
    [BsonKnownTypes(typeof(Alcohol), typeof(Axe), typeof(BuildingItem), typeof(ClothItem), typeof(CrateTools), typeof(Defibrilator), typeof(Eat), typeof(GasJerrycan), typeof(HandCuff), typeof(HealItem),
        typeof(IdentityCard), typeof(MaskItem), typeof(PhoneItem), typeof(RadioItem), typeof(BagItem), typeof(Unusable), typeof(Weapons), typeof(SeedItem), typeof(LockPick))]*/
    public class Item : ICloneable
    {
        [JsonProperty("id")]
        public ItemID id { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("desc")]
        public string description { get; set; }
        [JsonProperty("classes")]
        public string classes { get; set; }
        [JsonProperty("weight")]
        public double weight { get; set; }

        [DefaultValue(true), JsonProperty("give")]
        public bool isGiven { get; set; }
        [DefaultValue(true), JsonProperty("usable")]
        public bool isUsable { get; set; }
        [DefaultValue(true), JsonProperty("stack")]
        public bool isStackable { get; set; }
        [DefaultValue(true), JsonProperty("drop")]
        public bool isDropable { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }
        [JsonProperty("icon")]
        public string icon { get; set; }
        [JsonProperty("price")]
        public double itemPrice { get; set; }
        [JsonProperty("dock")]
        public bool isDockable { get; set; }

        public Dictionary<dynamic, dynamic> Variables = new Dictionary<dynamic, dynamic>();

        public Item(ItemID id, string name, string description, double weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, double itemPrice = 0,
            string type = "item", string icon = "unknown-item", string classes = "basic")
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.weight = weight;
            this.isGiven = isGiven;
            this.isUsable = isUsable;
            this.isStackable = isStackable;
            this.type = type;
            this.icon = icon;
            this.classes = classes;
            this.isDropable = isDropable;
            this.isDockable = isDockable;
            this.itemPrice = itemPrice;
        }

        public virtual Task Use(IPlayer Client, string inventoryType, int slot)
        {
            if (!isUsable) return Task.CompletedTask;
            return Task.CompletedTask;
        }

        public virtual Task OnPlayerGetItem(IPlayer player)
        {
            return Task.CompletedTask;
        }

        public virtual async Task<bool> Drop(IPlayer c, int quantite, int slot, OutfitInventory inventory)
        {
            if (!isDropable)
                return false;

            if (inventory.Delete(slot, quantite))
            {
                var position = await c.GetPositionAsync();
                var dimension = await c.GetDimensionAsync();
                ResuPickup resu = await ResuPickup.CreatePickup(MP.Utility.Joaat("prop_money_bag_01"), this, quantite, new Vector3(position.X, position.Y, position.Z - 1), false, TimeSpan.FromMinutes(1), dimension);
                resu.OnTakePickup += OnPickup;

                return true;
            }
            return false;
        }

        public virtual async Task<bool> Drop(IPlayer c, int quantite, int slot, Inventory inventory)
        {
            if (!isDropable)
                return false;

            if (inventory.Delete(slot, quantite))
            {
                var position = await c.GetPositionAsync();
                var dimension = await c.GetDimensionAsync();
                ResuPickup resu = await ResuPickup.CreatePickup(MP.Utility.Joaat("prop_money_bag_01"), this, quantite, new Vector3(position.X, position.Y, position.Z - 1), false, TimeSpan.FromMinutes(1), dimension);
                resu.OnTakePickup += OnPickup;

                return true;
            }
            return false;
        }

        public virtual async Task OnPickup(IPlayer client, ResuPickup pickup)
        {
            PlayerHandler ph = PlayerManager.GetPlayerByClient(client);
            if (ph != null)
            {
                if (!ph.InventoryIsFull(pickup.Quantite * pickup.Item.weight))
                {
                    if (await ph.AddItem(pickup.Item, pickup.Quantite))
                    {
                        await ph.PlayAnimation("putdown_low", "pickup_object", 0);
                        pickup.Delete();
                    }
                    else
                    {
                        await client.SendNotificationError("Action impossible.");
                    }
                }
                else
                {
                    await client.SendNotificationError("Vous n'avez pas la place.");
                }
            }
        }

        public virtual Task Give(IPlayer sender, IPlayer recever, int quantite)
        {
            return Task.CompletedTask;
        }

        public void SetData(string key, object value) => Variables.Add(key, value);
        public dynamic GetData(string key) => Variables.GetValueOrDefault(key);
        public void ResetData(string key) => Variables[key] = null;
        public bool HasData(string key) => Variables.ContainsKey(key);

        object ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        public Item CloneItem()
        {
            return (Item)this.MemberwiseClone();
        }
    }
}