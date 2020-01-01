using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using PropData = ResurrectionRP_Server.Models.PropData;
using ClothData = ResurrectionRP_Server.Models.ClothData;
using ItemID = ResurrectionRP_Server.Models.InventoryData.ItemID;
using Newtonsoft.Json;
using ResurrectionRP_Server.Utils.Enums;
using System.Numerics;
using ResurrectionRP_Server.Entities.Players;

namespace ResurrectionRP_Server.Inventory
{
    public static class RPGInventoryManager
    {
        #region Private static properties
        private static ConcurrentDictionary<IPlayer, RPGInventoryMenu> _clientMenus = new ConcurrentDictionary<IPlayer, RPGInventoryMenu>();
        #endregion

        #region Init
        public static void Init()
        {
            Alt.OnClient("RPGInventory_UseItem", RPGInventory_UseItem);
            Alt.OnClient("RPGInventory_DropItem", RPGInventory_DropItem);
            Alt.OnClient("RPGInventory_GiveItem", RPGInventory_GiveItem);
            Alt.OnClient("RPGInventory_ClosedMenu_SRV", RPGInventory_ClosedMenu_SRV);
            Alt.OnClient("RPGInventory_PriceItemInventory_SRV", RPGInventory_PriceItemInventory_SRV);

            Alt.OnClient("RPGInventory_SplitItemInventory_SRV", RPGInventory_SplitItemInventory_SRV);
            Alt.OnClient("RPGInventory_SwitchItemInventory_SRV", RPGInventory_SwitchItemInventory_SRV);
        }
        #endregion

        #region Server Events
        public static void OnPlayerQuit(IPlayer sender)
        {
            if (HasInventoryOpen(sender))
                CloseMenu(sender);
        }
        #endregion

