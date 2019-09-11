using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Economy;
using AltV.Net.Elements.Entities;

namespace ResurrectionRP_Server.Businesses
{

    public partial class Market
    {
        #region Menus
        public override async Task OpenMenu(IPlayer client, Entities.Peds.Ped npc = null)
        {
            Alt.Server.LogError("Are in open menu");
            if (Inventory.Locked)
            {
                client.SendNotificationError("La supérette est en cours de réapprovisionnement.");
                return;
            }

            Menu _menu = new Menu("SuperMarket", "", "Emplacements: " + Inventory.CurrentSize() + "/" + Inventory.MaxSize, 0, 0, Menu.MenuAnchor.MiddleRight, backCloseMenu: true);
            _menu.BannerSprite = Banner.Convenience;

            if (!Inventory.IsEmpty())
            {
                _menu.ItemSelectCallback = MarketMenuManager;
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
                client.SendNotification("Il n'y a pas de produits en vente.");
            await MenuManager.OpenMenu(client, _menu);
        }

        public override async Task<Menu> OpenSellMenu(IPlayer client, Menu menu)
        {
            if ( IsOwner(client) ||  IsEmployee(client))
            {
                Inactivity = DateTime.Now;
                menu.ItemSelectCallback += MarketOwnerMenuManager;

                menu.Items.AddRange(new List<MenuItem>()
                {
                    new MenuItem("Ajouter des produits", "", "ID_Add", true),
                    new MenuItem("Statut des pompes à essence", "", "ID_Stats", true)
                });

                MenuItem _item = new MenuItem("Prix de l'essence", "", "ID_EssencePrice", true, false, $"${EssencePrice} + ${GameMode.Instance.Economy.Taxe_Essence}");
                _item.SetInput(EssencePrice.ToString(), 10, InputType.UFloat);
                menu.Add(_item);

                if ( IsOwner(client))
                    menu.Add(new MenuItem($"Gérer les finances", "", "ID_TakeMoney", true, rightLabel: $"${BankAccount.Balance}"));
            }

            return await base.OpenSellMenu(client, menu);
        }
        #endregion

        #region Callbacks
        public async Task MarketOwnerMenuManager(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                await OnNpcSecondaryInteract(client, Ped);
                return;
            }

            Entities.Players.PlayerHandler player = client.GetPlayerHandler();

            switch (menuItem.Id)
            {
                case "ID_TakeMoney":
                    await Bank.BankMenu.OpenBankMenu(client, BankAccount, Bank.AtmType.Business, menu, MarketOwnerMenuManager);
                    break;
                case "ID_Add":
                    await menu.CloseMenu(client);
                    var invmenu = new Inventory.RPGInventoryMenu(player.PocketInventory, player.OutfitInventory, player.BagInventory, Inventory, true);
                    Inventory.Locked = true;
                    invmenu.OnMove += async (p, m) =>
                    {
                        await player.Update();
                        await Update();
                    };

                    invmenu.PriceChange += async (p, m, stack, stackprice) =>
                    {
                        client.SendNotification($"Le nouveau prix de {stack.Item.name} est de ${stackprice}");
                        await player.Update();
                        await Update();
                    };

                    invmenu.OnClose += (p, m) =>
                    {
                        Inventory.Locked = false;
                        return Task.CompletedTask;
                    };
                    await invmenu.OpenMenu(client);
                    break;

                case "ID_Stats":
                    string msg = $"Réservoir: {Litrage} / {LitrageMax} litre(s)";
                    client.SendNotification(msg);
                    break;

                case "ID_EssencePrice":
                    if (int.TryParse(menuItem.InputValue, out int price))
                    {
                        EssencePrice = price;
                        await Update();
                        client.SendNotification($"Le nouveau prix de l'essence est de ${EssencePrice + GameMode.Instance.Economy.Taxe_Essence} dont ${GameMode.Instance.Economy.Taxe_Essence} de taxe.");
                        await OnNpcSecondaryInteract(client, Ped);
                    }
                    break;

                default:
                    break;
            }
        }

        private async Task MarketMenuManager(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
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
                                await BankAccount.AddMoney(itemStack.Price * quantity, $"Achat de {itemStack.Item.name}", false);
                                GameMode.Instance.Economy.CaissePublique += tax;
                                await Update();
                                client.SendNotification($"Vous avez acheté un / des {itemStack.Item.name}(s) pour la somme de {(itemStack.Price * quantity) + tax} dont {tax} de taxes.");
                                await OpenMenu(client);
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
                Alt.Server.LogError ("MarketMenuManager: " +ex);
            }
        }
        #endregion
    }
}
