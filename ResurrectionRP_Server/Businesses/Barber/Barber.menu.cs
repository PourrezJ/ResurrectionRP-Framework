using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Bank;
using ResurrectionRP_Server.Models;

namespace ResurrectionRP_Server.Businesses.Barber
{

    public partial class BarberStore
    {
        #region Menu
        #region Private fields
        private int _firstcolorprev = 0;
        private int _secondcolorprev = 0;
        private int _beardfirstcolor = 0;
        private int _beardsecondcolor = 0;
        #endregion

        #region Main
        public override async Task OpenMenu(IPlayer client, Entities.Peds.Ped npc = null)
        {
            if (!( IsOwner(client) ||  IsEmployee(client)))
            {
                await client.NotifyAsync("Men, tu n'est pas coiffeur!");
                return;
            }

            Menu mainmenu = new Menu("ID_BarberMain", "", "", 0, 0, Menu.MenuAnchor.MiddleRight, false, true, true, Banner.Barber);

            List<object> _playerlist = new List<object>();
            foreach (PlayerHandler player in  client.GetNearestPlayers(5f)) { _playerlist.Add(player.Identite.Name); };

            if (_playerlist.Count > 0)
            {
                //Reset old client choise
                ClientSelected = null;

                //List client choise
                mainmenu.Add(new ListItem("Client:", "Choix du client", "ID_PlayerSelect", _playerlist, 0, true));

                // If is Owner ...
                if ( IsOwner(client))
                {
                    MenuItem getmoney = new MenuItem("Gérer les finances", "", "ID_TakeMoney", true, rightLabel: $"${BankAccount.Balance}");
                    getmoney.OnMenuItemCallback = GetMoneyMenu;
                    mainmenu.Add(getmoney);
                }

                MenuItem depot = new MenuItem("Déposer de l'argent", "", "ID_Depot", true);
                depot.SetInput("", 10, InputType.UFloat, true);
                depot.OnMenuItemCallback = DepotMoneyMenu;
                mainmenu.Add(depot);

                MenuItem haircut = new MenuItem("Faire une coupe de cheveux", "", "ID_Hair", true);
                haircut.OnMenuItemCallback = HairCutChoise;
                mainmenu.Add(haircut);

                MenuItem beardcut = new MenuItem("Tailler une barbe", "", "ID_Beard", true);
                beardcut.OnMenuItemCallback = BeardCutChoise;
                mainmenu.Add(beardcut);


                MenuItem colorchange = new MenuItem($"Faire une couleur (${ColorPrice})", "", "ID_Color", true);
                colorchange.OnMenuItemCallback = ColorChoise;
                mainmenu.Add(colorchange);

                await MenuManager.OpenMenu(client, mainmenu);
            }
            else
                await client.SendNotificationError("Aucun client autour.");
        }
        #endregion

        #region Depot
        private async Task GetMoneyMenu(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                await OpenMenu(client);
                return;
            }

            await BankMenu.OpenBankMenu(client, BankAccount, AtmType.Business, menu, GetMoneyMenu);
        }

        // Depot d'argent dans la caisse.
        private async Task DepotMoneyMenu(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (double.TryParse(menuItem.InputValue, out double result))
            {
                if (result < 0)
                    return;

                var ph = client.GetPlayerHandler();

                if (await ph.HasMoney(result))
                {
                    BankAccount.AddMoney(result, $"Ajout d'argent par {ph.Identite.Name}");
                    await client.SendNotificationSuccess($"Vous avez déposé ${result} dans la caisse.");
                }
                else
                    await client.SendNotificationError("Vous n'avez pas assez d'argent sur vous.");
            }
        }
        #endregion

