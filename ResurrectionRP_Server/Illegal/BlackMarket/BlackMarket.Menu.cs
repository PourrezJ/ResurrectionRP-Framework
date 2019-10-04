using System;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Peds;

namespace ResurrectionRP_Server.Illegal
{
    public partial class BlackMarket
    {
        private void OnBlackMInteract(IPlayer client, Ped npc)
        {
            Menu menu = new Menu("Black_Market", "Black Market");
            menu.ItemSelectCallback = BlackMarketCallBack;
            menu.Add(new MenuItem("Armes & Equipements", "Acheter des armes ou de l'équipements illégale", "ID_Weapons", true, false));
            if (!WeedLabsOwned.Contains(client.GetSocialClub()))
                menu.Add(new MenuItem("Laboratoire Cannabis", "Découvrir la position d'un laboratoire de production de Cannabis", "ID_WeedLab", true, false, "$50000"));

            if (!WeedDealerOwned.Contains(client.GetSocialClub()))
                menu.Add(new MenuItem("Dealer de Cannabis", "Découvrir la position du dealer de Cannabis", "ID_WeedDealer", true, false, "$50000"));

            menu.OpenMenu(client);
        }

        private void BlackMarketCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                menu.CloseMenu(client);
                return;
            }

            switch (menuItem.Id)
            {
                case "ID_Weapons":
                    OpenBlackMarket(client, menu);
                    break;

                case "ID_Weedlab":
                    BuyWeedLabPos(client);
                    break;

                case "ID_WeadDealer":
                    BuyWeedDealerPos(client);
                    break;
            }
        }

        private void BuyWeedLabPos(IPlayer client)
        {
            WeedLabsOwned.Add(client.GetSocialClub());
        }

        private void BuyWeedDealerPos(IPlayer client)
        {
            WeedDealerOwned.Add(client.GetSocialClub());
        }

        public void OpenBlackMarket(IPlayer client, Menu menu)
        {
            menu.ClearItems();
            menu.BackCloseMenu = false;
            menu.ItemSelectCallback = BlackMarketItemSelected;

            foreach (Models.Item item in IllegalItems)
            {
                if (item.isStackable)
                    menu.Add(new ListItem(item.name + " $" + item.itemPrice, item.description, "", 50, 0, true, false));
                else
                    menu.Add(new MenuItem(item.name + " $" + item.itemPrice.ToString(), item.description, "", true, false));
            }

            menu.OpenMenu(client);
        }

        private void BlackMarketItemSelected(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (itemIndex == -1)
            {
                OnBlackMInteract(client, BlackMPed);
                return;
            }
            var item = IllegalItems[itemIndex];
            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if (ph.HasMoney(item.itemPrice))
            {
                if (ph.AddItem(item))
                {
                    client.SendNotificationSuccess("Vous venez d'acheter " + item.name);
                    ph.UpdateFull();
                }
                else
                {
                    client.SendNotificationError("Vous n'avez pas la place sur vous pour " + item.name);
                }
            }
            else
            {
                client.SendNotificationError("Vous n'avez pas assez d'argent sur vous pour " + item.name);
            }
        }
    }
}
