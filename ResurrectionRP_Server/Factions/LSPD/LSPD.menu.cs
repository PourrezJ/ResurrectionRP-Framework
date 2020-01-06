using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils.Enums;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.XMenuManager;
using AltV.Net;
using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Services;

namespace ResurrectionRP_Server.Factions
{

    public partial class LSPD : Faction
    {
        #region Cellule
        private void OpenCelluleDoor(IPlayer client, Door door)
        {
            if (FactionManager.IsLspd(client))
            {
                XMenu xmenu = new XMenu("ID_Door");
                xmenu.SetData("Door", door);

                XMenuItem item = new XMenuItem($"{((door.Locked) ? "Ouvrir" : "Fermer")} la porte", "", icon: (door.Locked) ? XMenuItemIcons.DOOR_CLOSED_SOLID : XMenuItemIcons.DOOR_OPEN_SOLID);
                item.OnMenuItemCallback = OnDoorCall;
                xmenu.Add(item);

                xmenu.OpenXMenu(client);
            }
        }

        private static void OnDoorCall(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            Door door = menu.GetData("Door");

            if (door != null)
                door.SetDoorLockState(!door.Locked);

            XMenuManager.XMenuManager.CloseMenu(client);
        }
        #endregion

        #region Interaction Player
        public override XMenu InteractPlayerMenu(IPlayer client, IPlayer target, XMenu xmenu)
        {
            xmenu.SetData("Player", target);

            var search = new XMenuItem("Fouiller", "", "", XMenuItemIcons.HAND_PAPER_SOLID, true);
            search.OnMenuItemCallbackAsync = SearchPlayer;
            xmenu.Add(search);

            var penalty = new XMenuItem("Amende", "Mettre une amende", "ID_Invoice", XMenuItemIcons.FILE_INVOICE_SOLID, true);
            penalty.OnMenuItemCallback = InvoicePlayer;
            xmenu.Add(penalty);

            return base.InteractPlayerMenu(client, target, xmenu);
        }
        #endregion

        #region Menu Interaction NPC accueil
        public virtual void OpenAccueilMenu(IPlayer client)
        {
            if (!client.Exists)
                return;

            List<Invoice> Invoices = InvoiceList.FindAll(b => (b.SocialClub == client.GetSocialClub() && b.paid == false));

            if (Invoices.Count == 0)
                client.DisplayHelp("Vous n'avez aucune amende à payer !", 5000);

            Menu menu = new Menu("ID_Accueil", FactionName, "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true);
            menu.ItemSelectCallback = AccueilMenuCallback;
            //List<Invoice> amendes = InvoiceList.FindAll(b => b.SocialClub == client.GetSocialClub());
            //MenuItem menuitem = new MenuItem("Payer mes amendes", rightLabel: amendes.Count.ToString());
            //menu.Add(menuitem);
            // Quand il y aura plus d'élèment à mettre dans le menu on n'affichera pas directement les amendes
            foreach (Invoice item in Invoices)
            {
                string title = $"Payer {item.Desc}";
                string desc = $"Payer ${item.Amount}, reçu le {item.Date}";
                double pay = Convert.ToDouble(item.Amount);

                if (item.Date.AddDays(7) < DateTime.Now.AddYears(20))
                {
                    desc += $"\nUne majoration de 30% (~r~$+{item.Amount * 0.3}~w~) sera appliquée dû au retard de paiement.";
                    pay = Convert.ToDouble(item.Amount * 1.3);
                }

                MenuItem menuItem = new MenuItem(title, desc, id: "ID_PayInvoice", executeCallback: true, rightLabel: $"${pay}");
                menuItem.SetData("price", pay);
                menuItem.SetData("name", item.Desc);
                menuItem.SetData("invoice", item);
                menu.Add(menuItem);
            }

            menu.OpenMenu(client);
        }

        private void AccueilMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            switch (menuItem.Id)
            {
                case "ID_PayInvoice":
                    var clientHandler = client.GetPlayerHandler();

                    Invoice invoice = menuItem.GetData("invoice");

                    if (invoice != null)
                    {
                        if (!clientHandler.BankAccount.GetBankMoney(menuItem.GetData("price"), $"Réglement amende {menuItem.GetData("name")}"))
                        {
                            client.DisplayHelp( "Vous n'avez pas assez d'argent en banque pour payer l'amende.", 5000);
                            client.PlaySoundFrontEnd(-1, "ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET");
                            menu.CloseMenu(client);
                            return;
                        }

                        BankAccount.AddMoney(Convert.ToDouble(menuItem.GetData("price")), $"Paiement par {client.Name}, réglement amende {menuItem.GetData("name")}", false);
                        invoice.paid = true;
                        UpdateInBackground();
                        client.DisplayHelp( $"Vous venez de payer ~r~${menuItem.GetData("price")}~w~ pour régler votre amende.", 5000);
                        menu.CloseMenu(client);
                    }
                    menu.CloseMenu(client);
                    if (InvoiceList.FindAll(b => (b.SocialClub == client.GetSocialClub() && b.paid == false)).Count > 0)
                        OpenAccueilMenu(client);

                    return;
            }
        }
        #endregion

