﻿using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Bank;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net;

namespace ResurrectionRP_Server.Business.Barber
{

    public partial class BarberStore
    {
        #region Private fields
        private int _hairFirstColor = 0;
        private int _hairSecondColor = 0;
        private int _beardFirstColor = 0;
        private int _beardSecondColor = 0;
        #endregion

        #region Main
        public override async Task OpenMenu(IPlayer client, Entities.Peds.Ped npc = null)
        {
            if (!( IsOwner(client) ||  IsEmployee(client)))
            {
                client.SendNotification("Men, tu n'es pas coiffeur!");
                return;
            }

            Menu mainMenu = new Menu("ID_BarberMain", "", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true, Banner.Barber);

            List<object> _playerlist = new List<object>();

            foreach (IPlayer player in client.GetNearestPlayers(5f, false))
                _playerlist.Add(player.GetPlayerHandler().Identite.Name);

            if (_playerlist.Count > 0)
            {
                // Reset old client choice
                ClientSelected = null;

                // List client choice
                mainMenu.Add(new ListItem("Client:", "Choix du client", "ID_PlayerSelect", _playerlist, 0, true));

                // If is Owner ...
                if ( IsOwner(client))
                {
                    MenuItem getmoney = new MenuItem("Gérer les finances", "", "ID_TakeMoney", true, rightLabel: $"${BankAccount.Balance}");
                    getmoney.OnMenuItemCallback = BankAccountMenu;
                    mainMenu.Add(getmoney);
                }

                MenuItem depot = new MenuItem("Déposer de l'argent", "", "ID_Depot", true);
                depot.SetInput("", 10, InputType.UFloat, true);
                depot.OnMenuItemCallback = DepotMoneyMenu;
                mainMenu.Add(depot);

                MenuItem haircut = new MenuItem("Faire une coupe de cheveux", "", "ID_Hair", true);
                haircut.OnMenuItemCallback = HairCutChoice;
                mainMenu.Add(haircut);

                MenuItem beardcut = new MenuItem("Tailler une barbe", "", "ID_Beard", true);
                beardcut.OnMenuItemCallback = BeardCutChoice;
                mainMenu.Add(beardcut);


                MenuItem colorchange = new MenuItem($"Faire une couleur (${ColorPrice})", "", "ID_Color", true);
                colorchange.OnMenuItemCallback = ColorChoice;
                mainMenu.Add(colorchange);

                await MenuManager.OpenMenu(client, mainMenu);
            }
            else
                client.SendNotificationError("Aucun client à proximité.");
        }

        private async Task BarberMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (ClientSelected == null || ClientSelected.Client == null || !ClientSelected.Client.Exists)
            {
                await OpenMenu(client);
                return;
            }

            if (menuItem == null)
            {
                if (menu.Id == "ID_BarberHair")
                    ClientSelected.Client.SetCloth(ClothSlot.Hair, ClientSelected.Character.Hair.Hair, 0, 0);
                else if (menu.Id == "ID_BarberBeard")
                {
                    HeadOverlay hairs = ClientSelected.Character.Appearance[1];
                    ClientSelected.Client.SetHeadOverlay(1, new HeadOverlayData((uint)hairs.Index, hairs.Opacity, (uint)hairs.Color, (uint)hairs.SecondaryColor));
                }
                else if (menu.Id == "ID_BarberColor")
                {
                    ClientSelected.Client.SetHairColor((uint)ClientSelected.Character.Hair.Color, (uint)ClientSelected.Character.Hair.HighlightColor);

                    HeadOverlayData head = new HeadOverlayData()
                    {
                        Index = (uint)ClientSelected.Character.Appearance[1].Index,
                        ColorId = (uint)ClientSelected.Character.Hair.Color,
                        SecondaryColorId = (uint)ClientSelected.Character.Hair.HighlightColor,
                        Opacity = 255
                    };

                    ClientSelected.Client.SetHeadOverlay(1, head);
                }

                await OpenMenu(client);
            }
        }

        private async Task BankAccountMenu(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                await OpenMenu(client);
                return;
            }

