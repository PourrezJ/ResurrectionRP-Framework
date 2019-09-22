﻿using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Society.Societies.Bennys;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using VehicleInfoLoader.Data;

namespace ResurrectionRP_Server.Society.Societies.WildCustom
{
    public partial class WildCustom
    {
        #region Fields
        private static double PriceChangePlate = 50000;
        private static List<int> blackListModel = new List<int> { 5, 6, 7, 10, 13, 14, 15, 16, 17, 18, 19, 20, 21 };
        private IPlayer ClientInMenu;
        private byte _modType;
        private string _subtitle;
        private double _price;
        private int _red;
        private int _green;
        private int _blue;
        #endregion

        #region MainMenu
        private async Task OpenMainMenu(IPlayer client, IVehicle vehicle)
        {
            if (!(IsEmployee(client) || Owner == client.GetSocialClub()))
            {
                client.SendNotificationError("Hey mec tu veux quoi?!");
                return;
            }

            if (vehicle == null || !vehicle.Exists)
            {
                client.SendNotificationError("Aucun véhicule devant l'établi.");
                return;
            }

            try
            {
                var info = VehicleInfoLoader.VehicleInfoLoader.Get(await vehicle.GetModelAsync());

                if (info == null)
                {
                    client.SendNotificationError("Je ne touche pas à ce véhicule bordel.");
                    return;
                }
                else if (info.VehicleClass != 8 && _garageType == GarageType.Bike)
                {
                    client.SendNotificationError("Seules les motos sont autorisées mon pote!");
                    return;
                }
                else if (info.VehicleClass == 8 && _garageType == GarageType.Car)
                {
                    client.SendNotificationError("J'ai une geule à faire de la moto?!");
                    return;
                }
                else if (blackListModel.Contains(info.VehicleClass))
                {
                    client.SendNotificationError("OH! Je touche pas à ça, dégage!");
                    return;
                }
                else if (ClientInMenu != null && ClientInMenu != client)
                {
                    client.SendNotificationError("Un mécanicien utilise déjà la caisse à outils.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("OpenMainMenu" + ex);
                client.SendNotificationError("Erreur inconnue, contactez un membre du staff.");
                return;
            }

            Menu mainMenu = new Menu("ID_Main", "", SocietyName, Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true, Banner.Garage);
            mainMenu.ItemSelectCallback = MainMenuCallback;
            mainMenu.Finalizer = Finalizer;

            MenuItem cleaning = new MenuItem("Nettoyage", "", "Cleaning", true);
            mainMenu.Add(cleaning);

            MenuItem design = new MenuItem("Esthétique", "", "Design", true);
            mainMenu.Add(design);

            MenuItem performance = new MenuItem("Performance", "", "Perf", true);
            mainMenu.Add(performance);

            MenuItem historique = new MenuItem("Historique", "", "Histo", true);
            mainMenu.Add(historique);

            if (Owner == client.GetSocialClub() && !_vehicleBench.GetVehicleHandler().PlateHide)
            {
                MenuItem changePlate = new MenuItem("Retirer le véhicule du registre", "~r~Illégal ~w~Retirer le véhicule du registre de SanAndreas pour être non identifiable lors d'un contrôle.", "PlateChange", true);
                mainMenu.Add(changePlate);
            }

            await mainMenu.OpenMenu(client);
            ClientInMenu = client;
        }

        private async Task MainMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
            {
                await menu.CloseMenu(client);
                return;
            }

            if (menuItem.Id == "Design")
                await OpenDesignMenu(client);
            else if (menuItem.Id == "Perf")
                await OpenPerformanceMenu(client);
            else if (menuItem.Id == "Histo")
                await OpenHistoricMenu(client);
            else if (menuItem.Id == "Cleaning")
                CleaningVehicle(client);
            else if (menuItem.Id == "PlateChange")
                await OpenChangePlateMenu(client);
        }

        private void CleaningVehicle(IPlayer client)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
                return;

            client.SendNotificationPicture(Utils.Enums.CharPicture.CHAR_LS_CUSTOMS, SocietyName, "Nettoyage: ~r~Démarrage~w~.", "C'est parti!" );

            Utils.Utils.Delay(20000, true, () =>
            {
                if (_vehicleBench == null || !_vehicleBench.Exists)
                    return;

                client.SendNotificationPicture(Utils.Enums.CharPicture.CHAR_LS_CUSTOMS, SocietyName, "Nettoyage: ~g~Terminé~w~.","Elle est niquel!");
                var vh = _vehicleBench.GetVehicleHandler();

                if (vh == null)
                    return;

                vh.Dirt = 0;
                vh.Update();
            });
        }