        #region Beard
        private async Task BeardCutChoise(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            var listItem = menu.Items.Find(m => m.Id == "ID_PlayerSelect") as ListItem;
            var selected = listItem.SelectedItem;
            var name = listItem.Items[selected];
            ClientSelected = PlayerManager.GetPlayerByName(name.ToString());

            if (ClientSelected == null)
                return;

            if (ClientSelected.Character.Gender == 1) // if is a girl
            {
                await client.SendNotificationError("Les femmes à barbe sont interdites dans le pays.");
                return;
            }

            menu = new Menu("ID_BarberBeard", "", "", 0, 0, Menu.MenuAnchor.MiddleRight, false, true, false, Banner.Barber);
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
                await ClientSelected.Update();
                await Update();
            }
            else
                await client.SendNotificationError("Vous n'avez pas de fond de caisse.");

            await menu.CloseMenu(client);
        }
        #endregion

        #region HairCut
        private async Task HairCutChoise(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            var listItem = menu.Items.Find(m => m.Id == "ID_PlayerSelect") as ListItem;
            var selected = listItem.SelectedItem;
            var name = listItem.Items[selected];
            ClientSelected = PlayerManager.GetPlayerByName(name.ToString());

            if (ClientSelected == null)
                return;

            menu = new Menu("ID_BarberHair", "", "", 0, 0, Menu.MenuAnchor.MiddleRight, false, true, false, Banner.Barber);
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
                await ClientSelected.Update();
                await Update();
            }
            else
                await client.SendNotificationError("Vous n'avez pas de fond de caisse.");

            await menu.CloseMenu(client);
        }
        #endregion

        #region Color
        private async Task ColorChoise(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            var listItem = menu.Items.Find(m => m.Id == "ID_PlayerSelect") as ListItem;

            var selected = listItem.SelectedItem;
            var name = listItem.Items[selected];

            ClientSelected = PlayerManager.GetPlayerByName(name.ToString());

            if (ClientSelected == null) return;

            _firstcolorprev = ClientSelected.Character.Hair.Color;
            _secondcolorprev = ClientSelected.Character.Hair.HighlightColor;
            _beardfirstcolor = ClientSelected.Character.Appearance[1].Color;
            _beardsecondcolor = ClientSelected.Character.Appearance[1].SecondaryColor;

            menu = new Menu("ID_BarberColor", "", "", 0, 0, Menu.MenuAnchor.MiddleRight, false, true, false, Banner.Barber);
            menu.ItemSelectCallback = BarberMenuCallBack;
            menu.ListCallback = ColorPreview;
            menu.Finalizer = MenuFinalizer;

            List<object> _colorlist = new List<object>();

            for (int i = 0; i <= 63; i++) { _colorlist.Add(i); };

            menu.Add(new ListItem("Couleur cheveux principal", "Changer la couleur de cheveux principal.", "ID_FirstColor", _colorlist, ClientSelected.Character.Hair.Color, true, true));
            menu.Add(new ListItem("Couleur cheveux secondaire", "Changer la couleur de cheveux secondaire.", "ID_SecondColor", _colorlist, ClientSelected.Character.Hair.HighlightColor, true, true));
            menu.Add(new ListItem("Couleur barbe primaire", "Changer la couleur de la barbe secondaire.", "ID_BeardFirstColor", _colorlist, ClientSelected.Character.Hair.HighlightColor, true, true));
            menu.Add(new ListItem("Couleur barbe secondaire", "Changer la couleur de la barbe secondaire.", "ID_BeardSecondColor", _colorlist, ClientSelected.Character.Hair.HighlightColor, true, true));

            MenuItem valid = new MenuItem("~g~Valider les choix", executeCallback: true);
            valid.OnMenuItemCallback = ColorValidChoise;
            menu.Add(valid);

            await menu.OpenMenu(client);
        }

        private async Task ColorValidChoise(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            try
            {
                if (await BankAccount.GetBankMoney(ColorPrice, $"Couleur par {client.GetPlayerHandler().Identite.Name}"))
                {
                    ClientSelected.Character.Hair.Color = _firstcolorprev;
                    ClientSelected.Character.Hair.HighlightColor = _secondcolorprev;

                    var head = new HeadOverlay()
                    {
                        Index = ClientSelected.Character.Appearance[1].Index,
                        Color = _beardfirstcolor,
                        SecondaryColor = _beardsecondcolor,
                        Opacity = 255
                    };

                    ClientSelected.Character.Appearance[1] = head;

                    ClientSelected.Character.ApplyCharacter(ClientSelected.Client);
                    await ClientSelected.Update();
                    await Update();
                    await menu.CloseMenu(client);
                }
                else
                {
                    await client.SendNotificationError("Vous n'avez pas de fond de caisse.");
                    ClientSelected?.Character.ApplyCharacter(client);
                }
            }
            catch
            {
                await client.SendNotificationError("Pour appliquer la couleur.");
                ClientSelected?.Character.ApplyCharacter(client);

            }
        }
        #endregion

        #region Preview
        private async Task HairCutPreview(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            if (menuItem.Id == "ID_Hair")
            {
                List<Hairs> _hairsList = (ClientSelected.Character.Gender == 0) ? Hairs.HairsMenList : Hairs.HairsGirlList;
                await ClientSelected?.Client?.SetClothAsync(ClothSlot.Hair, _hairsList[itemIndex].ID, 0, 0);
            }
            else if (menuItem.Id == "ID_Beard")
            {
                var hair = ClientSelected.Character.Hair;
                await ClientSelected?.Client?.SetHeadOverlayAsync(1, new HeadOverlayData((byte)itemIndex, 255, (uint)hair.Color, (uint)hair.HighlightColor));
            }
        }

        private async Task ColorPreview(IPlayer client, Menu menu, IMenuItem menuItem, int listindex)
        {
            if (menuItem.Id == "ID_FirstColor")
            {
                _firstcolorprev = (byte)listindex;
            }
            else if (menuItem.Id == "ID_SecondColor")
            {
                _secondcolorprev = (byte)listindex;
            }
            else if (menuItem.Id == "ID_BeardFirstColor")
            {
                _beardfirstcolor = (byte)listindex;
            }
            else if (menuItem.Id == "ID_BeardSecondColor")
            {
                _beardsecondcolor = (byte)listindex;
            }

            await ClientSelected?.Client?.SetHairColorAsync((uint)_firstcolorprev, (uint)_secondcolorprev);

            HeadOverlayData head = new HeadOverlayData()
            {
                Index = (uint)ClientSelected.Character.Appearance[1].Index,
                ColorId = (uint)_beardfirstcolor,
                SecondaryColorId = (uint)_beardsecondcolor,
                Opacity = 255
            };

            await ClientSelected?.Client?.SetHeadOverlayAsync(1, head);
        }

        private Task MenuFinalizer(IPlayer client, Menu menu)
        {
            if (ClientSelected == null)
                return Task.CompletedTask;

            ClientSelected.Character.ApplyCharacter(ClientSelected.Client);
            return Task.CompletedTask;
        }
        #endregion
        #endregion

        #region Callbacks
        private async Task BarberMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                if (menu.Id == "ID_BarberHair")
                    await ClientSelected?.Client?.SetClothAsync(ClothSlot.Hair, ClientSelected.Character.Hair.Hair, 0, 0);
                else if (menu.Id == "ID_BarberBeard")
                {
                    HeadOverlay hairs = ClientSelected.Character.Appearance[1];
                    ClientSelected?.Client?.SetHeadOverlay(1, new HeadOverlayData((uint)hairs.Index, hairs.Opacity, (uint)hairs.Color, (uint)hairs.SecondaryColor));
                }
                else if (menu.Id == "ID_BarberColor")
                {
                    await ClientSelected?.Client?.SetHairColorAsync((uint)ClientSelected.Character.Hair.Color, (uint)ClientSelected.Character.Hair.HighlightColor);

                    HeadOverlayData head = new HeadOverlayData()
                    {
                        Index = (uint)ClientSelected.Character.Appearance[1].Index,
                        ColorId = (uint)ClientSelected.Character.Hair.Color,
                        SecondaryColorId = (uint)ClientSelected.Character.Hair.HighlightColor,
                        Opacity = 255
                    };

                    await ClientSelected?.Client?.SetHeadOverlayAsync(1, head);
                }

                await OpenMenu(client);
            }
        }
        #endregion
    }
}
