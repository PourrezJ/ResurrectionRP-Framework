using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Menus;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
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
        public async Task OpenPlayerMenu()
        {
            if (PlayerSync.IsDead)
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
            menu.Add(new MenuItem("Déconnecter", "", "ID_Disconnect", true));

            await MenuManager.OpenMenu(Client, menu);
        }

        private async Task OpenKeysMenu()
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

            await menu.OpenMenu(Client);
        }
        #endregion

        #region Callback
        private async Task MainMenuManager(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menu.Id == "ID_MainMenu")
            {
                switch (menuItem.Id)
                {
                    case "ID_Animations":
                        await AnimationsMenu.OpenAnimationsMenu(Client);
                        break;
                    case "ID_WalkingStyles":
                        if ( client.GetPlayerHandler().PlayerSync.Injured)
                        {
                            client.SendNotification("Vous êtes blessé, vous ne pouvez pas changer votre style de marche");
                            return;
                        }

                        await WalkingStyleMenu.OpenWalkingStyleMenu(Client);
                        break;
                    case "ID_Disconnect":
                        await client.EmitAsync("PlayerDisconnect");
                        await Task.Delay(100);
                        await Client.KickAsync("Disconnected");
                        break;
                    case "ID_Clefs":
                        await OpenKeysMenu();
                        break;
                    case "ID_Face":
                        await MenuManager.CloseMenu(client);
                        await FaceMenu.OpenFaceMenu(client);
                        break;
                    default:
                        break;
                }
            }
            else if (menu.Id == "ID_KeyMenu")
            {
                if (menuItem == null)
                {
                    await OpenPlayerMenu();
                    return;
                }

                menu = new Menu("ID_KeyOption", menuItem.RightLabel, "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR);
                keygiven = ListVehicleKey.Find(x => x.Plate == menuItem.RightLabel) ?? null;

                if (keygiven == null)
                    return;

                menu.ItemSelectCallback = MainMenuManager;
                menu.Add(new MenuItem("Donner", "", "ID_Give", executeCallback: true));
                menu.Add(new MenuItem("Jeter", "~r~Détruire la clefs", "ID_Delete", executeCallback: true));

                // if (await GameMode.Instance.FactionManager.LSCustom.HasPlayerIntoFaction(this.Client))
                //     menu.Add(new MenuItem("Dupliquer", "", "ID_Duplicate", executeCallback: true, rightLabel:$"${keyDupliPrice}"));

                await menu.OpenMenu(Client);
            }
            else if (menu.Id == "ID_KeyOption")
            {
                if (menuItem == null)
                {
                    await OpenKeysMenu();
                    return;
                }

                if (menuItem.Id == "ID_Give")
                {
                    /*
                    menu = new Menu("ID_GiveMenu", menuItem.Text, "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR);
                    menu.Callback = MainMenuManager;
                    var players = await PlayerManager.GetNearestPlayers(client, 2, true, await client.GetDimensionAsync());

                    if (players.Count > 0)
                    {
                        try
                        {
                            var destinataire = players[0];

                            if (destinataire != null && destinataire.Client.Exists)
                            {
                                destinataire.ListVehicleKey.Add(keygiven);
                                await destinataire.Update();
                                await destinataire.Client.SendNotificationSuccess($"Vous avez reçu la clé du véhicule {keygiven.VehicleName}");

                                if (keygiven != null)
                                    ListVehicleKey.Remove(keygiven);
                                await this.Update();
                                await Client.SendNotificationSuccess($"Vous avez donné la clé du véhicule {keygiven.VehicleName}");

                            }
                        }
                        catch (Exception ex)
                        {
                            Alt.Server.LogError($"Give key {PID} destinataire: {players[0].PID} - {ex}");
                        }
                    }
                    else
                        await Client.SendNotificationError("Personne autour de vous!");

                    await menu.CloseMenu(client);
                    */
                }
                else if (menuItem.Id == "ID_Delete")
                {
                    if (keygiven != null)
                        ListVehicleKey.Remove(keygiven);

                    Client.SendNotificationSuccess("Vous avez jeté la clé du véhicule " + menu.Title);
                    UpdateFull();
                    await MenuManager.CloseMenu(Client);
                }
                else if (menuItem.Id == "ID_Duplicate")
                {
                    /*
                    if (await GameMode.Instance.FactionManager.LSCustom.BankAccount.GetBankMoney(350, "Duplication de la clé: " + menu.Title))
                    {
                        if (keygiven != null)
                            ListVehicleKey.Add(keygiven);

                        await Client.SendNotificationSuccess("Vous avez dupliqué la clé du véhicule " + menu.Title);
                    }
                    else
                        await client.SendNotificationError("Vous n'avez pas l'argent nécessaire pour une copie de cette clé.");
                    */
                }
            }
        }
        #endregion
    }
}