        private async Task OpenChangePlateMenu(IPlayer client)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
                return;

            PlayerHandler ph = client.GetPlayerHandler();

            if (ph != null)
            {
                AcceptMenu accept = await AcceptMenu.OpenMenu(client, "", "Retirer plaque du registre :", "~r~ATTENTION CETTE MODIFICATION EST ILLEGALE\nVous devez avoir l'argent sur vous", "", $"${PriceChangePlate}", Banner.Garage, false, false);
                accept.AcceptMenuCallBack = (async (IPlayer c, bool response) =>
                {
                    if (response)
                    {
                        if (ph.HasMoney(PriceChangePlate) == true)
                        {
                            VehicleHandler vh = _vehicleBench.GetVehicleHandler();

                            if (vh != null)
                            {
                                vh.PlateHide = true;
                                client.SendNotificationSuccess("La plaque du véhicule a été retirée du registre");
                                ph.Update();
                                vh.Update();
                            }
                        }
                        else
                            client.SendNotificationError("Tu n'as pas l'argent sur toi...");
                    }

                    await OpenMainMenu(client, _vehicleBench);
                });
            }
        }
        #endregion

        #region Performance
        private async Task OpenPerformanceMenu(IPlayer client)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
                return;

            var manifest = _vehicleBench.GetVehicleHandler()?.VehicleManifest;

            if (manifest == null)
            {
                client.SendNotificationError("problème avec le véhicule.");
                return;
            }

            Menu menu = new Menu("ID_Perf", "", "Choisissez l'élément à installer :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, banner: Banner.Garage);
            menu.ItemSelectCallback = PerformanceMenuCallback;

            foreach (var mod in BennysData.PerformanceModList)
            {
                if (manifest.HasModType(mod.ModID))
                {
                    MenuItem item = new MenuItem(mod.ModName, executeCallback: true);
                    item.SetData("mod", mod.ModID);
                    item.OnMenuItemCallback = PerformanceItemMenuCallback;
                    menu.Add(item);
                }
            }

            await menu.OpenMenu(client);
            ClientInMenu = client;
        }

