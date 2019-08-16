
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Linq;

namespace ResurrectionRP_Server.Inventory
{

    public class OutfitSlots
    {
        public Item Item;
    }

    [BsonIgnoreExtraElements]
    public class OutfitInventory
    {
        public static string[] OutfitClasses = { "glasses", "cap", "necklace", "mask", "earring", "jacket", "watch", "shirt", "bracelet", "pants", "gloves", "shoes", "kevlar", "backpack", "phone", "radio", "weapon", "weapon" };

        public ItemStack[] Slots = new ItemStack[18];

        public OutfitInventory()
        {

        }

        public bool Delete(ItemStack itemStack, int quantite)
        {
            if (Delete(GetSlotIndexUseStack(itemStack), quantite))
                return true;
            return false;
        }


        public bool Delete(ItemID itemID, int quantite)
        {
            if (Slots.Any(x => x?.Item.id == itemID))
            {
                ItemStack itemStack = Slots.FirstOrDefault(x => x?.Item.id == itemID);

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
                MP.Logger.Error("Inventory Delete", ex);
                return false;
            }
        }

        public int GetSlotIndexUseStack(ItemStack stack)
        {
            for (int i = 0; i < Slots.Length; i++)
            {
                if (Slots[i] == stack)
                    return i;
            }
            return -1;
        }

        public ItemStack[] FindAllItemWithType(ItemID itemID)
        {
            return Array.FindAll(Slots, x => x?.Item.id == itemID);
        }
    }
}
