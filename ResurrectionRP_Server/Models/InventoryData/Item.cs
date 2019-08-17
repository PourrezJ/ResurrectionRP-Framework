using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using ResurrectionRP_Server.Models.InventoryData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Data;
using MongoDB.Bson.Serialization.Attributes;
using OutfitInventory = ResurrectionRP_Server.Inventory.OutfitInventory;

namespace ResurrectionRP_Server.Models
{
    [BsonKnownTypes(typeof(Items.Alcohol), typeof(Items.Axe), typeof(Items.BuildingItem), typeof(ClothItem), typeof(Items.CrateTools), typeof(Items.Defibrilator), typeof(Items.Eat), typeof(Items.GasJerrycan), typeof(Items.HandCuff), typeof(Items.HealItem),
        typeof(Items.IdentityCard), typeof(Items.MaskItem), typeof(Items.PhoneItem), typeof(Items.RadioItem), typeof(Items.BagItem), typeof(Items.Unusable), typeof(Weapons), typeof(Items.SeedItem), typeof(Items.LockPick))]
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
                var position = c.GetPosition();
                var dimension = c.Dimension;
                ResuPickup resu = await ResuPickup.CreatePickup(Alt.Hash("prop_money_bag_01"), this, quantite, new Vector3(position.X, position.Y, position.Z - 1), false, TimeSpan.FromMinutes(1), (uint)dimension);
                resu.OnTakePickup += OnPickup;

                return true;
            }
            return false;
        }

        public virtual async Task<bool> Drop(IPlayer c, int quantite, int slot, Inventory.Inventory inventory)
        {
            if (!isDropable)
                return false;

            if (inventory.Delete(slot, quantite))
            {
                var position = c.GetPosition();
                var dimension = c.Dimension;
                ResuPickup resu = await ResuPickup.CreatePickup(Alt.Hash("prop_money_bag_01"), this, quantite, new Vector3(position.X, position.Y, position.Z - 1), false, TimeSpan.FromMinutes(1), (uint)dimension);
                resu.OnTakePickup += OnPickup;

                return true;
            }
            return false;
        }

        public virtual async Task OnPickup(IPlayer client, ResuPickup pickup)
        {
            Entities.Players.PlayerHandler ph = Entities.Players.PlayerManager.GetPlayerByClient(client);
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
                        client.SendNotificationError("Action impossible.");
                    }
                }
                else
                {
                    client.SendNotificationError("Vous n'avez pas la place.");
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
            return MemberwiseClone();
        }

        public Item CloneItem()
        {
            return (Item)MemberwiseClone();
        }
    }
}