        #region Invoice
        private void InvoicePlayer(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            IPlayer _target = menu.GetData("Player");

            if (_target == null)
                return;

            List<Motif> MotifList = new List<Motif>();
            MotifList.Add(new Motif("Arrêt Gênant", 1000));
            MotifList.Add(new Motif("Stationnement Gênant", 1500, "Mise en fourrière si le propriétaire n'est pas présent"));
            MotifList.Add(new Motif("Non respect du code de la route", 5000, "Non respect d'un stop, Feu tricolore...\n Retrait de 1 point"));
            MotifList.Add(new Motif("Véhicule non conforme", 100000, "Présence de pièces interdits (Turbo)"));
            MotifList.Add(new Motif("Conduite OFF ROAD", 5000, "Conduite hors piste"));
            MotifList.Add(new Motif("Excés de Vitesse", 10000));
            MotifList.Add(new Motif("Non maîtrise du véhicule", 10000));
            MotifList.Add(new Motif("Visage masqué", 1500));
            MotifList.Add(new Motif("Conduite sans permis", 50000));
            MotifList.Add(new Motif("Conduite dangeureuse", 25000, "Contre sens, rouler sur les trottoirs..."));
            MotifList.Add(new Motif("Délit de fuite", 30000));
            MotifList.Add(new Motif("Conduite en état d'ivresse", 17500));
            MotifList.Add(new Motif("Conduite sous l'emprise de stupéfiants", 17500));
            MotifList.Add(new Motif("Dégradation de bien public/privé", 2000, "Prix allant de 2000 à 2 000 000$ (Faire prix personnalisé)"));
            MotifList.Add(new Motif("Ivresse sur la voie publique", 1500));
            MotifList.Add(new Motif("Agression verbable", 1500));
            MotifList.Add(new Motif("Agression physique", 15000));
            MotifList.Add(new Motif("Harcèlement moral", 25000));
            MotifList.Add(new Motif("Harcèlement physique", 50000));
            MotifList.Add(new Motif("Exhibition", 5000, "Nudité"));
            MotifList.Add(new Motif("Violation de propriété privée", 3000));
            MotifList.Add(new Motif("Vol", 0, "Dédommagement 2x le prix de l'objet"));
            MotifList.Add(new Motif("Recel", 0, "Dédommagement 2x la valeur de l'objet"));
            MotifList.Add(new Motif("Vol de véhicule", 0, "Dédommagement 2x la valeur de l'objet"));
            MotifList.Add(new Motif("Vol de ressource", 0, "Dédommagement 2x la valeur de l'objet"));
            MotifList.Add(new Motif("Travail au noir", 15000));
            MotifList.Add(new Motif("Outrage à agent LSPD", 10000));
            MotifList.Add(new Motif("Outrage à un membre de la Chancellerie", 50000));
            MotifList.Add(new Motif("Non coopération", 15000, "Refus d'obtempérer à un injonction"));
            MotifList.Add(new Motif("Diffamation", 10000));
            MotifList.Add(new Motif("Organisation d'évènement, manifestation non déclaré / autorisé", 100000));
            MotifList.Add(new Motif("Participation à un évènement, manifestation non autorisée", 10000));
            MotifList.Add(new Motif("Arme non déclarée", 0, "50% de la valeur de l'arme"));
            MotifList.Add(new Motif("Tir avec arme légale", 50000, "Hors légitime défense"));
            MotifList.Add(new Motif("Consommation de stupéfiants", 10000, "10,000$ par unité, dans la limite de 2 unités"));
            MotifList.Add(new Motif("Vente de stupéfiants", 5000, "5000$ par unité, dans la limite de 10 unités"));
            MotifList.Add(new Motif("Prostitution", 5000));
            MotifList.Add(new Motif("Client de la prostitution", 10000));

            Menu motifMenu = new Menu("ID_Invoice_Motif", "Amende", backCloseMenu: true);
            motifMenu.ItemSelectCallback = InvoiceCallBack;
            motifMenu.SetData("Player", _target);

            var validate = new MenuItem("~o~Verbaliser la personne", "Confirmer la verbalisation", "ID_Validate", executeCallback: true);
            motifMenu.Add(validate);

            var motif = new MenuItem("Motif personnalisé", "Entrez le motif de votre amende", "ID_MotifCustom", executeCallback: true);
            motif.SetInput("Rentrer le motif de l'amende", 99, InputType.Text);
            motifMenu.Add(motif);

            var prix = new MenuItem("Prix personnalisé", "Entrez le prix de l'amende", "ID_PrixCustom", executeCallback: true);
            prix.SetInput("Entrez le prix de l'amende", 10, InputType.UNumber);
            motifMenu.Add(prix);

            //client.Call("Display_subtitle", "Entrez le motif de l'amende.", 5000);

            foreach (Motif item in MotifList)
            {
                var ui = new MenuItem(item.name, item.desc?.ToString(), "ID_Motif", executeCallback: true);
                ui.RightLabel = $"${item.price}";
                ui.SetData("price", item.price);
                ui.SetData("name", item.name);
                motifMenu.Add(ui);
            }

            XMenuManager.XMenuManager.CloseMenu(client);
            motifMenu.OpenMenu(client);

            //var motif = new MenuItem("Motif", "Sélectionner le motif", "ID_Motif", executeCallback: true);
            //motif.SetInput("Rentrer le motif de l'amende", 99, InputType.Text);
            //motif.OnMenuItemCallback = InvoiceMotif;
            //invoicemenu.Add(motif);

            /**var montant = new MenuItem("Montant", "Rentrer le montant de l'amende", "ID_Montant");
            montant.SetInput("Rentrer le montant de l'amende", 99, InputType.Text);
            invoicemenu.Add(montant);**/

            //var valider = new MenuItem("~g~Valider", "", "ID_Valider");
            //montant.SetInput("", 99, InputType.Text);
            //invoicemenu.Add(montant);

            //await invoicemenu.OpenMenu(client);
        }