            await BankMenu.OpenBankMenu(client, BankAccount, AtmType.Business, menu, BankAccountMenu);
        }

        private async Task DepotMoneyMenu(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (double.TryParse(menuItem.InputValue, out double result))
            {
                if (result < 0)
                    return;

                var ph = client.GetPlayerHandler();

                if (ph.HasMoney(result))
                {
                    await BankAccount.AddMoney(result, $"Ajout d'argent par {ph.Identite.Name}");
                    client.SendNotificationSuccess($"Vous avez déposé ${result} dans la caisse.");
                }
                else
                    client.SendNotificationError("Vous n'avez pas assez d'argent sur vous.");
            }
        }
        #endregion

        #region Beard
        private async Task BeardCutChoice(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            var listItem = menu.Items.Find(m => m.Id == "ID_PlayerSelect") as ListItem;
            var selected = listItem.SelectedItem;
            var name = listItem.Items[selected];
            ClientSelected = PlayerManager.GetPlayerByName(name.ToString());

            if (ClientSelected == null)
                return;

            if (ClientSelected.Character.Gender == 1) // if is a girl
            {
                client.SendNotificationError("Les femmes à barbe sont interdites dans le pays.");
                return;
            }

            menu = new Menu("ID_BarberBeard", "", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, false, Banner.Barber);
            menu.ItemSelectCallback= BarberMenuCallBack;
            menu.IndexChangeCallback = HairCutPreview;
            menu.Finalizer = MenuFinalizer;

            foreach (var beard in Beards.BeardsList)
            {
                MenuItem item = new MenuItem(beard.Name, "", "ID_Beard", true, true);

                if (ClientSelected.Character.Appearance[1].Index == beard.ID)
                    item.RightBadge = BadgeStyle.Barber;
                else
                {
                    item.RightLabel = $"${beard.Price}";
                    item.OnMenuItemCallback = BeardCutSelected;
                }

                menu.Add(item);
            }

            await MenuManager.OpenMenu(client, menu);
            await HairCutPreview(client, menu, 0, menu.Items[0]);
        }

        private async Task BeardCutSelected(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            Beards beard = Beards.BeardsList[itemIndex];

            if (await BankAccount.GetBankMoney(beard.Price, $"Barbe par {client.GetPlayerHandler().Identite.Name}"))
            {
                ClientSelected.Character.Appearance[1].Index = (byte)itemIndex;
                ClientSelected.Character.Appearance[1].Color = (byte)ClientSelected.Character.Hair.Color;
                ClientSelected.Character.Appearance[1].SecondaryColor = (byte)ClientSelected.Character.Hair.HighlightColor;
                ClientSelected.Character.Appearance[1].Opacity = 255;

                ClientSelected.Character.ApplyCharacter(ClientSelected.Client);
                ClientSelected.UpdateFull();
                await Update();
            }
            else
                client.SendNotificationError("Vous n'avez pas de fond de caisse.");

            await OpenMenu(client);
        }
        #endregion

        #region HairCut
        private async Task HairCutChoice(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            var listItem = menu.Items.Find(m => m.Id == "ID_PlayerSelect") as ListItem;
            var selected = listItem.SelectedItem;
            var name = listItem.Items[selected];
            ClientSelected = PlayerManager.GetPlayerByName(name.ToString());

            if (ClientSelected == null)
                return;

            menu = new Menu("ID_BarberHair", "", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, false, Banner.Barber);
            menu.ItemSelectCallback = BarberMenuCallBack;
            menu.IndexChangeCallback = HairCutPreview;
            menu.Finalizer = MenuFinalizer;

            List<Hairs> _hairsList = (ClientSelected.Character.Gender == 0) ? Hairs.HairsMenList : Hairs.HairsGirlList;

            foreach (var hairs in _hairsList)
            {
                MenuItem item = new MenuItem(hairs.Name, "", "ID_Hair", true, true);

                if (ClientSelected.Character.Hair.Hair == hairs.ID)
                    item.RightBadge = BadgeStyle.Barber;
                else
                {
                    item.RightLabel = $"${hairs.Price}";
                    item.OnMenuItemCallback = HairCutSelected;
                }

                menu.Add(item);
            }

            await MenuManager.OpenMenu(client, menu);
            await HairCutPreview(client, menu, 0, menu.Items[0]);
        }

        private async Task HairCutSelected(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            List<Hairs> _hairsList = (ClientSelected.Character.Gender == 0) ? Hairs.HairsMenList : Hairs.HairsGirlList;
            Hairs hair = _hairsList[itemIndex];

            if (await BankAccount.GetBankMoney(hair.Price, $"Coupe cheveux par {client.GetPlayerHandler().Identite.Name}"))
            {
                ClientSelected.Character.Hair.Hair = hair.ID;
                ClientSelected.Character.ApplyCharacter(ClientSelected.Client);
                ClientSelected.UpdateFull();
                await Update();
            }
            else
                client.SendNotificationError("Vous n'avez pas de fond de caisse.");

            await OpenMenu(client);
        }

        private async Task HairCutPreview(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            if (menuItem.Id == "ID_Hair")
            {
                List<Hairs> _hairsList = (ClientSelected.Character.Gender == 0) ? Hairs.HairsMenList : Hairs.HairsGirlList;
                ClientSelected?.Client?.SetCloth(ClothSlot.Hair, _hairsList[itemIndex].ID, 0, 0);
            }
            else if (menuItem.Id == "ID_Beard")
            {
                var hair = ClientSelected.Character.Hair;

                if (ClientSelected.Client != null)
                ClientSelected?.Client?.SetHeadOverlay(1, new HeadOverlayData((byte)itemIndex, 255, (uint)hair.Color, (uint)hair.HighlightColor));
            }
        }
        #endregion

        #region Color
        private async Task ColorChoice(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            var listItem = menu.Items.Find(m => m.Id == "ID_PlayerSelect") as ListItem;
            
            var selected = listItem.SelectedItem;
            var name = listItem.Items[selected];
            ClientSelected = PlayerManager.GetPlayerByName(name.ToString());

            if (ClientSelected == null)
                return;

            _hairFirstColor = ClientSelected.Character.Hair.Color;
            _hairSecondColor = ClientSelected.Character.Hair.HighlightColor;
            _beardFirstColor = ClientSelected.Character.Appearance[1].Color;
            _beardSecondColor = ClientSelected.Character.Appearance[1].SecondaryColor;

            menu = new Menu("ID_BarberColor", "", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, false, Banner.Barber);
            menu.ItemSelectCallback = BarberMenuCallBack;
            menu.ListItemChangeCallback = ColorPreview;
            menu.Finalizer = MenuFinalizer;

            List<object> _colorlist = new List<object>();

            for (int i = 0; i <= 63; i++)
                _colorlist.Add(i.ToString());

            menu.Add(new ListItem("Couleur cheveux principal", "Changer la couleur de cheveux principal.", "ID_HairFirstColor", _colorlist, ClientSelected.Character.Hair.Color, true, true));
            menu.Add(new ListItem("Couleur cheveux secondaire", "Changer la couleur de cheveux secondaire.", "ID_HairSecondColor", _colorlist, ClientSelected.Character.Hair.HighlightColor, true, true));
            menu.Add(new ListItem("Couleur barbe primaire", "Changer la couleur de la barbe secondaire.", "ID_BeardFirstColor", _colorlist, ClientSelected.Character.Hair.HighlightColor, true, true));
            menu.Add(new ListItem("Couleur barbe secondaire", "Changer la couleur de la barbe secondaire.", "ID_BeardSecondColor", _colorlist, ClientSelected.Character.Hair.HighlightColor, true, true));

            MenuItem valid = new MenuItem("~g~Valider les choix", executeCallback: true);
            valid.OnMenuItemCallback = ColorValidChoice;
            menu.Add(valid);

            await menu.OpenMenu(client);
        }

        private async Task ColorValidChoice(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (await BankAccount.GetBankMoney(ColorPrice, $"Couleur par {client.GetPlayerHandler().Identite.Name}"))
            {
                ClientSelected.Character.Hair.Color = _hairFirstColor;
                ClientSelected.Character.Hair.HighlightColor = _hairSecondColor;

                var head = new HeadOverlay()
                {
                    Index = ClientSelected.Character.Appearance[1].Index,
                    Color = _beardFirstColor,
                    SecondaryColor = _beardSecondColor,
                    Opacity = 255
                };

                ClientSelected.Character.Appearance[1] = head;
                ClientSelected.Character.ApplyCharacter(ClientSelected.Client);
                ClientSelected.UpdateFull();
                await Update();
            }
            else
            {
                client.SendNotificationError("Vous n'avez pas de fond de caisse.");
                ClientSelected?.Character.ApplyCharacter(client);
            }

            await OpenMenu(client);
        }

        private async Task ColorPreview(IPlayer client, Menu menu, IListItem listItem, int listindex)
        {
            if (ClientSelected == null)
                await OpenMenu(client);

            if (listItem.Id == "ID_HairFirstColor")
                _hairFirstColor = listindex;
            else if (listItem.Id == "ID_HairSecondColor")
                _hairSecondColor = listindex;
            else if (listItem.Id == "ID_BeardFirstColor")
                _beardFirstColor = listindex;
            else if (listItem.Id == "ID_BeardSecondColor")
                _beardSecondColor = listindex;

            if (listItem.Id == "ID_HairFirstColor" || listItem.Id == "ID_HairSecondColor" && ClientSelected.Client.Exists)
                await ClientSelected.Client.SetHairColorAsync((uint)_hairFirstColor, (uint)_hairSecondColor);
            else if (ClientSelected.Client.Exists)
            {
                HeadOverlayData head = new HeadOverlayData()
                {
                    Index = (uint)ClientSelected.Character.Appearance[1].Index,
                    ColorId = (uint)_beardFirstColor,
                    SecondaryColorId = (uint)_beardSecondColor,
                    Opacity = 255
                };

                await ClientSelected.Client.SetHeadOverlayAsync(1, head);
            }
        }
        #endregion

        #region Finalizer
        private Task MenuFinalizer(IPlayer client, Menu menu)
        {
            if (ClientSelected == null)
                return Task.CompletedTask;

            ClientSelected.Character.ApplyCharacter(ClientSelected.Client);
            return Task.CompletedTask;
        }
        #endregion
    }
}
