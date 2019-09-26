using AltV.Net.Elements.Entities;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Models;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Items
{
    [BsonIgnoreExtraElements]
    class PhoneItem : Item
    {
        public Phone.Phone PhoneHandler;

        public PhoneItem(Models.InventoryData.ItemID id, string name, string description, Phone.Phone phone, int weight = 0, bool isGiven = false, bool isUsable = false, bool isStackable = true, bool isDropable = true, bool isDockable = false, int itemPrice = 0, string type = "phone", string icon = "phone", string classes = "phone") : base(id, name, description, weight, isGiven, isUsable, isStackable, isDropable, isDockable, itemPrice, type, icon, classes)
        {
            PhoneHandler = phone;
            this.description = PhoneHandler.PhoneNumber;
        }

        public override Task<bool> Drop(IPlayer c, int quantite, int slot, Inventory.OutfitInventory inventory)
        {
            Phone.Phone.RemovePhoneInList(c, PhoneHandler);
            return base.Drop(c, quantite, slot, inventory);
        }

        public override Task<bool> Drop(IPlayer c, int quantite, int slot, Inventory.Inventory inventory)
        {
            Phone.Phone.RemovePhoneInList(c, PhoneHandler);
            return base.Drop(c, quantite, slot, inventory);
        }

        public override Task Give(IPlayer sender, IPlayer recever, int quantite)
        {
            return base.Give(sender, recever, quantite);
        }

        public override Task OnPickup(IPlayer client, Models.ResuPickup pickup)
        {
            return base.OnPickup(client, pickup);
        }

        public override Task OnPlayerGetItem(IPlayer player)
        {
            Phone.Phone.AddPhoneInList(player, this.PhoneHandler);
            return Task.CompletedTask;
        }

        public override void Use(IPlayer c, string inventoryType, int slot)
        {
            Phone.PhoneManager.OpenPhone(c, PhoneHandler);
            // await base.Use(c);
        }
    }
}