        #region Public static methods
        public static void CloseMenu(IPlayer client, RPGInventoryMenu oldmenu = null)
        {
            RPGInventoryMenu menu = null;
            if (oldmenu != null || _clientMenus.TryRemove(client, out menu))
            {
                if (menu != null)
                    menu.OnClose?.Invoke(client, menu);

                if (!client.Exists)
                    return;

                client.EmitLocked("InventoryManager_CloseMenu");
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

        public static bool OpenMenu(IPlayer client, RPGInventoryMenu menu)
        {
            _clientMenus.TryRemove(client, out _);

            if (_clientMenus.TryAdd(client, menu))
            {
                client.EmitLocked("InventoryManager_OpenMenu",
                    JsonConvert.SerializeObject(menu.PocketsItems),
                    JsonConvert.SerializeObject(menu.BagItems),
                    JsonConvert.SerializeObject(menu.DistantItems),
                    JsonConvert.SerializeObject(menu.OutfitItems),
                    (menu.DistantPlayer == null) ? false : true);
            }

            return false;
        }
        #endregion

        #region Add Physical
        public static void RPGInventory_SetPlayerProps(IPlayer client, Models.Item item)
        {
            PlayerHandler player = client.GetPlayerHandler();
            _clientMenus.TryGetValue(client, out RPGInventoryMenu menu);
            Models.Attachment attach;
            if(item is ClothItem cloth) // CLOTHING STUFF
            {
                try {
                    switch(item.id) {
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

                            if(player.Character.Gender == 0)
                                torso = Loader.ClothingLoader.ClothingsMaleTopsList.DrawablesList[cloth.Clothing.Drawable].Torso[0];
                            else
                                torso = Loader.ClothingLoader.ClothingsFemaleTopsList.DrawablesList[cloth.Clothing.Drawable].Torso[0];

                            player.Clothing.Torso = new ClothData((byte) torso, 0, 0);
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
                    }
                } catch(Exception ex) {
                    Alt.Server.LogError("[RPGInventorymanager.RPGInventory_SetPlayerProps] Clothing " + ex.Message);
                }

            } else if((item) is Items.Weapons weapons) {
                try {
                    switch(weapons.id) {
                        case ItemID.Weapon:
                        case ItemID.LampeTorche:
                        case ItemID.Carabine:
                        case ItemID.Matraque:
                        case ItemID.Bat:
                        case ItemID.BattleAxe:
                        case ItemID.CombatPistol:
                        case ItemID.Flashlight:
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
                            if(GameMode.IsDebug)
                                Alt.Server.LogInfo("[RPGInventoryManager.RPGInventory_SetPlayerProps()] Giving a weapon (" + weapons.name + ") to " + client.GetPlayerHandler().PID);

                            client.GiveWeapon((uint) weapons.Hash, 99999, true);
                            break;
                    }
                } catch(Exception ex) {
                    Alt.Server.LogError("[RPGInventorymanager.RPGInventory_SetPlayerProps] Weapons " + ex.Message);
                }
            } else {
                try {
                    switch(item.id) {
                        case ItemID.Bag: // backpack
                            if((item) is Items.BagItem backpack && backpack.InventoryBag != null) {
                                player.BagInventory = backpack.InventoryBag;
                                player.Clothing.Bags = backpack.Clothing;
                                player.BagInventory = backpack.InventoryBag;

                            }
                            break;

                        case ItemID.Phone:
                            if((item) is Items.PhoneItem phoneItem && phoneItem.PhoneHandler != null) {
                                player.PhoneSelected = phoneItem.PhoneHandler;
                            }
                            break;

                        case ItemID.Radio:
                            if((item) is Items.RadioItem radioItem)
                                player.RadioSelected = radioItem.Radio;
                            break;
                            //V2 - Outfit Inventory n'est initié qu'à une première interaction joueur avec l'inventaire
/*                        case ItemID.Pioche:
                            if(menu.Outfit.prop != null)
                                menu.Outfit.DestroyProp();
                            attach = new Models.Attachment()
                            {
                                Bone = "PH_R_Hand",
                                PositionOffset = new Vector3(0.1f, -0.1f, -0.02f),
                                RotationOffset = new Vector3(80, 0, 170),
                                Type = (int) Streamer.Data.EntityType.Ped,
                                RemoteID = client.Id
                            };
                            menu.Outfit.prop = Entities.Objects.WorldObject.CreateObject((int) Alt.Hash("prop_tool_pickaxe"), client.Position.ConvertToVector3(), new System.Numerics.Vector3(), attach, false);
                            break;
                        case ItemID.Hache:
                            if(menu.Outfit.prop != null)
                                menu.Outfit.DestroyProp();
                            attach = new Models.Attachment()
                            {
                                Bone = "PH_R_Hand",
                                PositionOffset = new Vector3(0.1f, -0.1f, -0.02f),
                                RotationOffset = new Vector3(80, 0, 180),
                                Type = (int) Streamer.Data.EntityType.Ped,
                                RemoteID = client.Id
                            };
                            menu.Outfit.prop = Entities.Objects.WorldObject.CreateObject((int) Alt.Hash("prop_tool_fireaxe"), client.Position.ConvertToVector3(), new System.Numerics.Vector3(), attach, false);

                            break;
                        case ItemID.MarteauPiqueur:
                            if(menu.Outfit.prop != null)
                                menu.Outfit.DestroyProp();
                            menu.Outfit.prop = Entities.Objects.WorldObject.CreateObject((int) Alt.Hash("prop_tool_jackham"), client.Position.ConvertToVector3(), new System.Numerics.Vector3(), false);
                            (item as Items.Tool).JackHammerSetWalkingStyle(client, menu.Outfit.prop);
                            break;

                        case ItemID.Pelle:
                            if(menu.Outfit.prop != null)
                                menu.Outfit.DestroyProp();
                            break;
                        case ItemID.Marteau:
                            if(menu.Outfit.prop != null)
                                menu.Outfit.DestroyProp();
                            attach = new Models.Attachment()
                            {
                                Bone = "PH_R_Hand",
                                PositionOffset = new Vector3(0.1f, 0.1f, 0),
                                RotationOffset = new Vector3(80, 0, 180),
                                Type = (int) Streamer.Data.EntityType.Ped,
                                RemoteID = client.Id
                            };
                            menu.Outfit.prop = Entities.Objects.WorldObject.CreateObject((int) Alt.Hash("prop_tool_mallet"), client.Position.ConvertToVector3(), new System.Numerics.Vector3(), attach, false);
                            break;
                        case ItemID.CrateTool:
                            if(menu.Outfit.prop != null)
                                menu.Outfit.DestroyProp();
                            attach = new Models.Attachment()
                            {
                                Bone = "PH_R_Hand",
                                PositionOffset = new Vector3(0, 0, 0),
                                RotationOffset = new Vector3(90, 0, 0),
                                Type = (int) Streamer.Data.EntityType.Ped,
                                RemoteID = client.Id
                            };
                            menu.Outfit.prop = Entities.Objects.WorldObject.CreateObject((int) Alt.Hash("gr_prop_gr_tool_box_01a"), client.Position.ConvertToVector3(), new System.Numerics.Vector3(), attach, false);
                            break;*/
                    }
                } catch(Exception ex) {
                    Alt.Server.LogError("[RPGInventorymanager.RPGInventory_SetPlayerProps] Others " + ex.Message);
                }
            }

            player.Clothing.UpdatePlayerClothing();
            
           
        }
        #endregion

        #region Remove physical
        public static void RPGInventory_DeletePlayerProps(IPlayer client, Models.Item item)
        {
            PlayerHandler ph = client.GetPlayerHandler();
            _clientMenus.TryGetValue(client, out RPGInventoryMenu menu);

            if(item is ClothItem cloth) {
                switch(item.id) {
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
                }
            } else if((item) is Items.Weapons weapons) {
                switch(item.id) {
                    case ItemID.Weapon:
                    case ItemID.LampeTorche:
                    case ItemID.Carabine:
                    case ItemID.Matraque:
                    case ItemID.Bat:
                    case ItemID.BattleAxe:
                    case ItemID.CombatPistol:
                    case ItemID.Flashlight:
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
                        client.RemoveAllWeapons();
                        break;
                }
            } else {
                switch(item.id) {
                    case ItemID.Phone:
                        if((item) is Items.PhoneItem phoneItem && phoneItem.PhoneHandler != null) {
                            ph.PhoneSelected = null;
                        }
                        break;
                    case ItemID.Bag: // backpack
                        ph.Clothing.Bags = new ClothData();
                        ph.BagInventory = null;
                        menu.Bag = null;
                        menu.BagItems = null;
                        //await menu.CloseMenu(sender);
                        break;

                    case ItemID.Hache:
                    case ItemID.Pioche:
                    case ItemID.Marteau:
                    case ItemID.MarteauPiqueur:
                    case ItemID.Pelle:
                    case ItemID.CrateTool:
                        menu.Outfit.DestroyProp();
                        client.StopAnimation();
                        break;
                }
            }
            ph.Clothing.UpdatePlayerClothing();
        }
        #endregion

            #region Use
            private static void RPGInventory_UseItem(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            if (args.Length != 3)
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
                    case InventoryTypes.Pocket:
                        itemStack = menu.Inventory.InventoryList[itemSlot];
                        break;

                    case InventoryTypes.Bag:
                        itemStack = menu.Bag.InventoryList[itemSlot];
                        break;

                    case InventoryTypes.Distant:
                        itemStack = menu.Distant.InventoryList[itemSlot];
                        break;
                }

                if (itemStack != null && itemStack.Item != null)
                    itemStack.Item.Use(client, targetInventory, itemSlot);

                Refresh(client, menu);
            }
        }
        #endregion

