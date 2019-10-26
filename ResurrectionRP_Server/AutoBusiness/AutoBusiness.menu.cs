using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Utils;
using System;
using ResurrectionRP_Server.Models;
using System.Collections.Generic;
using ResurrectionRP_Server.Entities.Players;
using AltV.Net;

namespace ResurrectionRP_Server.AutoBusiness
{
    public partial class AutoBusiness
    {

        protected void OpenMainMenu(IPlayer client, Menu oldMenu = null) {

            Menu menu = new Menu("AutoBusiness", this.name, "Que voulez-vous ?", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true);
            menu.ItemSelectCallback = MenuCallBack;

            if(buyItems.Count > 0)
                menu.Add( new MenuItem("Acheter", "Acheter quelque chose chez nous", "ID_Buy", true, rightLabel: "$$$") );
            if(sellItems.Count > 0)
                menu.Add( new MenuItem("Vendre", "Vendre quelque chose chez nous", "ID_Sell", true, rightLabel: "$") );
            if(tradeItems.Count > 0)
                menu.Add(new MenuItem("Echanger", "Un echange de bons procédés", "ID_Trade", true) );

            menu.OpenMenu(client);
        }

        protected void OpenBuyMenu(IPlayer client)
        {

            Menu menu = new Menu("AutoBusiness", this.name, "Que voulez-vous acheter ?", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true);
            menu.ItemSelectCallback = MenuCallBack;

            foreach (KeyValuePair<Item, int> p in buyItems)
            {
                MenuItem item = new MenuItem(p.Key.name, $"Acheter {p.Key.name} pour ~g~$~w~{p.Value}", "ID_BuyItem", true, rightLabel: "$" + p.Value.ToString());
                item.SetData("Item", p.Key);
                item.SetData("Price", p.Value);
                menu.Add(
                        item
                    ) ;
            }

            menu.OpenMenu(client);
        }
        
        protected void OpenSellMenu(IPlayer client)
        {

            Menu menu = new Menu("AutoBusiness", this.name, "Que voulez-vous vendre ?", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true);
            menu.ItemSelectCallback = MenuCallBack;

            foreach (KeyValuePair<Item, int> p in sellItems)
            {
                MenuItem item = new MenuItem(p.Key.name, $"Vendre {p.Key.name} pour ~g~$~w~{p.Value}", "ID_SellItem", true, rightLabel: "$" + p.Value.ToString());
                item.SetData("Item", p.Key);
                item.SetData("Price", p.Value);
                menu.Add(
                        item
                    );
            }

            menu.OpenMenu(client);
        }
        
        protected void OpenTradeMenu(IPlayer client)
        {

            Menu menu = new Menu("AutoBusiness", this.name, "Que voulez-vous échanger ?", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true);
            menu.ItemSelectCallback = MenuCallBack;

            foreach (KeyValuePair<ItemStack, ItemStack> p in tradeItems)
            {
                MenuItem item = new MenuItem($"{p.Key.Quantity} {p.Key.Item.name} pour", $"Donnez {p.Key.Quantity} {p.Key.Item.name} pour {p.Value.Quantity} {p.Value.Item.name} ", "ID_TradeItem", true, rightLabel: $"{p.Value.Quantity} {p.Value.Item.name}");
                item.SetData("Left", p.Key);
                item.SetData("Right", p.Value);
                menu.Add(
                    item
                    );
            }

            menu.OpenMenu(client);
        }


        protected void MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            Item item = null;
            int price = -1;
            PlayerHandler _client = client.GetPlayerHandler();

            MenuManager.CloseMenu(client);

            switch (menuItem.Id)
            {
                case "ID_Buy":
                    OpenBuyMenu(client);
                    break;
                case "ID_Sell":
                    OpenSellMenu(client);
                    break;
                case "ID_Trade":
                    OpenTradeMenu(client);
                    break;
                case "ID_BuyItem":
                    item = menuItem.GetData("Item");
                    price = menuItem.GetData("Price");
                    if (_client.InventoryIsFull(item.weight))
                    {
                        client.DisplayHelp("Votre inventaire est déjà plein!");
                        break;
                    }
                    if (_client.HasBankMoney((double)price, $"Achat {item.name}", true))
                    {
                        _client.AddItem(item, 1);
                        
                        client.DisplayHelp($"Vous avez acheté {item.name}, \nvous en avez {_client.CountItem(item.id)} désormais");
                        client.SendNotificationPicture(Utils.Enums.CharPicture.CHAR_DAVE, "Bijouterie", "Bonne journée!", "Content de faire affaire avec vous, revenez vite!");
                    }
                    else
                        client.DisplayHelp("Vous n'avez pas assez d'argent");
                    break;
                case "ID_SellItem":
                    item = menuItem.GetData("Item");
                    price = menuItem.GetData("Price");
                    if (_client.InventoryIsFull(item.weight))
                    {
                        client.DisplayHelp("Votre inventaire est déjà plein!");
                        break;
                    }
                    if(_client.HasItemID(item.id) )
                    {
                        _client.AddMoney((double)price);
                        _client.DeleteOneItemWithID(item.id);
                        client.DisplayHelp($"Vous avez vendu {item.name}, \nvous en avez {_client.CountItem(item.id)} désormais");
                        client.SendNotificationPicture(Utils.Enums.CharPicture.CHAR_DAVE, "Bijouterie", "Bonne journée!", "Content de faire affaire avec vous, revenez vite!");
                    } else
                        client.SendNotificationPicture(Utils.Enums.CharPicture.CHAR_DAVE, "Bijouterie", "Arnaqueur", "Vous n'avez rien à vendre ! Vous tentez de m'avoir ?");
                    break;
                case "ID_TradeItem":
                    ItemStack left = menuItem.GetData("Left");
                    ItemStack right = menuItem.GetData("Right");
                    if(_client.HasItemID(left.Item.id) && _client.CountItem(left.Item.id) >= left.Quantity)
                    {
                        if(_client.InventoryIsFull( (left.Quantity * left.Item.weight) - (right.Quantity * right.Item.weight)))
                        {
                            client.DisplayHelp("Votre inventaire est déjà plein!");
                            break;
                        }
                        _client.DeleteAllItem(left.Item.id, left.Quantity);
                        _client.AddItem(right.Item, right.Quantity);
                        client.SendNotificationPicture(Utils.Enums.CharPicture.CHAR_DAVE, "Bijouterie", "Bonne journée!", "Content de faire affaire avec vous, revenez vite!");
                    }
                    else
                        client.SendNotificationPicture(Utils.Enums.CharPicture.CHAR_DAVE, "Bijouterie", "Arnaqueur", "Vous n'avez pas ce qu'il faut ! Vous tentez de m'avoir ?");
                    break;
            }

        }
    }
}
