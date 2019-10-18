
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Linq;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Models.InventoryData;

namespace ResurrectionRP_Server.Inventory
{

    public class OutfitSlots
    {
        public Models.Item Item;
    }

    [BsonIgnoreExtraElements]
    public class OutfitInventory
    {
        public static string[] OutfitClasses = { "glasses", "cap", "necklace", "mask", "earring", "jacket", "watch", "shirt", "bracelet", "pants", "gloves", "shoes", "kevlar", "backpack", "phone", "radio", "weapon", "weapon" };

        public Models.ItemStack[] Slots = new Models.ItemStack[18];

        public Entities.Objects.WorldObject prop = null;

        public OutfitInventory()
        {

        }

        public bool Delete(Models.ItemStack itemStack, int quantite)
        {
            if (Delete(GetSlotIndexUseStack(itemStack), quantite))
                return true;
            return false;
        }


        public bool Delete(Models.InventoryData.ItemID itemID, int quantite)
        {
            if (Slots.Any(x => x?.Item.id == itemID))
            {
                Models.ItemStack itemStack = Slots.FirstOrDefault(x => x?.Item.id == itemID);

                if (itemStack == null)
                    return false;

                if (Delete(GetSlotIndexUseStack(itemStack), quantite))
                    return true;
            }
            return false;
        }

        public bool Delete(int itemSlot, int quantite)
        {
            try
            {
                var itemStack = Slots[itemSlot];
                if (itemStack.Quantity > quantite)
                    itemStack.Quantity -= quantite;
                else
                    Slots[itemSlot] = null;
                return true;
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("Inventory Delete" + ex);
                return false;
            }
        }

        public int GetSlotIndexUseStack(Models.ItemStack stack)
        {
            for (int i = 0; i < Slots.Length; i++)
            {
                if (Slots[i] == stack)
                    return i;
            }
            return -1;
        }

        public Models.ItemStack HasItemEquip(ItemID item) =>
            Slots.FirstOrDefault((p) => (p?.Item.id == item));

        public Models.ItemStack[] FindAllItemWithType(Models.InventoryData.ItemID itemID)
        {
            return Array.FindAll(Slots, x => x?.Item.id == itemID);
        }
    }
}