        #region Drop
        private static void RPGInventory_DropItem(IPlayer client, object[] args)
        {
            try
            {
                if (args.Length != 4)
                    return;

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

                            if (invItem.stack.Item.Drop(client, quantity, slot, menu.Inventory))
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

                            if (invItem.stack.Item.Drop(client, quantity, slot, menu.Bag))
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

                            if (invItem.stack.Item.Drop(client, quantity, slot, menu.Distant))
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

                            if (invItem.stack.Item.Drop(client, quantity, slot, menu.Outfit))
                            {
                                if (invItem.stack.Quantity == 0)
                                    menu.PocketsItems.RPGInventoryItems.Remove(invItem);
                                else
                                    invItem.quantity = invItem.stack.Quantity;
                            }
                            RPGInventory_DeletePlayerProps(client, invItem.stack.Item);

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
        private static void RPGInventory_GiveItem(IPlayer client, object[] args)
        {
            try
            {
                if (args.Length != 4)
                    return;

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
                        case InventoryTypes.Pocket:
                            invItem = menu.PocketsItems.RPGInventoryItems.Find(s => s.inventorySlot == slot);
                            if (invItem == null)
                                return;

                            if (phDistant.AddItem(invItem.stack.Item, quantity))
                            {
                                phDistant.Client.SendNotification($"On vous a donné {quantity} {invItem.stack.Item.name}");
                                ph.DeleteItem(slot, inventoryType, quantity);
                            }

                            break;
                        case InventoryTypes.Bag:
                            invItem = menu.BagItems.RPGInventoryItems.Find(s => s.inventorySlot == slot);
                            if (invItem == null)
                                return;
                            if (phDistant.AddItem(invItem.stack.Item, quantity))
                            {
                                phDistant.Client.SendNotification($"On vous a donné {quantity} {invItem.stack.Item.name}");
                                ph.DeleteItem(slot, inventoryType, quantity);
                            }

                            break;
                        case InventoryTypes.Distant:
                            invItem = menu.DistantItems.RPGInventoryItems.Find(s => s.inventorySlot == slot);
                            if (invItem == null)
                                return;
                            if (phDistant.AddItem(invItem.stack.Item, quantity))
                            {
                                phDistant.Client.SendNotification($"On vous a donné {quantity} {invItem.stack.Item.name}");
                                ph.DeleteItem(slot, inventoryType, quantity);
                            }
                            break;
                        case InventoryTypes.Outfit:
                            invItem = menu.OutfitItems.RPGInventoryItems.Find(s => s.inventorySlot == slot);
                            if (invItem == null)
                                return;
                            if (phDistant.AddItem(invItem.stack.Item, quantity))
                            {
                                phDistant.Client.SendNotification($"On vous a donné {quantity} {invItem.stack.Item.name}");
                                ph.DeleteItem(slot, inventoryType, quantity);
                            }
                            RPGInventory_SetPlayerProps(client, invItem.stack.Item);

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
        private static void RPGInventory_SwitchItemInventory_SRV(IPlayer client, object[] args)
        {
            try
            {
                if (args.Length != 5)
                    return;

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
                        if (oldRPGInv == InventoryTypes.Outfit)
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
                            if (stack.Item.id == ItemID.Bag && targetRPGInv != InventoryTypes.Outfit)
                            {
                                var backpack = item as Items.BagItem;

                                if (!backpack.InventoryBag.IsEmpty())
                                {
                                    menu.CloseMenu(client);
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
                                            menu.CloseMenu(client);
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

                                case InventoryTypes.Distant:
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

                                case InventoryTypes.Outfit:
                                    menu.Outfit.Slots[slotID] = stack;

                                    break;
                            }

                            if (oldInventory != null)
                                oldInventory.InventoryList[oldslotID] = null;

                            #region Clothing 
                            // Remove
                            Models.Attachment attach = null;
                            if (oldRPGInv == InventoryTypes.Outfit)
                            {
                                RPGInventory_DeletePlayerProps(client, item);
                                
                                menu.Outfit.Slots[oldslotID] = null;
                            }
                            // Equip
                            else if (targetRPGInv == InventoryTypes.Outfit)
                            {
                                var cloth = item as ClothItem;

                                RPGInventory_SetPlayerProps(client, item);

                            }
                            #endregion
                        }
                    }

                    menu.OnMove?.Invoke(client, menu);
                    Refresh(client, menu);
                    player.UpdateFull();
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError($"RPGInventory_SwitchItemInventory_SRV lenght: {args.Length}" + ex);
            }
        }
        #endregion

        #region Split
        private static void RPGInventory_SplitItemInventory_SRV(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            if (args.Length < 7)
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

                try
                {
                    if (inv != null && inv.InventoryList[oldSlot] != null && oldCount == inv.InventoryList[oldSlot].Quantity && oldCount - splitCount == newCount)
                    {
                        inv.InventoryList[oldSlot].Quantity -= splitCount;

                        var cloneItem = (Models.ItemStack)inv.InventoryList[oldSlot].Clone();
                        cloneItem.Quantity = splitCount;

                        inv.InventoryList[newSlot] = cloneItem;
                        client.GetPlayerHandler().UpdateFull();
                    }
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError($"[RPGInventoryManager.RPGInventory_SplitItemInventory_SRV()] inventoryType: {inventoryType}, itemID: {itemID}, newSlot: {newSlot}, oldSlot: {oldSlot}, oldCount: {oldCount}, newCount: {newCount}, splitCount: {splitCount}, inv.InventoryList.Length: {inv.InventoryList.Length} - {ex}");
                }
            }

            new RPGInventoryMenu(menu.Inventory, menu.Outfit, menu.Bag, menu.Distant).OpenMenu(client);
        }
        #endregion

        #region Price
        private static void RPGInventory_PriceItemInventory_SRV(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            if (args.Length != 4)
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
                    if (inv.InventoryList[slot] != null)
                    {
                        inv.InventoryList[slot].Price = price;
                        menu.PriceChange?.Invoke(client, menu, inv.InventoryList[slot], price);
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
                menu.BagItems = new RPGInventory
                {
                    CurrentSize = menu.Bag.CurrentSize(),
                    MaxSize = menu.Bag.MaxSize,
                    Slots = menu.Bag.MaxSlot,
                    RPGInventoryItems = new List<RPGInventoryItem>()
                };

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

                menu.DistantItems = new RPGInventory
                {
                    CurrentSize = menu.Distant.CurrentSize(),
                    MaxSize = menu.Distant.MaxSize,
                    Slots = menu.Distant.MaxSlot,
                    RPGInventoryItems = new List<RPGInventoryItem>(),
                    IsMarket = market
                };

                for (int i = 0; i < menu.Distant.InventoryList.Length; i++)
                {
                    if (menu.Distant.InventoryList[i] != null && menu.Distant.InventoryList[i].Item != null)
                        menu.DistantItems.RPGInventoryItems.Add(new RPGInventoryItem(menu.Distant.InventoryList[i], InventoryTypes.Distant, i));
                }
            }

            if (menu.Outfit != null)
            {
                menu.OutfitItems = new RPGInventoryOutfit
                {
                    NamedSlots = new RPGOutfitSlots[18],
                    RPGInventoryItems = new List<RPGInventoryItem>()
                };
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
        private static void RPGInventory_ClosedMenu_SRV(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            var player = client;

            if (_clientMenus.TryRemove(player, out RPGInventoryMenu menu))
                menu.OnClose?.Invoke(player, menu);
        }
        #endregion
    }
}