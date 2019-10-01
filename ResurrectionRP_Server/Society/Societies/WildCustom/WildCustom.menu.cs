using AltV.Net;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Society.Societies.WildCustom
{
    public partial class WildCustom : Garage
    {
        #region Constants
        private const double PRICE_CHANGE_PLATE = 50000;
        #endregion

        #region MainMenu
        protected override void OpenMainMenu(IPlayer client)
        {
            if (!(IsEmployee(client) || Owner == client.GetSocialClub()))
            {
                client.SendNotificationError("Hey mec tu veux quoi?!");
                return;
            }

            if (VehicleBench == null || !VehicleBench.Exists)
            {
                client.SendNotificationError("Aucun véhicule devant l'établi.");
                return;
            }

            try
            {
                var info = VehicleInfoLoader.VehicleInfoLoader.Get(VehicleBench.Model);

                if (info == null)
                {
                    client.SendNotificationError("Je ne touche pas à ce véhicule bordel.");
                    return;
                }
                else if (info.VehicleClass == 8)
                {
                    client.SendNotificationError("J'ai une gueule à faire de la moto?!");
                    return;
                }
                else if (BlackListCategories.Contains(info.VehicleClass))
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
            mainMenu.FinalizerAsync = Finalizer;

            MenuItem cleaning = new MenuItem("Nettoyage", "", "Cleaning", true);
            mainMenu.Add(cleaning);

            MenuItem design = new MenuItem("Esthétique", "", "Design", true);
            mainMenu.Add(design);

            MenuItem performance = new MenuItem("Performance", "", "Perf", true);
            mainMenu.Add(performance);

            MenuItem historique = new MenuItem("Historique", "", "Histo", true);
            mainMenu.Add(historique);

            if (Owner == client.GetSocialClub() && !VehicleBench.GetVehicleHandler().PlateHide)
            {
                MenuItem changePlate = new MenuItem("Retirer le véhicule du registre", "~r~Illégal ~w~Retirer le véhicule du registre de SanAndreas pour être non identifiable lors d'un contrôle.", "PlateChange", true);
                mainMenu.Add(changePlate);
            }

            mainMenu.OpenMenu(client);
            ClientInMenu = client;
        }

        protected override void MainMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                menu.CloseMenu(client);
                return;
            }

            if (menuItem.Id == "Design")
                OpenDesignMenu(client);
            else if (menuItem.Id == "Perf")
                OpenPerformanceMenu(client);
            else if (menuItem.Id == "Histo")
                OpenHistoricMenu(client);
            else if (menuItem.Id == "Cleaning")
                CleaningVehicle(client);
            else if (menuItem.Id == "PlateChange")
                OpenHidePlateMenu(client);
        }
        #endregion

        #region Cleaning
        private void CleaningVehicle(IPlayer client)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                MenuManager.CloseMenu(client);
                return;
            }

            client.SendNotificationPicture(Utils.Enums.CharPicture.CHAR_LS_CUSTOMS, SocietyName, "Nettoyage: ~r~Démarrage~w~.", "C'est parti!");

            Utils.Utils.SetInterval(() =>
            {
                if (VehicleBench == null || !VehicleBench.Exists)
                    return;

                client.SendNotificationPicture(Utils.Enums.CharPicture.CHAR_LS_CUSTOMS, SocietyName, "Nettoyage: ~g~Terminé~w~.", "Elle est niquel!");
                VehicleHandler vh = VehicleBench.GetVehicleHandler();

                if (vh == null)
                    return;

                vh.DirtLevel = 0;
                vh.UpdateInBackground(false);
            }, 20000);
        }
        #endregion

        #region Hide plate
        private void OpenHidePlateMenu(IPlayer client)
        {
            if (VehicleBench == null || !VehicleBench.Exists)
            {
                MenuManager.CloseMenu(client);
                return;
            }

            PlayerHandler ph = client.GetPlayerHandler();

            if (ph != null)
            {
                AcceptMenu accept = AcceptMenu.OpenMenu(client, "", "Retirer plaque du registre :", "~r~ATTENTION CETTE MODIFICATION EST ILLEGALE\nVous devez avoir l'argent sur vous", "", $"${PRICE_CHANGE_PLATE}", Banner.Garage, false, false);
                accept.AcceptMenuCallBack = (IPlayer c, bool response) =>
                {
                    if (response)
                    {
                        if (ph.HasMoney(PRICE_CHANGE_PLATE) == true)
                        {
                            VehicleHandler vh = VehicleBench.GetVehicleHandler();

                            if (vh != null)
                            {
                                vh.PlateHide = true;
                                client.SendNotificationSuccess("La plaque du véhicule a été retirée du registre");
                                vh.UpdateInBackground(false);
                            }
                        }
                        else
                            client.SendNotificationError("Tu n'as pas l'argent sur toi...");
                    }

                    OpenMainMenu(client);
                    return Task.CompletedTask;
                };
            }
        }
        #endregion
    }
}
