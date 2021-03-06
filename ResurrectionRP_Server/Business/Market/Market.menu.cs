﻿using AltV.Net;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Business
{

    public partial class Market
    {   
        #region Menus
        public override void OpenMenu(IPlayer client, Entities.Peds.Ped npc = null)
        {
            if (Inventory.Locked)
            {
                client.SendNotificationError("La supérette est en cours de réapprovisionnement.");
                return;
            }

            Menu menu = new Menu("SuperMarket", "", "Emplacements: " + Inventory.CurrentSize() + "/" + Inventory.MaxSize, Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true);
            menu.BannerSprite = Banner.Convenience;
            menu.ItemSelectCallback = MarketMenuManager;

            if (Owner == null)
            {
                foreach (var loadedItem in itemsWithoutOwner)
                {
                    List<object> values = new List<object>();
                    for (int i = 1; i <= 100; i++)
                        values.Add(i.ToString());

                    var item = LoadItem.GetItemWithID(loadedItem);

                    if (item == null)
                        continue;

                    double gettaxe = Economy.Economy.CalculPriceTaxe(item.itemPrice * Globals.PRICE_MULT_IF_NO_OWN, GameMode.Instance.Economy.Taxe_Market);
                    ListItem listitem = new ListItem(item.name + " ($ " + ((item.itemPrice * Globals.PRICE_MULT_IF_NO_OWN) + gettaxe).ToString() + ")", item.description, "item_" + item.name, values, 0);
                    listitem.ExecuteCallback = true;
                    menu.Add(listitem);
                }
            }
            else if (!Inventory.IsEmpty())
            {
                for (int a = 0; a < Inventory.InventoryList.Length; a++)
                {
                    var inv = Inventory.InventoryList[a];

                    if (inv != null)
                    {
                        List<object> values = new List<object>();
                        for (int i = 1; i <= inv.Quantity; i++) values.Add(i.ToString());
                        double gettaxe = Economy.Economy.CalculPriceTaxe(inv.Price, GameMode.Instance.Economy.Taxe_Market);
                        ListItem item = new ListItem(inv.Item.name + " ($ " + (inv.Price + gettaxe).ToString() + ")", inv.Item.description, "item_" + inv.Item.name, values, 0);
                        item.ExecuteCallback = true;
                        item.SetData("StackIndex", a);
                        menu.Add(item);
                    }
                }
            }
            else
            {
                client.SendNotification("Il n'y a pas de produits en vente.");

                if (MenuManager.HasOpenMenu(client))
                    MenuManager.CloseMenu(client);
                return;
            }

            menu.OpenMenu(client);
        }

        public override Menu OpenSellMenu(IPlayer client, Menu menu)
        {
            var ph = client.GetPlayerHandler();
            menu.ItemSelectCallback = MarketOwnerMenuManager;

            if (IsOwner(client) || IsEmployee(client))
            {
                Inactivity = DateTime.Now;
                
                menu.Items.AddRange(new List<MenuItem>()
                {
                    new MenuItem("Ajouter des produits", "", "ID_Add", true),
                    new MenuItem("Statut des pompes à essence", "", "ID_Stats", true)
                });
               
                MenuItem _item = new MenuItem("Prix de revente de l'essence", "", "ID_EssencePrice", true, false, $"${Station.EssencePrice} + ${GameMode.Instance.Economy.Taxe_Essence}");
                _item.SetInput(Station.EssencePrice.ToString(), 10, InputType.UFloat);
                menu.Add(_item);

                if ( IsOwner(client))
                    menu.Add(new MenuItem($"Gérer les finances", "", "ID_TakeMoney", true, rightLabel: $"${BankAccount.Balance}"));
            }

            if (ph != null && ph.StaffRank >= Utils.Enums.StaffRank.Moderator)
            {
                MenuItem menuitem = new MenuItem("[STAFF] Définir le niveau d'essence", $"Niveau actuel {Station.Litrage}L / {Station.LitrageMax}L", "ID_StaffEssence", true, false);
                menuitem.SetInput(Station.Litrage.ToString(), 10, InputType.UFloat);
                menu.Add(menuitem);

                menu.Add(new MenuItem("[STAFF] Ouvrir l'inventaire", "", "ID_Add", true));
            }
            return base.OpenSellMenu(client, menu);
        }
        #endregion

        #region Callbacks
        public void MarketOwnerMenuManager(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                OnNpcSecondaryInteract(client, Ped);
                return;
            }

            PlayerHandler player = client.GetPlayerHandler();

            switch (menuItem.Id)
            {
                case "ID_TakeMoney":
                    Bank.BankMenu.OpenBankMenu(client, BankAccount, Bank.AtmType.Business, menu, MarketOwnerMenuManager);
                    break;

                case "ID_Add":
                    menu.CloseMenu(client);
                    Inventory.Locked = true;
                    var invmenu = new Inventory.RPGInventoryMenu(player.PocketInventory, player.OutfitInventory, player.BagInventory, Inventory, true);

                    invmenu.OnMove += (p, m) =>
                    {
                        player.UpdateFull();
                        UpdateInBackground();
                    };

                    invmenu.PriceChange += (p, m, stack, stackprice) =>
                    {
                        client.SendNotification($"Le nouveau prix de {stack.Item.name} est de ${stackprice}");
                        player.UpdateFull();
                        UpdateInBackground();
                    };

                    invmenu.OnClose += (p, m) =>
                    {
                        Inventory.Locked = false;
                    };
                    invmenu.OpenMenu(client);
                    break;

                case "ID_Stats":
                    string msg = $"Réservoir: {Station.Litrage} / {Station.LitrageMax} litre(s)";
                    client.SendNotification(msg);
                    break;

                case "ID_EssencePrice":
                    if (int.TryParse(menuItem.InputValue, out int price))
                    {
                        Station.EssencePrice = price;
                        UpdateInBackground();
                        client.SendNotification($"Le nouveau prix de l'essence est de ${Station.EssencePrice + GameMode.Instance.Economy.Taxe_Essence} dont ${GameMode.Instance.Economy.Taxe_Essence} de taxe.");
                        OnNpcSecondaryInteract(client, Ped);
                    }
                    break;

                case "ID_StaffEssence":
                    if (int.TryParse(menuItem.InputValue, out int quantity))
                    {
                        Station.Litrage = quantity;
                        UpdateInBackground();
                        client.SendNotification($"Vous avez défini le litrage de la station à {quantity.ToString()}");
                        OnNpcSecondaryInteract(client, Ped);
                    }
                    break;
                default:
                    break;
            }
        }

        private void MarketMenuManager(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            PlayerHandler _player = client.GetPlayerHandler();
            Models.ItemStack itemStack = Owner != null ? Inventory.InventoryList[(int)menuItem.GetData("StackIndex")] : new Models.ItemStack(itemsWithoutOwner[itemIndex], 999, LoadItem.GetItemWithID(itemsWithoutOwner[itemIndex]).itemPrice * Globals.PRICE_MULT_IF_NO_OWN);
            var selected = ((ListItem)menuItem).SelectedItem;
            int quantity = Convert.ToInt32(((ListItem)menuItem).Items[selected]);
            double tax = Economy.Economy.CalculPriceTaxe((itemStack.Price * quantity), GameMode.Instance.Economy.Taxe_Market);
            double price = (itemStack.Price * quantity) + tax;

            if (_player.Money >= price)
            {
                if (itemStack.Quantity >= quantity)
                {
                    if (_player.AddItem(itemStack.Item, quantity))
                    {
                        if (_player.HasMoney(price))
                        {
                            Inventory.Delete(itemStack, quantity); 
                            if (Owner != null)
                                BankAccount.AddMoney(itemStack.Price * quantity, $"Achat de {itemStack.Item.name}", false);
                            GameMode.Instance.Economy.CaissePublique += tax;
                            UpdateInBackground();
                            client.SendNotification($"Vous avez acheté un / des {itemStack.Item.name}(s) pour la somme de {(itemStack.Price * quantity) + tax} dont {tax} de taxes.");
                            OpenMenu(client);
                        }
                    }
                    else
                        client.SendNotification("Vous n'avez pas la place dans votre inventaire!");
                }
            }
            else
                client.SendNotification("Vous n'avez pas assez d'argent sur vous!");
        }
        #endregion
    }
}