        private void InvoiceCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            Invoice invoice = (!menu.HasData("CurrentInvoice")) ? new Invoice() : menu.GetData("CurrentInvoice");

            if (invoice.Player == null)
                invoice.Player = menu.GetData("Player");

            if (!invoice.Player.Exists)
                return;

            if (invoice.SocialClub == null)
                invoice.SocialClub = invoice.Player.GetSocialClub();

            switch (menuItem.Id)
            {
                case "ID_MotifCustom":
                    invoice.Desc = menuItem.InputValue;
                    menu.OpenMenu(client);
                    break;
                case "ID_PrixCustom":
                    try
                    {
                        invoice.Amount = Convert.ToDouble(menuItem.InputValue);
                    }
                    catch
                    {
                        client.SendNotificationError("~r~Le prix ne doit être exclusivement numérique.");
                    }
                    menu.OpenMenu(client);
                    break;
                case "ID_Motif":
                    invoice.Desc = menuItem.GetData("name");
                    invoice.Amount = menuItem.GetData("price");
                    break;
                case "ID_Validate":
                    if (invoice.Desc == null || invoice.Amount <= 0)
                    {
                        client.SendNotificationError( "~r~Vous devez avoir sélectionné un motif et un prix pour mettre une amende.");
                        break;
                    }
                    CreateInvoice(client, invoice);
                    PlayerHandler clientHandler = invoice.Player.GetPlayerHandler();
                    client.DisplayHelp( $"L'amende est enregistrée et a été envoyée à {clientHandler.Identite.Name}.", 10000);
                    invoice.Player.DisplayHelp( $"Vous venez de recevoir une amende.\nVous avez 7 jours pour vous rendre au poste.", 10000);
                    menu.CloseMenu(client);
                    break;
            }

