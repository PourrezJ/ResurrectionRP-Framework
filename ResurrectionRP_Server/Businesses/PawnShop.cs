﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Entities.Players;

namespace ResurrectionRP_Server.Businesses
{
    public class PawnShop : Business
    {
        #region Public fields
        public Models.InventoryBox InventoryBox;
        #endregion

        #region Constructor
        public PawnShop(string businnessName, Models.Location location, uint blipSprite, int inventoryMax, Models.InventoryBox inventoryBox, PedModel pedhash = 0, string owner = null, bool buyable = true, bool onsale = true) : base(businnessName, location, blipSprite, inventoryMax, pedhash, owner, buyable, onsale)
        {
            Buyable = true;
            InventoryBox = inventoryBox;
        }
        #endregion

        #region Overrides
        public override async Task Init()
        {
            this.MaxEmployee = 5;
/*            if (InventoryBox != null) TODO
                await InventoryBox.Spawn();*/

            await base.Init();
        }
        #endregion

        #region Menus
        public override async Task OpenMenu(IPlayer client, Entities.Peds.Ped npc = null)
        {
            if (Inventory.Locked)
            {
                await client.SendNotificationError("Pawn Shop est en cours de réapprovisionnement.");
                return;
            }

            Menu _menu = new Menu("Pawn Shop", "", "Emplacements: " + Inventory.CurrentSize() + "/" + Inventory.MaxSize, 0, 0, Menu.MenuAnchor.MiddleRight, backCloseMenu: true);
            _menu.BannerSprite = Banner.Guns;

            if (!Inventory.IsEmpty())
            {
                _menu.ItemSelectCallback = StoreMenuManager;
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
                        _menu.Add(item);
                    }
                }
            }
            else
                await client.NotifyAsync("Il n'y a pas de produits en vente.");
            await MenuManager.OpenMenu(client, _menu);
        }

        public override async Task<Menu> OpenSellMenu(IPlayer client, Menu menu)
        {
            if ( IsOwner(client) ||  IsEmployee(client))
            {
                Inactivity = DateTime.Now;
                menu.ItemSelectCallback += StoreOwnerMenuManager;

                menu.Add(new MenuItem("Ajouter des produits", "", "ID_Add", true));

                if ( IsOwner(client))
                {
                    menu.Add(new MenuItem($"Gérer les finances", "", "ID_TakeMoney", true, rightLabel: $"${BankAccount.Balance}"));
                }
            }

            return await base.OpenSellMenu(client, menu);
        }
        #endregion

        #region Callbacks
        public async Task StoreOwnerMenuManager(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                await OnNpcSecondaryInteract(client, Ped);
                return;
            }

            PlayerHandler ph = client.GetPlayerHandler();

            switch (menuItem.Id)
            {
                case "ID_TakeMoney":
                    await Bank.BankMenu.OpenBankMenu(client, BankAccount, Bank.AtmType.Business, menu, StoreOwnerMenuManager);
                    break;
                case "ID_Add":
                    Inventory.Locked = true;
                    await menu.CloseMenu(client);
                    var invmenu = new Inventory.RPGInventoryMenu(ph.PocketInventory, ph.OutfitInventory, ph.BagInventory, Inventory, true);

                    invmenu.OnMove += async (p, m) =>
                    {
                        await ph.Update();
                        await Update();
                    };

                    invmenu.PriceChange += async (p, m, stack, stackprice) =>
                    {
                        await client.NotifyAsync($"Le nouveau prix de {stack.Item.name} est de ${stackprice} ");
                        await ph.Update();
                        await Update();
                    };

                    invmenu.OnClose += (p, m) =>
                    {
                        Inventory.Locked = false;
                        return Task.CompletedTask;
                    };

                    await invmenu.OpenMenu(client);
                    break;

                default:

                    break;
            }

            await Update();
            await ph.Update();
        }

        private async Task StoreMenuManager(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            try
            {
                PlayerHandler _player = client.GetPlayerHandler();
                Models.ItemStack itemStack = Inventory.InventoryList[(int)menuItem.GetData("StackIndex")];
                var selected = ((ListItem)menuItem).SelectedItem;
                var test = ((ListItem)menuItem).Items[selected];
                int quantity = Convert.ToInt32(test);
                double tax = Economy.Economy.CalculPriceTaxe((itemStack.Price * quantity), GameMode.Instance.Economy.Taxe_Market);
                double price = (itemStack.Price * quantity) + tax;

                if (_player.Money >= price)
                {
                    if (itemStack.Quantity >= quantity)
                    {
                        if (await _player.AddItem(itemStack.Item, quantity))
                        {
                            if (await _player.HasMoney(price))
                            {
                                Inventory.Delete(itemStack, quantity);
                                BankAccount.AddMoney(itemStack.Price * quantity, $"Achat de {itemStack.Item.name}", false);
                                GameMode.Instance.Economy.CaissePublique += tax;
                                await Update();
                                await client.NotifyAsync($"Vous avez acheté un/des {itemStack.Item.name}(s) pour la somme de {(itemStack.Price * quantity) + tax} dont {tax} de taxes.");
                                await OpenMenu(client);
                            }
                        }
                        else
                            await client.NotifyAsync("Vous n'avez pas la place dans votre inventaire!");
                    }
                }
                else
                    await client.NotifyAsync("Vous n'avez pas assez d'argent sur vous!");
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("StoreMenuManager: " + ex);
            }
        }
        #endregion
    }
}