using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Utils;

namespace ResurrectionRP_Server.Business
{
    public class HeadShop : Business
    {
        #region Private fields
        private static ItemID[] itemsWithoutOwner = new ItemID[]
        {
            ItemID.GSkunk,
            ItemID.GWhite,
            ItemID.GOrange,
            ItemID.GPurple,
            ItemID.Hydro,
            ItemID.Secateur,
        };

        #endregion
        public HeadShop(string businnessName, Location location, uint blipSprite, int inventoryMax, PedModel pedhash = 0, string owner = null, bool buyable = true, bool onsale = true) : base(businnessName, location, blipSprite, inventoryMax, pedhash, owner, buyable, onsale)
        {
            Buyable = false;
        }

        public override void OpenMenu(IPlayer client, Ped npc)
        {
            Menu menu = new Menu("Head-Shop", "", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true);
            menu.BannerSprite = Banner.Guns;
            menu.ItemSelectCallback = StoreMenuManager;

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
            menu.OpenMenu(client);
            base.OpenMenu(client, npc);
        }

        private void StoreMenuManager(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            PlayerHandler _player = client.GetPlayerHandler();
            ItemStack itemStack = new ItemStack(itemsWithoutOwner[itemIndex], 999, LoadItem.GetItemWithID(itemsWithoutOwner[itemIndex]).itemPrice * Globals.PRICE_MULT_IF_NO_OWN);
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
                            OpenMenu(client, null);
                        }
                    }
                    else
                        client.SendNotification("Vous n'avez pas la place dans votre inventaire!");
                }
            }
            else
                client.SendNotification("Vous n'avez pas assez d'argent sur vous!");
        }
    }
}