        private async Task PerformanceMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
                await menu.CloseMenu(client);
            else if (menuItem == null)
                await OpenMainMenu(client, _vehicleBench);
        }

        private async Task PerformanceItemMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
            {
                await menu.CloseMenu(client);
                return;
            }
            else if (!menuItem.HasData("mod"))
            {
                client.SendNotificationError("problème avec le véhicule.");
                await OpenPerformanceMenu(client);
                return;
            }

            _modType = menuItem.GetData("mod");
            _subtitle = $"{menuItem.Text} :";
            await OpenPerformanceChoiceMenu(client);
        }

        private async Task OpenPerformanceChoiceMenu(IPlayer client)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
                return;

            var vh = _vehicleBench.GetVehicleHandler();
            var manifest = vh.VehicleManifest;

            if (vh == null || !manifest.HasModType(_modType) && _modType != 666) // 666 = neons
            {
                client.SendNotificationError("Je ne peux pas sur ce véhicule.");
                await OpenPerformanceMenu(client);
            }

            Menu menu = new Menu("ID_Perfs", "", _subtitle, Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, banner: Banner.Garage);
            menu.ItemSelectCallback = PerformanceChoiceMenuCallback;
            menu.IndexChangeCallback = ModPreview;
            menu.Finalizer = Finalizer;

            var mods = manifest.Mods(_modType);
            var perfdata = BennysData.GetPerformanceData(_modType).Value;
            vh.Mods.TryGetValue(_modType, out byte valueInstalled);

            for (int i = 0; i < mods.Count(); i++)
            {
                var mod = mods.ElementAt(i);

                if (mod == null || mod.Name == string.Empty)
                    continue;

                string modName = mod.LocalizedName;

                if (modName == string.Empty)
                    modName = mod.Name;

                if (_modType == 11 && mod.Name == "CMOD_ARM_0")
                    modName = "Gestion moteur de série";
                else if (_modType == 11 && mod.Name == "CMOD_ENG_6")
                    modName = "Reprog moteur Niv. 5";

                MenuItem item = null;

                if (i == valueInstalled)
                {
                    item = new MenuItem(modName, executeCallbackIndexChange: true);

                    if (_garageType == GarageType.Bike)
                        item.RightBadge = BadgeStyle.Bike;
                    else
                        item.RightBadge = BadgeStyle.Car;
                }
                else
                {
                    item = new MenuItem(modName, executeCallback: true, executeCallbackIndexChange: true);
                    var price = BennysData.CalculPrice(vh, perfdata.ModPrice[i]);

                    if (price != 0)
                        item.RightLabel = $"${price}";

                    item.SetData("price", price);
                    item.OnMenuItemCallback = ModChoice;
                }

                menu.Add(item);
            }

            await menu.OpenMenu(client);
            ClientInMenu = client;
        }

        private async Task PerformanceChoiceMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
            {
                await menu.CloseMenu(client);
                return;
            }

            VehicleHandler vh = _vehicleBench.GetVehicleHandler();

            if (menuItem == null)
            {
                if (vh.Mods.ContainsKey(_modType))
                    _vehicleBench.SetMod(_modType, vh.Mods[_modType]);
                else
                    _vehicleBench.SetMod(_modType, 0);

                await OpenPerformanceMenu(client);
            }
        }
        #endregion

        #region Design
        private async Task OpenDesignMenu(IPlayer client)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
                return;

            Menu menu = new Menu("ID_Design", "", "Choisissez l'élément à installer :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, banner: Banner.Garage);
            menu.ItemSelectCallback = DesignMenuCallback;
            menu.Finalizer = Finalizer;

            VehicleHandler vh = _vehicleBench.GetVehicleHandler();
            var manifest = vh.VehicleManifest;

            foreach (var mod in BennysData.EsthetiqueModList)
            {
                if (manifest.HasModType(mod.ModID))
                {
                    var price = BennysData.CalculPrice(vh, BennysData.GetEsthetiqueData(mod.ModID)?.ModPrice ?? 0);

                    MenuItem item = new MenuItem(mod.ModName, rightLabel: $"${price}", executeCallback: true);
                    item.SetData("mod", mod.ModID);
                    item.SetData("price", price);
                    item.OnMenuItemCallback = DesignMenuItemCallback;
                    menu.Add(item);
                }
            }

            if (manifest.Neon)
            {
                double price = BennysData.CalculPrice(vh, 2500);
                MenuItem item = new MenuItem("Néons", "", "Neons", rightLabel: $"${price}", executeCallback: true, executeCallbackIndexChange: true);
                item.SetData("price", price);
                menu.Add(item);
            }

            await menu.OpenMenu(client);
            ClientInMenu = client;
        }

        private async Task DesignMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
            {
                await menu.CloseMenu(client);
                return;
            }
            else if (menuItem == null)
            {
                await OpenMainMenu(client, _vehicleBench);
                return;
            }

            if (menuItem.Id == "Neons")
            {
                _price = menuItem.GetData("price");
                await OpenNeonsMenu(client);
            }
        }

        private async Task DesignMenuItemCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
            {
                await menu.CloseMenu(client);
                return;
            }
            else if (!menuItem.HasData("mod"))
            {
                client.SendNotificationError("problème avec le véhicule.");
                await OpenDesignMenu(client);
                return;
            }

            _modType = menuItem.GetData("mod");
            _price = menuItem.GetData("price");
            _subtitle = $"{menuItem.Text} :";
            await OpenDesignChoiceMenu(client);
        }

        private async Task OpenDesignChoiceMenu(IPlayer client)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
                return;

            VehicleHandler vh = _vehicleBench.GetVehicleHandler();
            var manifest = vh.VehicleManifest;

            if (!manifest.HasModType(_modType)) // 666 = neons
            {
                client.SendNotificationError("Je ne peux pas sur ce véhicule.");
                await OpenDesignMenu(client);
            }

            Menu menu = new Menu("ID_DesignChoice", "", _subtitle, Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, banner: Banner.Garage);
            menu.IndexChangeCallback = ModPreview;
            menu.ItemSelectCallback= DesignChoiceCallback;
            menu.Finalizer = Finalizer;

            var mods = manifest.Mods(_modType);
            vh.Mods.TryGetValue(_modType, out byte valueInstalled);

            for (int i = 0; i < mods.Count(); i++)
            {
                var mod = mods.ElementAt(i);

                if (mod == null || mod.Name == string.Empty)
                    continue;

                MenuItem item = null;
                string modName = mod.LocalizedName;

                if (modName == string.Empty)
                    modName = mod.Name;

                if (i == valueInstalled)
                {
                    item = new MenuItem(modName, executeCallbackIndexChange: true);

                    if (_garageType == GarageType.Bike)
                        item.RightBadge = BadgeStyle.Bike;
                    else
                        item.RightBadge = BadgeStyle.Car;
                }
                else
                {
                    item = new MenuItem(modName, executeCallback: true, executeCallbackIndexChange: true, rightLabel: $"${_price}");
                    item.SetData("price", _price);
                }

                item.OnMenuItemCallback = ModChoice;
                menu.Add(item);
            }

            await menu.OpenMenu(client);
            ClientInMenu = client;
            await ModPreview(client, menu, 0, menu.Items[0]);
        }

        private async Task DesignChoiceCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
            {
                await menu.CloseMenu(client);
                return;
            }

            VehicleHandler vh = _vehicleBench.GetVehicleHandler();

            if (menuItem == null)
            {
                if (vh.Mods.ContainsKey(_modType))
                    _vehicleBench.SetMod(_modType, vh.Mods[_modType]);
                else
                    _vehicleBench.SetMod(_modType, 0);

                if (_modType == 14)
                    StopKlaxon(_vehicleBench);

                await OpenDesignMenu(client);
            }
        }

        private async Task OpenNeonsMenu(IPlayer client)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
                return;

            VehicleHandler vh = _vehicleBench.GetVehicleHandler();

            Menu menu = new Menu("ID_Neons", "", "Néons :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, banner: Banner.Garage);
            menu.ItemSelectCallback = NeonsMenuCallback;
            menu.ListItemChangeCallback = NeonListItemChangeCallback;
            menu.Finalizer = Finalizer;

            _red = vh.NeonColor.R / 17;
            _green = vh.NeonColor.G / 17;
            _blue = vh.NeonColor.B / 17;
            List<object> colorItems = GetColorListItems();
            menu.Add(new ListItem("Rouge", "", "Red", colorItems, _red, false, true));
            menu.Add(new ListItem("Vert", "", "Green", colorItems, _green, false, true));
            menu.Add(new ListItem("Bleu", "", "Blue", colorItems, _blue, false, true));

            MenuItem menuItem = new MenuItem("Valider", "", "Validate", true, false, $"${_price}");
            menuItem.SetData("price", _price);
            menu.Add(menuItem);

            vh.NeonState = new Tuple<bool, bool, bool, bool>(true, true,true, true);
            await menu.OpenMenu(client);
            ClientInMenu = client;
        }

        private async Task NeonsMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
            {
                await menu.CloseMenu(client);
                return;
            }

            VehicleHandler vh = _vehicleBench.GetVehicleHandler();

            if (menuItem == null)
            {
                int red = vh.NeonColor.R;
                int green = vh.NeonColor.G;
                int blue = vh.NeonColor.B;
                 _vehicleBench.NeonColor = (Color.FromArgb(red, green, blue));

                await OpenDesignMenu(client);
                return;
            }

            if (menuItem.Id == "Validate")
            {
                double price = menuItem.GetData("price");
                await InstallNeon(client, price);
            }
        }

        private async Task NeonListItemChangeCallback(IPlayer client, Menu menu, IListItem listItem, int listIndex)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
            {
                await menu.CloseMenu(client);
                return;
            }

            if (listItem.Id == "Red")
                _red = int.Parse(listItem.Items[listIndex].ToString());
            else if (listItem.Id == "Green")
                _green = int.Parse(listItem.Items[listIndex].ToString());
            else if (listItem.Id == "Blue")
                _blue = int.Parse(listItem.Items[listIndex].ToString());

            Color color = Color.FromArgb(_red * 17, _green * 17, _blue * 17);
            _vehicleBench.NeonColor = (color);
        }

        private Task ModPreview(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
                return Task.CompletedTask;

            byte selected = (byte)itemIndex;

            if (_modType == 14)
            {
                _vehicleBench.SetMod(_modType, selected);
                PreviewKlaxon(_vehicleBench);
            }
            else
                _vehicleBench.SetMod(_modType, selected);

            return Task.CompletedTask;
        }
        #endregion

        #region Historique Vehicle
        private async Task OpenHistoricMenu(IPlayer client)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
                return;

            VehicleHandler vh = _vehicleBench.GetVehicleHandler();

            if (vh == null)
            {
                client.SendNotificationError("Problème avec le véhicule.");
                return;
            }

            if (vh.Mods.Count == 0)
            {
                client.SendNotificationError("Aucune amélioration n'est disponible sur le véhicule!");
                return;
            }
            else if (vh.Mods.Count >= 1)
            {
                Menu menu = new Menu("ID_Histo", "", "Historique :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, false, Banner.Garage);
                menu.ItemSelectCallback = HistoricMenuCallback;
                menu.Finalizer = Finalizer;

                foreach (var mod in vh.Mods)
                {
                    VehicleMod modData;

                    if (mod.Key == 22)
                        modData = vh.VehicleManifest.GetMod(mod.Key, mod.Value);
                    else
                        modData = vh.VehicleManifest.GetMod(mod.Key, mod.Value - 1);

                    if (modData != null)
                    {
                        string modName = modData.LocalizedName;

                        if (modData.Name == "CMOD_ARM_0")
                            modName = "Gestion moteur de série";
                        else if (modData.Name == "CMOD_ENG_6")
                            modName = "Reprog moteur Niv. 5";
                        else if (modData.LocalizedName == string.Empty)
                            modName = modData.Name;

                        menu.Add(new MenuItem(modName));
                    }
                }

                if (vh.NeonColor != null && vh.NeonColor.ToArgb() != 0)
                {
                    Color color = vh.NeonColor;
                    menu.Add(new MenuItem($"Néons : Rouge {color.R / 17} - Vert {color.G / 17} - Bleu {color.B / 17}"));
                }

                await menu.OpenMenu(client);
                ClientInMenu = client;
            }
        }

        private async Task HistoricMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
                await OpenMainMenu(client, _vehicleBench);
        }
        #endregion

        #region END
        private async Task InstallNeon(IPlayer client, double price)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
                return;

            VehicleHandler vh = _vehicleBench.GetVehicleHandler();

            if (await BankAccount.GetBankMoney(price, $"{SocietyName}: Néons"))
            {
                vh.NeonColor = Color.FromArgb(_red * 17, _green * 17, _blue * 17);
                vh.Update();

                client.SendNotificationSuccess($"Vous avez installé des Néons pour la somme de ${price}");
                await OpenNeonsMenu(client);
            }
        }

        private async Task ModChoice(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (_vehicleBench == null || !_vehicleBench.Exists)
                return;

            byte selected = (byte)itemIndex;

            VehicleHandler vh = _vehicleBench?.GetVehicleHandler();
            double price = menu.HasData("price") ? menu.GetData("price") : menuItem.GetData("price");

            string modName = string.Empty;

            // Bug with xenons
            if (_modType == 22)
                modName = vh.VehicleManifest.GetMod(_modType, selected).LocalizedName;
            else
                modName = vh.VehicleManifest.GetMod(_modType, selected - 1).LocalizedName;

            if (_modType == 11 && vh.VehicleManifest.GetMod(_modType, selected - 1).Name == "CMOD_ARM_0")
                modName = "Gestion moteur de série";

            bool hasMoney = true;

            if (price > 0)
                hasMoney = await BankAccount.GetBankMoney(price, $"{SocietyName}: {modName}");

            if (hasMoney)
            {
                if (vh == null)
                    return;

                vh.Mods.AddOrUpdate(_modType, selected, (key, oldvalue) => selected);
                _vehicleBench.SetMod(_modType, selected);
                vh.Update();
                string str = $"Vous avez installé {modName}";

                if (price != 0)
                    str += $" pour la somme de ${price}.";

                client.SendNotificationSuccess(str);
            }
            else
                client.SendNotificationError("Vous n'avez pas assez sur le compte de l'entreprise.");

            if (menu.Id == "ID_DesignChoice")
                await OpenDesignChoiceMenu(client);
            else if (menu.Id == "ID_Perfs")
                await OpenPerformanceChoiceMenu(client);
        }

        private async Task Finalizer(IPlayer client, Menu menu)
        {
            ClientInMenu = null;
            await Task.CompletedTask;
        }
        #endregion

        #region Private methods
        private List<object> GetColorListItems()
        {
            List<object> items = new List<object>();

            for (int i = 0; i <= 15; i++)
                items.Add(i.ToString());

            return items;
        }
        #endregion
    }
}

