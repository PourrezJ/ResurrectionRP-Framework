using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Farms;
using ResurrectionRP_Server.Items;
using ResurrectionRP_Server.Menus;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using System;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities.Players
{
    partial class PlayerHandler
    {
        #region Private static fields
        private static double keyDupliPrice = 350;
        #endregion

        #region Private fields
        private VehicleKey keygiven;
        #endregion

        #region Menus
        public void OpenPlayerMenu()
        {
            if (Client.IsDead)
                return;

            if (PlayerSync.IsCuff)
            {
                Client.SendNotificationError("Vous ne pouvez pas ouvrir le menu\nVous êtes menotté.");
                return;
            }
            
            Menu menu = new Menu("ID_MainMenu", Identite.Name, "Choisissez une option :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true);
            menu.BannerColor = new MenuColor(0, 0, 0, 0);
            menu.ItemSelectCallback = MainMenuManager;
            
            menu.Add(new MenuItem("Mes clefs", "", "ID_Clefs", true));
            menu.Add(new MenuItem("Animations", "Réglage des touches des animations", "ID_Animations", true));
            menu.Add(new MenuItem("Styles de Marche", "", "ID_WalkingStyles", true));
            menu.Add(new MenuItem("Expression Visage", "", "ID_Face", true));
            menu.Add(new MenuItem("Bourse", "", "ID_Bourse", true));
            menu.Add(new MenuItem("Déconnecter", "", "ID_Disconnect", true));
            MenuManager.OpenMenu(Client, menu);
        }

        private void OpenKeysMenu()
        {
            Menu menu = new Menu("ID_KeyMenu", "Mes Clefs", "Clefs que vous possédez", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR);
            menu.ItemSelectCallback = MainMenuManager;

            if (ListVehicleKey.Count <= 0)
            {
                Client.SendNotificationError("Vous n'avez pas de clées sur vous!");
                return;
            }
            else
            {
                foreach (VehicleKey key in ListVehicleKey)
                {
                    menu.Add(new MenuItem(key.VehicleName, "", "ID_Clefs", executeCallback: true, rightLabel: key.Plate));
                }
            }

            menu.OpenMenu(Client);
        }
        #endregion

        #region Callback
        private void MainMenuManager(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menu.Id == "ID_MainMenu")
            {
                switch (menuItem.Id)
                {
                    case "ID_Animations":
                        AnimationsMenu.OpenAnimationsMenu(Client);
                        break;
                    case "ID_WalkingStyles":
                        if ( client.GetPlayerHandler().PlayerSync.Injured)
                        {
                            client.SendNotification("Vous êtes blessé, vous ne pouvez pas changer votre style de marche");
                            return;
                        }

                        WalkingStyleMenu.OpenWalkingStyleMenu(Client);
                        break;
                    case "ID_Disconnect":
                        Client.Kick("Disconnected");
                        break;
                    case "ID_Clefs":
                        OpenKeysMenu();
                        break;
                    case "ID_Face":
                        MenuManager.CloseMenu(client);
                        FaceMenu.OpenFaceMenu(client);
                        break;
                    case "ID_Bourse":
                        menu.ClearItems();
                        menu.Title = menuItem.Text;
                        menu.SubTitle = "";

                        foreach(var value in GameMode.Instance.Economy.Bourse.Values)
                        {
                            var menuitem = new MenuItem(LoadItem.GetItemWithID(value.Key)?.name, "", "", rightLabel: Math.Round(value.Value, 2) + "%");
                            menuitem.Description = "Prix à l'unité: $" + GameMode.Instance.Economy.Bourse.GetCurrentPrice(value.Key, FarmManager.GetItemPrice(value.Key));
                            menu.Add(menuitem);
                        }
                        menu.OpenMenu(Client);

                        break;
                    default:
                        break;
                }
            }
            else if (menu.Id == "ID_KeyMenu")
            {
                if (menuItem == null)
                {
                    OpenPlayerMenu();
                    return;
                }

                menu = new Menu("ID_KeyOption", menuItem.RightLabel, "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR);
                keygiven = ListVehicleKey.Find(x => x.Plate == menuItem.RightLabel) ?? null;

                if (keygiven == null)
                    return;

                menu.ItemSelectCallback = MainMenuManager;
                menu.Add(new MenuItem("Donner", "", "ID_Give", executeCallback: true));
                menu.Add(new MenuItem("Jeter", "~r~Détruire la clefs", "ID_Delete", executeCallback: true));

               if (Factions.FactionManager.LSCustom.HasPlayerIntoFaction(this.Client))
                     menu.Add(new MenuItem("Dupliquer", "", "ID_Duplicate", executeCallback: true, rightLabel:$"${keyDupliPrice}"));

                menu.OpenMenu(Client);
            }
            else if (menu.Id == "ID_KeyOption")
            {
                if (menuItem == null)
                {
                    OpenKeysMenu();
                    return;
                }

                if (menuItem.Id == "ID_Give")
                {
                    menu = new Menu("ID_GiveMenu", menuItem.Text, "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR);
                    menu.ItemSelectCallback = MainMenuManager;

                    var players = client.GetNearestPlayers(2, true, client.Dimension);

                    if (players.Count > 0)
                    {
                        try
                        {
                            var destinataire = players[0].GetPlayerHandler();

                            if (destinataire != null && destinataire.Client.Exists)
                            {
                                destinataire.ListVehicleKey.Add(keygiven);
                                destinataire.UpdateFull();
                                destinataire.Client.SendNotificationSuccess($"Vous avez reçu la clé du véhicule {keygiven.VehicleName}");

                                if (keygiven != null)
                                    ListVehicleKey.Remove(keygiven);
                                this.UpdateFull();
                                Client.SendNotificationSuccess($"Vous avez donné la clé du véhicule {keygiven.VehicleName}");

                            }
                        }
                        catch (Exception ex)
                        {
                            AltV.Net.Alt.Server.LogDebug($"Give key {PID} destinataire: {players[0].GetSocialClub()} - {ex}");
                        }
                    }
                    else
                        Client.SendNotificationError("Personne autour de vous!");

                    menu.CloseMenu(client);
                }
                else if (menuItem.Id == "ID_Delete")
                {
                    if (keygiven != null)
                        ListVehicleKey.Remove(keygiven);

                    Client.SendNotificationSuccess("Vous avez jeté la clé du véhicule " + menu.Title);
                    UpdateFull();
                    MenuManager.CloseMenu(client);
                }
                else if (menuItem.Id == "ID_Duplicate")
                {
                    bool result = false;
                    result = Factions.FactionManager.LSCustom.BankAccount.GetBankMoney(350, "Duplication de la clé: " + menu.Title);

                    if (result)
                    {
                        if (keygiven != null)
                            ListVehicleKey.Add(keygiven);

                        Client.SendNotificationSuccess("Vous avez dupliqué la clé du véhicule " + menu.Title);
                    }
                    else
                        client.SendNotificationError("Vous n'avez pas l'argent nécessaire pour une copie de cette clé.");
                }
            }
        }
        #endregion
    }
}
