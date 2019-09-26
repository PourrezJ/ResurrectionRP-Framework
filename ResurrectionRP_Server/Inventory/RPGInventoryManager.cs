using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using PropData = ResurrectionRP_Server.Models.PropData;
using ClothData = ResurrectionRP_Server.Models.ClothData;
using ItemID = ResurrectionRP_Server.Models.InventoryData.ItemID;
using Newtonsoft.Json;
using ResurrectionRP_Server.Utils.Enums;

namespace ResurrectionRP_Server.Inventory
{
    public class RPGInventoryManager
    {
        #region Private static properties
        private static ConcurrentDictionary<IPlayer, RPGInventoryMenu> _clientMenus = new ConcurrentDictionary<IPlayer, RPGInventoryMenu>();
        #endregion

        #region Constructor
        public RPGInventoryManager()
        {
            AltAsync.OnClient("RPGInventory_UseItem", RPGInventory_UseItem);
            AltAsync.OnClient("RPGInventory_DropItem", RPGInventory_DropItem);
            AltAsync.OnClient("RPGInventory_GiveItem", RPGInventory_GiveItem);
            AltAsync.OnClient("RPGInventory_SwitchItemInventory_SRV", RPGInventory_SwitchItemInventory_SRV);
            AltAsync.OnClient("RPGInventory_SplitItemInventory_SRV", RPGInventory_SplitItemInventory_SRV);
            AltAsync.OnClient("RPGInventory_ClosedMenu_SRV", RPGInventory_ClosedMenu_SRV);
            AltAsync.OnClient("RPGInventory_PriceItemInventory_SRV", RPGInventory_PriceItemInventory_SRV);
        }
        #endregion

        #region Server Events
        public void OnPlayerQuit(IPlayer sender)
        {
            if (HasInventoryOpen(sender))
            {
                Task.Run(async () =>
                await CloseMenu(sender));
            }
        }
        #endregion

        #region Public static methods
        public static async Task CloseMenu(IPlayer client, RPGInventoryMenu oldmenu = null)
        {
            RPGInventoryMenu menu = null;
            if (oldmenu != null || _clientMenus.TryRemove(client, out menu))
            {
                if (menu?.OnClose != null)
                    await menu.OnClose.Invoke(client, menu);

                if (!client.Exists)
                    return;
                await client.EmitAsync("InventoryManager_CloseMenu");
            }
        }

        public static bool HasInventoryOpen(Inventory inventory) => _clientMenus.Values.Any(i => i.Inventory == inventory || i.Bag == inventory || i.Distant == inventory);
        public static bool HasInventoryOpen(IPlayer client) => _clientMenus.ContainsKey(client);
        public static RPGInventoryMenu GetRPGInventory(IPlayer client)
        {
            if (_clientMenus.TryGetValue(client, out RPGInventoryMenu menu))
                return menu;
            return null;
        }


        public static async Task<bool> OpenMenu(IPlayer client, RPGInventoryMenu menu)
        {
            RPGInventoryMenu oldMenu = null;
            _clientMenus.TryRemove(client, out oldMenu);

            if (_clientMenus.TryAdd(client, menu))
            {
                await client.EmitAsync("InventoryManager_OpenMenu",
                    JsonConvert.SerializeObject(menu.PocketsItems),
                    JsonConvert.SerializeObject(menu.BagItems),
                    JsonConvert.SerializeObject(menu.DistantItems),
                    JsonConvert.SerializeObject(menu.OutfitItems),
                    (menu.DistantPlayer == null) ? false : true);

                return true;
            }
            return false;
        }
        #endregion

        #region Use
        private async Task RPGInventory_UseItem(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            RPGInventoryMenu menu = null;

           // ItemID itemID = (ItemID)Convert.ToInt32(args[0]);
            string targetInventory = Convert.ToString(args[1]);
            int itemSlot = Convert.ToInt32(args[2]);


            if (_clientMenus.TryGetValue(client, out menu))
            {
                Models.ItemStack itemStack = null;

                switch (targetInventory)
                {
                    case Utils.Enums.InventoryTypes.Pocket:
                        itemStack = menu.Inventory.InventoryList[itemSlot];
                        break;

                    case Utils.Enums.InventoryTypes.Bag:
                        itemStack = menu.Bag.InventoryList[itemSlot];
                        break;

                    case Utils.Enums.InventoryTypes.Distant:
                        itemStack = menu.Distant.InventoryList[itemSlot];
                        break;
                }

                if (itemStack != null && itemStack.Item != null)
                {
                    itemStack.Item.Use(client, targetInventory, itemSlot);
                    await itemStack.Item.UseAsync(client, targetInventory, itemSlot);
                }

                Refresh(client, menu);
            }
        }
        #endregion

