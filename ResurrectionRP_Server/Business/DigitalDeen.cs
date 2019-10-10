using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Bank;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Utils;
using AltV.Net.Enums;

namespace ResurrectionRP_Server.Business
{
    public class DigitalDeen : Business
    {
        #region Constructor
        public DigitalDeen(string businnessName, Location location, uint blipSprite, int inventoryMax, PedModel pedhash = 0, string owner = null, bool buyable = true, bool onsale = true) : base(businnessName, location, blipSprite, inventoryMax, pedhash, owner, buyable, onsale)
        {
            Buyable = true;
        }
        #endregion

        #region Menus
        public override void OpenMenu(IPlayer client, Entities.Peds.Ped npc = null)
        {
            if (Inventory.Locked)
            {
                client.SendNotificationError("Le Digital Den est en cours de réapprovisionnement.");
                return;
            }

            if (!Inventory.IsEmpty())
            {
                Menu menu = new Menu("DigitalDean", "", "Emplacements: " + Inventory.CurrentSize() + "/" + Inventory.MaxSize, Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true);
                menu.BannerSprite = Banner.Guns;
                menu.ItemSelectCallback = StoreMenuManager;

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

                menu.OpenMenu(client);
            }
            else
            {
                client.SendNotification("Il n'y a pas de produits en vente.");

                if (MenuManager.HasOpenMenu(client))
                    MenuManager.CloseMenu(client);
            }
        }

        public override async Task<Menu> OpenSellMenu(IPlayer client, Menu menu)
        {
            if ( IsOwner(client) ||  IsEmployee(client))
            {
                Inactivity = DateTime.Now;
                menu.ItemSelectCallback = StoreOwnerMenuManager;
                menu.Add(new MenuItem("Ajouter des produits", "", "ID_Add", true));

                if (IsOwner(client))
                    menu.Add(new MenuItem($"Gérer les finances", "", "ID_TakeMoney", true, rightLabel: $"${BankAccount.Balance}"));
            }

            return await base.OpenSellMenu(client, menu);
        }
        #endregion

        #region Callbacks
        public void StoreOwnerMenuManager(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                Task.Run(async () => { await OnNpcSecondaryInteract(client, Ped); });
                return;
            }

            PlayerHandler player = client.GetPlayerHandler() ;

            switch (menuItem.Id)
            {
                case "ID_TakeMoney":
                    BankMenu.OpenBankMenu(client, BankAccount, AtmType.Business, menu, StoreOwnerMenuManager);
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
                        client.SendNotification($"Le nouveau prix de {stack.Item.name} est de ${stackprice} ");
                        player.UpdateFull();
                        UpdateInBackground();
                    };

                    invmenu.OnClose += (p, m) =>
                    {
                        Inventory.Locked = false;
                    };

                    invmenu.OpenMenu(client);
                    break;

                default:
                    break;
            }
        }

        private void StoreMenuManager(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            try
            {
                PlayerHandler _player = client.GetPlayerHandler();
                ItemStack itemStack = Inventory.InventoryList[(int)menuItem.GetData("StackIndex")];
                var selected = ((ListItem)menuItem).SelectedItem;
                var test = ((ListItem)menuItem).Items[selected];
                int quantity = Convert.ToInt32(test);
                double tax = Economy.Economy.CalculPriceTaxe(itemStack.Price * quantity, GameMode.Instance.Economy.Taxe_Market);
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
                                BankAccount.AddMoney(itemStack.Price * quantity, $"Achat de {itemStack.Item.name}", false);
                                GameMode.Instance.Economy.CaissePublique += tax;
                                UpdateInBackground();
                                client.SendNotification($"Vous avez acheté un/des {itemStack.Item.name}(s) pour la somme de {(itemStack.Price * quantity) + tax} dont {tax} de taxes.");
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
            catch (Exception ex)
            {
                Alt.Server.LogError ("StoreMenuManager: " + ex);
            }
        }

        private void StoreGetMenuManager(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex, dynamic data)
        {
            PlayerHandler _player = client.GetPlayerHandler();
            ItemStack itemStack = Inventory.InventoryList[itemIndex];
            int quantity;
            if (int.TryParse(Convert.ToString(data[menuItem.Text]["Value"]), out quantity) &&
                _player.PocketInventory.AddItem(client, itemStack.Item, quantity))
            {
                Inventory.Delete(itemStack, quantity);
            }
            else
            {
                client.SendNotification("Vous n'avez pas la place dans votre inventaire!");
                return;
            }

            UpdateInBackground();
            if (MenuManager.HasOpenMenu(client))
                MenuManager.CloseMenu(client);
        }
        #endregion
    }
}
