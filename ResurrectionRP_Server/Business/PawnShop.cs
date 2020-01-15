using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Business
{
    public class PawnShop : Business
    {
        #region Private fields
        private static ItemID[] itemsWithoutOwner = new ItemID[]
        {
            ItemID.Pioche,
            ItemID.Bag,
            ItemID.Marteau
        };
        #endregion

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
        public override void Init()
        {
            this.MaxEmployee = 5;
            if (InventoryBox != null) 
                InventoryBox.Spawn();

            base.Init();
        }
        #endregion

        #region Menus
        public override void OpenMenu(IPlayer client, Entities.Peds.Ped npc = null)
        {
            if (Inventory.Locked)
            {
                client.SendNotificationError("Pawn Shop est en cours de réapprovisionnement.");
                return;
            }

            Menu menu = new Menu("Pawn Shop", "", "Emplacements: " + Inventory.CurrentSize() + "/" + Inventory.MaxSize, Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true);
            menu.BannerSprite = Banner.Guns;
            menu.ItemSelectCallback = StoreMenuManager;

            if (Owner == null)
            {
                foreach (var loadedItem in itemsWithoutOwner)
                {
                    List<object> values = new List<object>();
                    for (int i = 1; i <= 100; i++)
                        values.Add(i.ToString());

                    var item = ResurrectionRP_Server.Inventory.Inventory.ItemByID(loadedItem);

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
            if ( IsOwner(client) ||  IsEmployee(client))
            {
                Inactivity = DateTime.Now;
                menu.ItemSelectCallback = StoreOwnerMenuManager;

                menu.Add(new MenuItem("Ajouter des produits", "", "ID_Add", true));

                if ( IsOwner(client))
                {
                    menu.Add(new MenuItem($"Gérer les finances", "", "ID_TakeMoney", true, rightLabel: $"${BankAccount.Balance}"));
                }
            }

            return base.OpenSellMenu(client, menu);
        }
        #endregion

        #region Callbacks
        public void StoreOwnerMenuManager(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                OnNpcSecondaryInteract(client, Ped);
                return;
            }

            PlayerHandler ph = client.GetPlayerHandler();

            switch (menuItem.Id)
            {
                case "ID_TakeMoney":
                    Bank.BankMenu.OpenBankMenu(client, BankAccount, Bank.AtmType.Business, menu, StoreOwnerMenuManager);
                    break;

                case "ID_Add":
                    menu.CloseMenu(client);
                    Inventory.Locked = true;
                    var invmenu = new Inventory.RPGInventoryMenu(ph.PocketInventory, ph.OutfitInventory, ph.BagInventory, Inventory, true);

                    invmenu.OnMove += (p, m) =>
                    {
                        ph.UpdateFull();
                        UpdateInBackground();
                    };

                    invmenu.PriceChange += (p, m, stack, stackprice) =>
                    {
                        client.SendNotification($"Le nouveau prix de {stack.Item.name} est de ${stackprice} ");
                        ph.UpdateFull();
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
        #endregion
    }
}