        #region Drop
        private async Task RPGInventory_DropItem(IPlayer client, object[] args)
        {
            try
            {
                string inventoryType = Convert.ToString(args[0]);
               // ItemID itemID = (ItemID)Convert.ToInt32(args[1]);
                int slot = Convert.ToInt32(args[2]);
                int quantity = Convert.ToInt32(args[3]);

                if (quantity == 0)
                    return;

                if (!client.Exists)
                    return;

                var ph = client.GetPlayerHandler();
                if (ph == null)
                    return;

                RPGInventoryMenu menu = null;
                RPGInventoryItem invItem = null;

                if (_clientMenus.TryGetValue(client, out menu))
                {
                    switch (inventoryType)
                    {
                        case InventoryTypes.Pocket:
                            invItem = menu.PocketsItems.RPGInventoryItems.Find(s => s.inventorySlot == slot);

                            if (invItem == null)
                                return;

                            if (await invItem.stack.Item.Drop(client, quantity, slot, menu.Inventory))
                            {
                                if (invItem.stack.Quantity == 0)
                                    menu.PocketsItems.RPGInventoryItems.Remove(invItem);
                                else
                                    invItem.quantity = invItem.stack.Quantity;
                            }

                            break;
                        case InventoryTypes.Bag:
                            invItem = menu.BagItems.RPGInventoryItems.Find(s => s.inventorySlot == slot);

                            if (invItem == null)
                                return;

                            if (await invItem.stack.Item.Drop(client, quantity, slot, menu.Bag))
                            {
                                if (invItem.stack.Quantity == 0)
                                    menu.PocketsItems.RPGInventoryItems.Remove(invItem);
                                else
                                    invItem.quantity = invItem.stack.Quantity;
                            }

                            break;
                        case InventoryTypes.Distant:
                            invItem = menu.DistantItems.RPGInventoryItems.Find(s => s.inventorySlot == slot);

                            if (invItem == null)
                                return;

                            if (await invItem.stack.Item.Drop(client, quantity, slot, menu.Distant))
                            {
                                if (invItem.stack.Quantity == 0)
                                    menu.PocketsItems.RPGInventoryItems.Remove(invItem);
                                else
                                    invItem.quantity = invItem.stack.Quantity;
                            }

                            break;
                        case InventoryTypes.Outfit:
                            invItem = menu.OutfitItems.RPGInventoryItems.Find(s => s.inventorySlot == slot);

                            if (invItem == null)
                                return;

                            if (await invItem.stack.Item.Drop(client, quantity, slot, menu.Outfit))
                            {
                                if (invItem.stack.Quantity == 0)
                                    menu.PocketsItems.RPGInventoryItems.Remove(invItem);
                                else
                                    invItem.quantity = invItem.stack.Quantity;
                            }

                            switch (invItem.id)
                            {
                                case ItemID.Glasses: // glasses
                                    ph.Clothing.Glasses = (ph.Character.Gender == 0) ? new PropData(14, 0) : new PropData(13, 0);
                                    break;

                                case ItemID.Hats: // hair
                                    ph.Clothing.Hats = (ph.Character.Gender == 0) ? new PropData(121, 0) : new PropData(120, 0);
                                    break;

                                case ItemID.Necklace: // necklace
                                    ph.Clothing.Accessory = new ClothData();
                                    break;

                                case ItemID.Mask: // mask
                                    ph.Clothing.Mask = new ClothData();
                                    break;

                                case ItemID.Ears: // earring
                                    ph.Clothing.Ears = (ph.Character.Gender == 0) ? new PropData(33, 0) : new PropData(12, 0);
                                    break;

                                case ItemID.Jacket: // jacket
                                    ph.Clothing.Torso = new ClothData(15, 0, 0);
                                    ph.Clothing.Tops = new ClothData(15, 0, 0);
                                    break;

                                case ItemID.Watch: // watch
                                    ph.Clothing.Watches = (ph.Character.Gender == 0) ? new PropData(2, 0) : new PropData(1, 0);
                                    break;

                                case ItemID.TShirt: // shirt
                                    ph.Clothing.Undershirt = (ph.Character.Gender == 0) ? new ClothData(57, 0, 0) : new ClothData(34, 0, 0);
                                    break;

                                case ItemID.Bracelet: // bracelet
                                    ph.Clothing.Bracelets = new PropData(15, 0);
                                    break;

                                case ItemID.Pant: // pants
                                    ph.Clothing.Legs = (ph.Character.Gender == 0) ? new ClothData(14, 0, 0) : new ClothData(15, 0, 0);
                                    break;

                                case ItemID.Glove: // gloves
                                    ph.Clothing.Glasses = (ph.Character.Gender == 0) ? new PropData(6, 0) : new PropData(5, 0);
                                    break;

                                case ItemID.Shoes: // shoes
                                    ph.Clothing.Feet = (ph.Character.Gender == 0) ? new ClothData(34, 0, 0) : new ClothData(35, 0, 0);
                                    break;

                                case ItemID.Kevlar: // kevlar
                                    ph.Clothing.BodyArmor = new ClothData(0, 0, 0);
                                    break;

                                case ItemID.Bag: // backpack
                                    ph.Clothing.Bags = new ClothData();
                                    ph.BagInventory = null;
                                    menu.Bag = null;
                                    menu.BagItems = null;
                                    //await menu.CloseMenu(sender);
                                    break;

                                case ItemID.Weapon:
                                case ItemID.LampeTorche:
                                case ItemID.Carabine:
                                case ItemID.Matraque:
                                case ItemID.Bat:
                                case ItemID.BattleAxe:
                                case ItemID.CombatPistol:
                                case ItemID.Flashlight:
                                case ItemID.Hache:
                                case ItemID.HeavyPistol:
                                case ItemID.Knife:
                                case ItemID.Machete:
                                case ItemID.Musket:
                                case ItemID.SNSPistol:
                                case ItemID.Colt6Coup:
                                case ItemID.Colt1911:
                                case ItemID.Magnum357:
                                case ItemID.Pistol50:
                                case ItemID.Pistol:
                                case ItemID.Pump:
                                case ItemID.Taser:
                                    await client.RemoveAllWeaponsAsync();
                                    break;
                            }

                            ph.Clothing.UpdatePlayerClothing();
                            break;
                    }

                    // Temporary solution to save inventory after object drop. Doesn't update inventory when dropping from distant inventory.
                    ph.UpdateFull();
                    Refresh(client, menu);
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("RPGInventory_DropItem" + ex);
            }
        }

        #endregion

        #region Give
        private async Task RPGInventory_GiveItem(IPlayer client, object[] args)
        {
            try
            {
                string inventoryType = Convert.ToString(args[0]);
                int itemID = Convert.ToInt32(args[1]);
                int slot = Convert.ToInt32(args[2]);
                int quantity = Convert.ToInt32(args[3]);

                if (quantity == 0)
                    return;

                if (!client.Exists)
                    return;

                var ph = client.GetPlayerHandler();
                if (ph == null)
                    return;

                RPGInventoryMenu menu = null;
                RPGInventoryItem invItem = null;
                _clientMenus.TryGetValue(client, out menu);
                if (menu != null)
                {
                    if (menu.DistantPlayer == null)
                        return;

                    if (!menu.DistantPlayer.Exists)
                        return;

                    var phDistant = menu.DistantPlayer.GetPlayerHandler();

                    if (phDistant == null)
                        return;

                    switch (inventoryType)
                    {
                        case Utils.Enums.InventoryTypes.Pocket:
                            invItem = menu.PocketsItems.RPGInventoryItems.Find(s => s.inventorySlot == slot);
                            if (invItem == null)
                                return;

                            if (await phDistant.AddItem(invItem.stack.Item, quantity))
                            {
                                phDistant.Client.SendNotification($"On vous à donner {quantity} {invItem.stack.Item.name}");
                                ph.DeleteItem(slot, inventoryType, quantity);
                            }

                            break;
                        case Utils.Enums.InventoryTypes.Bag:
                            invItem = menu.BagItems.RPGInventoryItems.Find(s => s.inventorySlot == slot);
                            if (invItem == null)
                                return;
                            if (await phDistant.AddItem(invItem.stack.Item, quantity))
                            {
                                phDistant.Client.SendNotification($"On vous à donner {quantity} {invItem.stack.Item.name}");
                                ph.DeleteItem(slot, inventoryType, quantity);
                            }

                            break;
                        case Utils.Enums.InventoryTypes.Distant:
                            invItem = menu.DistantItems.RPGInventoryItems.Find(s => s.inventorySlot == slot);
                            if (invItem == null)
                                return;
                            if (await phDistant.AddItem(invItem.stack.Item, quantity))
                            {
                                phDistant.Client.SendNotification($"On vous à donner {quantity} {invItem.stack.Item.name}");
                                ph.DeleteItem(slot, inventoryType, quantity);
                            }
                            break;
                        case Utils.Enums.InventoryTypes.Outfit:
                            invItem = menu.OutfitItems.RPGInventoryItems.Find(s => s.inventorySlot == slot);
                            if (invItem == null)
                                return;
                            if (await phDistant.AddItem(invItem.stack.Item, quantity))
                            {
                                phDistant.Client.SendNotification($"On vous a donné {quantity} {invItem.stack.Item.name}");
                                ph.DeleteItem(slot, inventoryType, quantity);
                            }


                            switch (invItem.id)
                            {
                                case ItemID.Glasses: // glasses
                                    ph.Clothing.Glasses = (ph.Character.Gender == 0) ? new PropData(14, 0) : new PropData(13, 0);
                                    break;

                                case ItemID.Hats: // hair
                                    ph.Clothing.Hats = (ph.Character.Gender == 0) ? new PropData(121, 0) : new PropData(120, 0);
                                    break;

                                case ItemID.Necklace: // necklace
                                    ph.Clothing.Accessory = new ClothData();
                                    break;

                                case ItemID.Mask: // mask
                                    ph.Clothing.Mask = new ClothData();
                                    break;

                                case ItemID.Ears: // earring
                                    ph.Clothing.Ears = (ph.Character.Gender == 0) ? new PropData(33, 0) : new PropData(12, 0);
                                    break;

                                case ItemID.Jacket: // jacket
                                    ph.Clothing.Torso = new ClothData(15, 0, 0);
                                    ph.Clothing.Tops = new ClothData(15, 0, 0);
                                    break;

                                case ItemID.Watch: // watch
                                    ph.Clothing.Watches = (ph.Character.Gender == 0) ? new PropData(2, 0) : new PropData(1, 0);
                                    break;

                                case ItemID.TShirt: // shirt
                                    ph.Clothing.Undershirt = (ph.Character.Gender == 0) ? new ClothData(57, 0, 0) : new ClothData(34, 0, 0);
                                    break;

                                case ItemID.Bracelet: // bracelet
                                    ph.Clothing.Bracelets = new PropData(15, 0);
                                    break;

                                case ItemID.Pant: // pants
                                    ph.Clothing.Legs = (ph.Character.Gender == 0) ? new ClothData(14, 0, 0) : new ClothData(15, 0, 0);
                                    break;

                                case ItemID.Glove: // gloves
                                    ph.Clothing.Glasses = (ph.Character.Gender == 0) ? new PropData(6, 0) : new PropData(5, 0);
                                    break;

                                case ItemID.Shoes: // shoes
                                    ph.Clothing.Feet = (ph.Character.Gender == 0) ? new ClothData(34, 0, 0) : new ClothData(35, 0, 0);
                                    break;

                                case ItemID.Kevlar: // kevlar
                                    ph.Clothing.BodyArmor = new ClothData(0, 0, 0);
                                    break;

                                case ItemID.Bag: // backpack
                                    ph.Clothing.Bags = new ClothData();
                                    ph.BagInventory = null;
                                    menu.Bag = null;
                                    menu.BagItems = null;
                                    //await menu.CloseMenu(sender);
                                    break;

                                case ItemID.Weapon:
                                case ItemID.LampeTorche:
                                case ItemID.Carabine:
                                case ItemID.Matraque:
                                case ItemID.Bat:
                                case ItemID.BattleAxe:
                                case ItemID.CombatPistol:
                                case ItemID.Flashlight:
                                case ItemID.Hache:
                                case ItemID.HeavyPistol:
                                case ItemID.Knife:
                                case ItemID.Machete:
                                case ItemID.Musket:
                                case ItemID.SNSPistol:
                                case ItemID.Colt6Coup:
                                case ItemID.Colt1911:
                                case ItemID.Magnum357:
                                case ItemID.Pistol50:
                                case ItemID.Pistol:
                                case ItemID.Pump:
                                case ItemID.Taser:
                                    await client.RemoveAllWeaponsAsync();
                                    break;
                            }

                            ph.Clothing.UpdatePlayerClothing();

                            break;
                    }

                    // Temporary solution to save inventory after object drop
                    ph.UpdateFull();
                    phDistant.UpdateFull();
                    Refresh(client, menu);
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("RPGInventory_DropItem" + ex);
            }
        }
        #endregion

        #region Switch
        private async Task RPGInventory_SwitchItemInventory_SRV(IPlayer client, object[] args)
        {
            Alt.Server.LogColored("RPGInventory_SwitchItemInventory_SRV called!");
            try
            {
                string targetRPGInv = Convert.ToString(args[0]);
                string oldRPGInv = Convert.ToString(args[1]);
                int itemID = Convert.ToInt32(args[2]);
                int slotID = Convert.ToInt32(args[3]);
                int oldslotID = Convert.ToInt32(args[4]);

                RPGInventoryMenu menu = null;
                _clientMenus.TryGetValue(client, out menu);

                if (menu != null)
                {
                    Inventory oldInventory = null;

                    switch (oldRPGInv) // OLD Inventory
                    {
                        case InventoryTypes.Pocket:
                            oldInventory = menu.Inventory;
                            break;
                        case InventoryTypes.Bag:
                            oldInventory = menu.Bag;
                            break;
                        case InventoryTypes.Distant:
                            oldInventory = menu.Distant;
                            break;
                    }

                    Models.ItemStack stack = null;
                    Models.Item item = null;

                    Entities.Players.PlayerHandler player = client.GetPlayerHandler();
                    if (player == null)
                        return;

                    if (oldInventory != null)
                    {
                        if (oldInventory.InventoryList[oldslotID] != null)
                        {
                            stack = oldInventory.InventoryList[oldslotID];
                            item = stack?.Item;
                        }
                    }
                    else
                    {
                        if (oldRPGInv == Utils.Enums.InventoryTypes.Outfit)
                        {
                            stack = menu.Outfit.Slots[oldslotID];
                            item = stack?.Item;
                        }
                    }

                    if (item != null)
                    {
                        if (oldRPGInv == targetRPGInv) // Changement de slots
                        {
                            if (oldInventory == null) // Changement de slots dans le outfit?! 
                                return;

                            //stack.SlotIndex = slotID;
                            if (oldInventory.InventoryList[slotID] != null)
                            {
                                if (oldInventory.InventoryList[slotID].Item.id == item.id)
                                {
                                    oldInventory.InventoryList[slotID].Quantity += stack.Quantity;
                                }
                            }
                            else
                            {
                                oldInventory.InventoryList[slotID] = stack;
                            }

                            oldInventory.InventoryList[oldslotID] = null;

                            switch (oldRPGInv) // OLD Inventory
                            {
                                case InventoryTypes.Pocket:
                                    menu.Inventory = oldInventory;
                                    break;
                                case InventoryTypes.Bag:
                                    menu.Bag = oldInventory;
                                    break;
                                case InventoryTypes.Distant:
                                    menu.Distant = oldInventory;
                                    break;
                            }
                        }
                        else
                        {
                            if (stack.Item.id == ItemID.Bag && targetRPGInv != Utils.Enums.InventoryTypes.Outfit)
                            {
                                var backpack = item as Items.BagItem;

                                if (!backpack.InventoryBag.IsEmpty())
                                {
                                    await menu.CloseMenu(client);
                                    client.SendNotificationError("Votre sac n'est pas vide!");
                                    return;
                                }
                            }

                            switch (targetRPGInv) // NEW Inventory
                            {
                                case InventoryTypes.Pocket:
                                    if (!menu.Inventory.IsFull(stack.Quantity * stack.Item.weight)) // vérification si y'a de la place
                                    {
                                        if (menu.Inventory.InventoryList[slotID] != null)
                                        {
                                            if (menu.Inventory.InventoryList[slotID].Item.id == stack.Item.id)
                                            {
                                                menu.Inventory.InventoryList[slotID].Quantity += stack.Quantity;
                                            }
                                        }
                                        else
                                        {
                                            menu.Inventory.InventoryList[slotID] = stack;
                                        }
                                    }
                                    else
                                    {
                                        client.SendNotificationError("Vous n'avez pas assez de place pour faire ça");
                                        return;
                                    }

                                    break;

                                case InventoryTypes.Bag:
                                    if (!menu.Bag.IsFull(stack.Quantity * stack.Item.weight)) // vérification si y'a de la place
                                    {
                                        if (stack.Item.id == ItemID.Bag)
                                        {
                                            await menu.CloseMenu(client);
                                            client.SendNotificationError("Euh ... non!");
                                            return;
                                        }

                                        if (menu.Bag.InventoryList[slotID] != null)
                                        {
                                            if (menu.Bag.InventoryList[slotID].Item.id == stack.Item.id)
                                            {
                                                menu.Bag.InventoryList[slotID].Quantity += stack.Quantity;
                                            }
                                        }
                                        else
                                        {
                                            menu.Bag.InventoryList[slotID] = stack;
                                        }
                                    }
                                    else
                                    {
                                        client.SendNotificationError("Vous n'avez pas assez de place pour faire ça");
                                        return;
                                    }

                                    break;

                                case Utils.Enums.InventoryTypes.Distant:
                                    if (!menu.Distant.IsFull(stack.Quantity * stack.Item.weight)) // vérification si y'a de la place
                                    {
                                        if (menu.Distant.InventoryList[slotID] != null)
                                        {
                                            if (menu.Distant.InventoryList[slotID].Item.id == stack.Item.id)
                                            {
                                                menu.Distant.InventoryList[slotID].Quantity += stack.Quantity;
                                            }
                                        }
                                        else
                                        {
                                            menu.Distant.InventoryList[slotID] = stack;
                                        }
                                    }
                                    else
                                    {
                                        client.SendNotificationError("Vous n'avez pas assez de place pour faire ça");
                                        return;
                                    }

                                    break;

                                case Utils.Enums.InventoryTypes.Outfit:
                                    menu.Outfit.Slots[slotID] = stack;

                                    break;
                            }

                            if (oldInventory != null)
                                oldInventory.InventoryList[oldslotID] = null;

                            #region Clothing 
                            // Remove
                            if (oldRPGInv == Utils.Enums.InventoryTypes.Outfit)
                            {
                                switch (item.id)
                                {
                                    case ItemID.Glasses: // glasses
                                        player.Clothing.Glasses = (player.Character.Gender == 0) ? new PropData(14, 0) : new PropData(13, 0);
                                        break;

                                    case ItemID.Hats: // hair
                                        player.Clothing.Hats = (player.Character.Gender == 0) ? new PropData(121, 0) : new PropData(120, 0);
                                        break;

                                    case ItemID.Necklace: // necklace
                                        player.Clothing.Accessory = new ClothData();
                                        break;

                                    case ItemID.Mask: // mask
                                        player.Clothing.Mask = new ClothData();
                                        break;

                                    case ItemID.Ears: // earring
                                        player.Clothing.Ears = (player.Character.Gender == 0) ? new PropData(33, 0) : new PropData(12, 0);
                                        break;

                                    case ItemID.Jacket: // jacket
                                        player.Clothing.Torso = new ClothData(15, 0, 0);
                                        player.Clothing.Tops = new ClothData(15, 0, 0);
                                        break;

                                    case ItemID.Watch: // watch
                                        player.Clothing.Watches = (player.Character.Gender == 0) ? new PropData(2, 0) : new PropData(1, 0);
                                        break;

                                    case ItemID.TShirt: // shirt
                                        player.Clothing.Undershirt = (player.Character.Gender == 0) ? new ClothData(57, 0, 0) : new ClothData(34, 0, 0);
                                        break;

                                    case ItemID.Bracelet: // bracelet
                                        player.Clothing.Bracelets = new PropData(15, 0);
                                        break;

                                    case ItemID.Pant: // pants
                                        player.Clothing.Legs = (player.Character.Gender == 0) ? new ClothData(14, 0, 0) : new ClothData(15, 0, 0);
                                        break;

                                    case ItemID.Glove: // gloves
                                        player.Clothing.Glasses = (player.Character.Gender == 0) ? new PropData(6, 0) : new PropData(5, 0);
                                        break;

                                    case ItemID.Shoes: // shoes
                                        player.Clothing.Feet = (player.Character.Gender == 0) ? new ClothData(34, 0, 0) : new ClothData(35, 0, 0);
                                        break;

                                    case ItemID.Kevlar: // kevlar
                                        player.Clothing.BodyArmor = new ClothData(0, 0, 0);
                                        break;

                                    case ItemID.Bag: // backpack
                                        player.Clothing.Bags = new ClothData();
                                        player.BagInventory = null;
                                        menu.Bag = null;
                                        //await menu.CloseMenu(sender);
                                        break;

                                    case ItemID.Phone:
                                        player.PhoneSelected = null;
                                        break;

                                    case ItemID.Radio:
                                        player.RadioSelected = null;
                                        break;

                                    case ItemID.Weapon:
                                    case ItemID.LampeTorche:
                                    case ItemID.Carabine:
                                    case ItemID.Matraque:
                                    case ItemID.Bat:
                                    case ItemID.BattleAxe:
                                    case ItemID.CombatPistol:
                                    case ItemID.Flashlight:
                                    case ItemID.Hache:
                                    case ItemID.HeavyPistol:
                                    case ItemID.Knife:
                                    case ItemID.Machete:
                                    case ItemID.Musket:
                                    case ItemID.SNSPistol:
                                    case ItemID.Colt6Coup:
                                    case ItemID.Colt1911:
                                    case ItemID.Magnum357:
                                    case ItemID.Pistol50:
                                    case ItemID.Pistol:
                                    case ItemID.Pump:
                                    case ItemID.Taser:
                                        await client.RemoveAllWeaponsAsync();
                                        break;
                                }

                                player.Clothing.UpdatePlayerClothing();
                                menu.Outfit.Slots[oldslotID] = null;
                            }
                            // Equip
                            else if (targetRPGInv == Utils.Enums.InventoryTypes.Outfit)
                            {
                                var cloth = item as ClothItem;

                                switch (item.id)
                                {
                                    case ItemID.Glasses: // glasses
                                        player.Clothing.Glasses = new PropData(cloth.Clothing.Drawable, cloth.Clothing.Texture);
                                        break;

                                    case ItemID.Hats: // hair
                                        player.Clothing.Hats = new PropData(cloth.Clothing.Drawable, cloth.Clothing.Texture);
                                        break;

                                    case ItemID.Necklace: // necklace
                                        player.Clothing.Accessory = cloth.Clothing;
                                        break;

                                    case ItemID.Mask: // mask
                                        player.Clothing.Mask = cloth.Clothing;
                                        break;

                                    case ItemID.Ears: // earring
                                        player.Clothing.Ears = new PropData(cloth.Clothing.Drawable, cloth.Clothing.Texture);
                                        break;

                                    case ItemID.Jacket: // jacket
                                        player.Clothing.Tops = cloth.Clothing;

                                        int torso = 0;

                                        if (player.Character.Gender == 0)
                                            torso = Loader.ClothingLoader.ClothingsMaleTopsList.DrawablesList[cloth.Clothing.Drawable].Torso[0];
                                        else
                                            torso = Loader.ClothingLoader.ClothingsFemaleTopsList.DrawablesList[cloth.Clothing.Drawable].Torso[0];

                                        player.Clothing.Torso = new ClothData((byte)torso, 0, 0);
                                        break;

                                    case ItemID.Watch: // watch
                                        player.Clothing.Watches = new PropData(cloth.Clothing.Drawable, cloth.Clothing.Texture);
                                        break;

                                    case ItemID.TShirt: // shirt
                                        player.Clothing.Undershirt = cloth.Clothing;
                                        break;

                                    case ItemID.Bracelet: // bracelet
                                        player.Clothing.Bracelets = new PropData(cloth.Clothing.Drawable, cloth.Clothing.Texture);
                                        break;

                                    case ItemID.Pant: // pants
                                        player.Clothing.Legs = cloth.Clothing;
                                        break;

                                    case ItemID.Glove: // gloves
                                        player.Clothing.Glasses = new PropData(cloth.Clothing.Drawable, cloth.Clothing.Texture);
                                        break;

                                    case ItemID.Shoes: // shoes
                                        player.Clothing.Feet = cloth.Clothing;
                                        break;

                                    case ItemID.Kevlar: // kevlar
                                        player.Clothing.BodyArmor = cloth.Clothing;
                                        break;

                                    case ItemID.Bag: // backpack
                                        var backpack = (item) as Items.BagItem;
                                        if (backpack != null)
                                        {
                                            player.BagInventory = backpack.InventoryBag;
                                            player.Clothing.Bags = backpack.Clothing;
                                            menu.Bag = backpack.InventoryBag;
                                        }
                                        break;

                                    case ItemID.Phone:
                                        var phoneItem = (item) as Items.PhoneItem;
                                        if (phoneItem != null)
                                            player.PhoneSelected = phoneItem.PhoneHandler;
                                        break;

                                    case ItemID.Radio:
                                        var radioItem = (item) as Items.RadioItem;
                                        if (radioItem != null)
                                            player.RadioSelected = radioItem.Radio;
                                        break;

                                    case ItemID.Weapon:
                                    case ItemID.LampeTorche:
                                    case ItemID.Carabine:
                                    case ItemID.Matraque:
                                    case ItemID.Bat:
                                    case ItemID.BattleAxe:
                                    case ItemID.CombatPistol:
                                    case ItemID.Flashlight:
                                    case ItemID.Hache:
                                    case ItemID.HeavyPistol:
                                    case ItemID.Knife:
                                    case ItemID.Machete:
                                    case ItemID.Musket:
                                    case ItemID.SNSPistol:
                                    case ItemID.Colt6Coup:
                                    case ItemID.Colt1911:
                                    case ItemID.Magnum357:
                                    case ItemID.Pistol50:
                                    case ItemID.Pistol:
                                    case ItemID.Pump:
                                    case ItemID.Taser:
                                        var weaponItem = (item) as Items.Weapons;
                                        if (weaponItem != null)
                                        {
                                            await client.GiveWeaponAsync((uint)weaponItem.Hash, 99999, true);
                                        }
                                        break;
                                }

                                player.Clothing.UpdatePlayerClothing();
                            }
                            #endregion
                        }
                    }

                    if (menu.OnMove != null)
                        await menu.OnMove.Invoke(client, menu);

                    player.UpdateFull();
                    Refresh(client, menu);
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("RPGInventory_SwitchItemInventory_SRV" + ex);
            }
        }
        #endregion

        #region Split
        private async Task RPGInventory_SplitItemInventory_SRV(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            string inventoryType = Convert.ToString(args[0]);
            int itemID = Convert.ToInt32(args[1]);
            int newSlot = Convert.ToInt32(args[2]);
            int oldSlot = Convert.ToInt32(args[3]);
            int oldCount = Convert.ToInt32(args[4]);
            int newCount = Convert.ToInt32(args[5]);
            int splitCount = Convert.ToInt32(args[6]);

            RPGInventoryMenu menu;
            Inventory inv = null;

            if (_clientMenus.TryGetValue(client, out menu))
            {
                switch (inventoryType)
                {
                    case InventoryTypes.Pocket:
                        inv = menu.Inventory;
                        break;
                    case InventoryTypes.Bag:
                        inv = menu.Bag;
                        break;
                    case InventoryTypes.Distant:
                        inv = menu.Distant;
                        break;
                }

                if (inv != null)
                {
                    if (inv.InventoryList[oldSlot] != null && (oldCount == inv.InventoryList[oldSlot].Quantity && oldCount - splitCount == newCount))
                    {
                        inv.InventoryList[oldSlot].Quantity -= splitCount;

                        var cloneItem = (Models.ItemStack)inv.InventoryList[oldSlot].Clone();
                        cloneItem.Quantity = splitCount;

                        inv.InventoryList[newSlot] = cloneItem;
                        client.GetPlayerHandler()?.UpdateFull();
                    }
                }
            }
            await new RPGInventoryMenu(menu.Inventory, menu.Outfit, menu.Bag, menu.Distant).OpenMenu(client);
        }

        #endregion

        #region Price
        private async Task RPGInventory_PriceItemInventory_SRV(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            string inventoryType = Convert.ToString(args[0]);
            int itemID = Convert.ToInt32(args[1]);
            int slot = Convert.ToInt32(args[2]);
            int price = Convert.ToInt32(args[3]);

            RPGInventoryMenu menu;
            Inventory inv = null;

            if (_clientMenus.TryGetValue(client, out menu))
            {
                switch (inventoryType)
                {
                    case Utils.Enums.InventoryTypes.Pocket:
                        inv = menu.Inventory;
                        break;
                    case Utils.Enums.InventoryTypes.Bag:
                        inv = menu.Bag;
                        break;
                    case Utils.Enums.InventoryTypes.Distant:
                        inv = menu.Distant;
                        break;
                }

                if (inv != null)
                {
                    if (inv.InventoryList[slot] != null)
                    {
                        inv.InventoryList[slot].Price = price;

                        if (menu.PriceChange != null)
                            await menu.PriceChange.Invoke(client, menu, inv.InventoryList[slot], price);
                    }
                }
            }
        }
        #endregion

        #region Refresh
        public static void Refresh(IPlayer sender, RPGInventoryMenu menu)
        {
            menu.PocketsItems.RPGInventoryItems = new List<RPGInventoryItem>();

            for (int i = 0; i < menu.Inventory.InventoryList.Length; i++)
            {
                if (menu.Inventory.InventoryList[i] != null && menu.Inventory.InventoryList[i].Item != null)
                    menu.PocketsItems.RPGInventoryItems.Add(new RPGInventoryItem(menu.Inventory.InventoryList[i], InventoryTypes.Pocket, i));
            }

            if (menu.Bag != null)
            {
                menu.BagItems = new RPGInventory();
                menu.BagItems.CurrentSize = menu.Bag.CurrentSize();
                menu.BagItems.MaxSize = menu.Bag.MaxSize;
                menu.BagItems.Slots = menu.Bag.MaxSlot;
                menu.BagItems.RPGInventoryItems = new List<RPGInventoryItem>();

                for (int i = 0; i < menu.Bag.InventoryList.Length; i++)
                {
                    if (menu.Bag.InventoryList[i] != null && menu.Bag.InventoryList[i].Item != null)
                        menu.BagItems.RPGInventoryItems.Add(new RPGInventoryItem(menu.Bag.InventoryList[i], InventoryTypes.Bag, i));
                }
            }
            else
            {
                menu.BagItems = null;
            }

            if (menu.Distant != null)
            {
                bool market = false;
                if (menu.DistantItems != null)
                    market = menu.DistantItems.IsMarket;

                menu.DistantItems = new RPGInventory();
                menu.DistantItems.CurrentSize = menu.Distant.CurrentSize();
                menu.DistantItems.MaxSize = menu.Distant.MaxSize;
                menu.DistantItems.Slots = menu.Distant.MaxSlot;
                menu.DistantItems.RPGInventoryItems = new List<RPGInventoryItem>();
                menu.DistantItems.IsMarket = market;

                for (int i = 0; i < menu.Distant.InventoryList.Length; i++)
                {
                    if (menu.Distant.InventoryList[i] != null && menu.Distant.InventoryList[i].Item != null)
                        menu.DistantItems.RPGInventoryItems.Add(new RPGInventoryItem(menu.Distant.InventoryList[i], InventoryTypes.Distant, i));
                }
            }

            if (menu.Outfit != null)
            {
                menu.OutfitItems = new RPGInventoryOutfit();
                menu.OutfitItems.NamedSlots = new RPGOutfitSlots[18];
                menu.OutfitItems.RPGInventoryItems = new List<RPGInventoryItem>();
                for (int i = 0; i < menu.Outfit.Slots.Length; i++)
                {
                    if (menu.Outfit.Slots[i] != null)
                    {
                        menu.OutfitItems.NamedSlots[i] = new RPGOutfitSlots(i + 1, (menu.Outfit.Slots[i].Item != null) ? menu.Outfit.Slots[i].Item.name : "", OutfitInventory.OutfitClasses[i], true);
                        menu.OutfitItems.RPGInventoryItems.Add(new RPGInventoryItem(menu.Outfit.Slots[i], InventoryTypes.Outfit, i));
                    }
                    else
                    {
                        menu.OutfitItems.NamedSlots[i] = new RPGOutfitSlots(i + 1, "", OutfitInventory.OutfitClasses[i], true);
                    }
                }
            }

            sender.EmitLocked("InventoryManager_RefreshMenu",
                    JsonConvert.SerializeObject(menu.PocketsItems),
                    JsonConvert.SerializeObject(menu.BagItems),
                    JsonConvert.SerializeObject(menu.DistantItems),
                    JsonConvert.SerializeObject(menu.OutfitItems),
                    (menu.DistantPlayer == null) ? false : true);

        }
        #endregion

        #region Close
        private async Task RPGInventory_ClosedMenu_SRV(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            var player = client;

            if (_clientMenus.TryRemove(player, out RPGInventoryMenu menu))
                await menu.OnClose?.Invoke(player, menu);
        }
        #endregion
    }
}