            menu.SetData("CurrentInvoice", invoice);

            if (invoice.Desc != null || invoice.Amount > 0)
                client.DisplaySubtitle( $"~o~Motif~w~: {invoice.Desc} \n~g~Prix~w~: ~g~$~w~{invoice.Amount}", 5000);
        }

        private void CreateInvoice(IPlayer client, Invoice invoice)
        {
            try
            {
                InvoiceList.Add(invoice);
                UpdateInBackground();
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("CreateInvoice" + ex);
            }
        }
        #endregion

        #region Interaction Vehicle
        public override async Task<XMenu> InteractVehicleMenu(IPlayer client, IVehicle target, XMenu xmenu)
        {
            xmenu.SetData("Vehicle", target);

            var _identVeh = new XMenuItem("Identification du véhicule.", icon: XMenuItemIcons.ID_CARD_SOLID, executeCallback: true);
            _identVeh.OnMenuItemCallbackAsync = IdentVehicle;
            xmenu.Add(_identVeh);

            var _poundVehicle = new XMenuItem("Fourrière", "Mettre en fourrière", "", XMenuItemIcons.INFO, false);
            _poundVehicle.OnMenuItemCallback = PoundVehicle;
            xmenu.Add(_poundVehicle);

            return await base.InteractVehicleMenu(client, target, xmenu);
        }

        private void PoundVehicle(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            XMenuManager.XMenuManager.CloseMenu(client);
            IVehicle target = menu.GetData("Vehicle");
            if (target != null && target.Exists)
            {
                if ((Factions.FactionManager.LSCustom?.GetEmployeeOnline()).Count > 0) 
                {
                    // appel vers la fourrière
                    client.SendNotificationPicture(CharPicture.DIA_MIC, "QG LSPD", "Demande de mise en fourrière.", "Fourrière indisponible, contacter un dépanneur.");
                    return;
                }

                client.SendNotificationPicture(CharPicture.DIA_MIC, "QG LSPD", "Demande de mise en fourrière.", $"Besoin de retrait du véhicule {target.NumberplateText}" );

                Utils.Utils.Delay(30000, () =>
                {
                    VehicleHandler vh = target.GetVehicleHandler();
                    if (vh == null)
                        return;

                    client.SendNotificationPicture(CharPicture.DIA_MIC, "QG LSPD", "Demande de mise en fourrière.", "La fourrière est venu récupérer le véhicule.");
                    Task.Run(async ()=>await Pound.AddVehicleInPoundAsync(vh.VehicleData));
                });
            }
        }

        private async Task IdentVehicle(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            XMenuManager.XMenuManager.CloseMenu(client);
            IVehicle target = menu.GetData("Vehicle");
            if (target != null)
            {
                client.SendNotificationPicture(CharPicture.DIA_MIC, "QG LSPD", "Demande d'information:", "~r~Recherche en cours~w~.");

                Identite identite = null;
                string infos = string.Empty;

                string ownerId = target.GetVehicleHandler()?.VehicleData.OwnerID;
                if (!string.IsNullOrEmpty(ownerId))
                {
                    var player = PlayerManager.GetPlayerBySCN(ownerId);

                    var vh = target.GetVehicleHandler();

                    if (vh == null)
                        return;

                    if (!vh.VehicleData.PlateHide)
                    {
                        identite = (player != null) ? player.Identite : await Identite.GetOfflineIdentite(ownerId);

                        if (identite != null)
                        {
                            infos = $"Plaque {target.NumberplateText} \n" +
                            $"Appartient à: {identite.Name}";
                        }
                        else
                        {
                            infos = $"Véhicule {target.NumberplateText} inconnu!";
                        }
                    }
                    else
                    {
                        infos = "Véhicule introuvable dans notre registre.";
                    }
                }

                Utils.Utils.Delay(20000, () => {
                    client.SendNotificationPicture(CharPicture.DIA_MIC, "QG LSPD", "Information trouvées:", infos );
                });
            }
        }
        #endregion

        #region Private methods
        private Task SearchPlayer(IPlayer client, XMenu menu, XMenuItem menuItem, int itemIndex, dynamic data)
        {
            return Task.CompletedTask;
        }
        #endregion
    }
}
