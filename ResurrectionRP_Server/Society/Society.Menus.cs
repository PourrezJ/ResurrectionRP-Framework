using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Utils.Enums;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Factions;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Bank;


namespace ResurrectionRP_Server.Society
{
    public partial class Society
    {
        public virtual async Task OpenParkingMenu(IPlayer client)
        {
            if (Parking != null)
            {
                if (await IsEmployee(client) || Owner == client.GetSocialClub())
                    await Parking.OpenParkingMenu(client);
                else
                    client.SendNotificationError("Vous ne faites pas partie des employés de cette entreprise.");
            }
        }

        public virtual async Task<Menu> OpenServerJobMenu(IPlayer client)
        {
            Menu menu = new Menu("ID_SocietyMainMenu", SocietyName, "Administration de la société", 0, 0, Menu.MenuAnchor.MiddleRight, false, true, true);
            menu.ItemSelectCallback += SocietyCallBackMenu;

            if (Owner != null && (client.GetPlayerHandler().StaffRank >= AdminRank.Moderator || FactionManager.IsGouv(client)))
            {
                var identite = (await Identite.GetOfflineIdentite(Owner)).Name ?? Owner;
                menu.SubTitle = $"Propriétaire: {identite}";
            }

            if (client.GetPlayerHandler()?.StaffRank > AdminRank.Player)
            {
                MenuItem item = new MenuItem("Changer de propriétaire", "", "pproprio", executeCallback: true);
                item.SetInput("Social Club Name", 60, InputType.Text);
                menu.Add(item);

                if (Parking == null)
                {
                    menu.Add(new MenuItem("~r~Ajouter un parking", "", "ID_AddParking", executeCallback: true));
                }
                else
                {
                    menu.Add(new MenuItem("~r~Modifier le parking", "", "ID_ModifyParking", executeCallback: true));
                }
            }

            if (Owner == client.GetSocialClub() || await IsEmployee(client))
            {
                if (InService.Contains(client.GetSocialClub()))
                    menu.Add(new MenuItem("Quitter votre service", "", "qservice", executeCallback: true));
                else
                    menu.Add(new MenuItem("Prendre votre service", "", "pservice", executeCallback: true));

                menu.Add(new MenuItem("Ouvrir l'inventaire", "", "dinventaire", executeCallback: true));


                if (Owner == client.GetSocialClub() || FactionManager.IsGouv(client))
                {
                    MenuItem getmoney = new MenuItem($"Gérer les finances", $"Caisse de l'entreprise: ${BankAccount.Balance}", "ID_money", executeCallback: true);
                    getmoney.OnMenuItemCallback = FinanceMenu;
                    menu.Add(getmoney);

                    menu.Add(new MenuItem("Gestion des employés", "", "gemployee", executeCallback: true));

                    MenuItem item = new MenuItem("Changer de nom d'entreprise", "", "ID_NameChange", executeCallback: true, rightLabel: $"${PriceNameChange}");
                    item.SetInput(SocietyName, 60, InputType.Text);
                    menu.Add(item);

                    // REVENTE
                    if (Resell)
                    {
                        menu.Add(new MenuItem("Annuler la vente", "", "ID_CancelResell", executeCallback: true));
                    }
                    else
                    {
                        item = new MenuItem("~r~Mettre en vente votre affaire", "", "ID_Resell", executeCallback: true);
                        item.SetInput("", 10, InputType.UNumber);
                        menu.Add(item);
                    }
                }
                else
                {
                    MenuItem depot = new MenuItem("Déposer de l'argent dans les caisses", "", "ID_Depot", true);
                    depot.SetInput("", 10, InputType.UFloat, true);
                    depot.OnMenuItemCallback = DepotMoneyMenu;
                    menu.Add(depot);
                }
            }
            else
            {
                if (Resell)
                {
                    menu.Add(new MenuItem($"Acheter {SocietyName}", "", "ID_Buy", true, rightLabel: $"${ResellPrice}"));
                }
                else
                {
                    client.SendNotificationError("Vous n'êtes pas autorisé à prendre ce job");
                }
            }

            await MenuManager.OpenMenu(client, menu);
            return menu;
        }

        // Depot d'argent dans la caisse.
        private async Task DepotMoneyMenu(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (double.TryParse(menuItem.InputValue, out double result))
            {
                var ph = client.GetPlayerHandler();

                if (ph == null)
                    return;

                if (result < 0) return;
                if (await ph.HasMoney(result))
                {
                    BankAccount.AddMoney(result, $"Ajout d'argent par {ph.Identite.Name}");
                    await ph.Update();
                    client.SendNotificationSuccess($"Vous avez déposé ${result} dans la caisse.");
                }
                else
                    client.SendNotificationError("Vous n'avez pas assez d'argent sur vous.");
            }
        }

        // Récupérer l'argent dans la caisse.
        private async Task FinanceMenu(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                await OpenServerJobMenu(client);
                return;
            }

            menu = await BankMenu.OpenBankMenu(client, BankAccount, AtmType.Faction, menu, FinanceMenu);
        }

        private async Task SocietyCallBackMenu(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            PlayerHandler ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            switch (menuItem.Id)
            {
                case "pservice":
                    await PriseService(client);
                    await MenuManager.CloseMenu(client);
                    break;

                case "qservice":
                    await QuitterService(client);
                    await MenuManager.CloseMenu(client);
                    break;
                case "gemployee":
                    await GestionEmployee(client, menu);
                    break;

                case "dinventaire":
                    Inventory.Locked = true;
                    await menu.CloseMenu(client);
                    var invmenu = new Inventory.RPGInventoryMenu(ph.PocketInventory, ph.OutfitInventory, ph.BagInventory, Inventory, true);

                    invmenu.OnMove += async (p, m) =>
                    {
                        await ph.Update();
                        await Update();
                    };

                    invmenu.OnClose += (p, m) =>
                    {
                        Inventory.Locked = false;
                        return Task.CompletedTask;
                    };

                    await invmenu.OpenMenu(client);
                    break;
                case "pproprio":
                    string socialClub = menuItem.InputValue;

                    if (!string.IsNullOrEmpty(socialClub))
                    {
                        PlayerHandler player = await PlayerManager.GetPlayerHandlerDatabase(socialClub);

                        if (player == null)
                            client.SendNotificationError("Joueur inconnu");
                        else
                        {
                            Owner = socialClub;
                            await Update();
                            client.SendNotificationSuccess("Propriétaire changé");
                            await OpenServerJobMenu(client);
                        }
                    }
                    break;
                case "ID_CancelResell":
                    Resell = false;
                    client.SendNotificationSuccess($"Vous avez annulé la vente de {SocietyName}");
                    await MenuManager.CloseMenu(client);
                    break;
                case "ID_Resell":
                    if (int.TryParse(menuItem.InputValue, out int _resell))
                    {
                        ResellPrice = _resell;
                        Resell = true;
                        client.SendNotificationSuccess($"Vous avez mis en vente {SocietyName} pour la somme de ${ResellPrice}");
                        await menu.CloseMenu(client);
                    }
                    else
                    {
                        client.SendNotificationError("Erreur dans la saisie!");
                    }
                    break;
                case "ID_Buy":
                    if (await ph.HasBankMoney(ResellPrice, $"Achat société {SocietyName}."))
                    {
                        Resell = false;
                        Owner = client.GetSocialClub();
                        client.SendNotificationSuccess($"Vous avez acheté {SocietyName} pour la somme de ${ResellPrice}");
                    }
                    else
                    {
                        client.SendNotificationError($"Vous n'avez pas assez d'argent sur votre compte.");
                    }
                    break;
                case "ID_NameChange":
                    string societyName = menuItem.InputValue;

                    if (!string.IsNullOrEmpty(societyName) && societyName != SocietyName)
                    {
                        if (await ph.HasBankMoney(PriceNameChange, "Changement de nom de societé."))
                        {
                            SocietyName = societyName;
                            await Update();
                            client.SendNotificationSuccess($"Vous avez changé le nom en {SocietyName}");
                        }
                        else
                            client.SendNotificationError("Vous n'avez pas assez d'argent en banque");
                    }
                    break;
                case "ID_AddParking":
                    if (SocietyManager.AddParkingList.TryAdd(client, this))
                    {
                        client.SendNotification("Tapez dans le tchat la commande /addparkingsociety pour ajouter le parking");
                    }
                    else
                    {
                        client.SendNotificationError("Un parking est déjà disponible pour cette société.");
                    }
                    break;
                default:
                    break;
            }
        }

        public virtual async Task GestionEmployee(IPlayer client, Menu menu)
        {
            menu.Reset();
            menu.BackCloseMenu = false;
            menu.SubTitle = "Gestion des employés";
            menu.ItemSelectCallback = GestionEmployeeCallback;

            MenuItem ajouter = new MenuItem("Ajouter un employé", "", "add_employe", executeCallback: true);
            ajouter.Description = "Mettez le prénom puis le nom de famille pour l'ajouter.";
            ajouter.SetInput("Prénom Nom", 60, InputType.Text);
            menu.Add(ajouter);

            string ownerName = client.GetPlayerHandler()?.Identite.Name;

            if (Employees.Count > 0)
            {
                foreach (var employe in Employees)
                {
                    menu.Add(new MenuItem(employe.Value, "", "delete_employe", executeCallback: true));
                }
            }

            await menu.OpenMenu(client);
        }

        private async Task GestionEmployeeCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                await OpenServerJobMenu(client);
                return;
            }

            if (menuItem.Id == "add_employe")
            {
                string _msg = menuItem.InputValue;

                if (!string.IsNullOrEmpty(_msg))
                {
                    var ph = PlayerManager.GetPlayerByName(_msg);

                    if (Employees.Count >= MaxEmployee)
                    {
                        client.SendNotificationError("Vous avez atteint le nombre maximun d'employés.");
                        return;
                    }
                    else if (Employees.Any(p => p.Value.ToLower() == _msg.ToLower()))
                    {
                        client.SendNotificationError("Cette personne fait déjà partie des employés");
                        return;
                    }

                    if (ph != null)
                    {
                        Employees.Add(ph.PID, ph.Identite.Name);
                        client.SendNotificationSuccess($"{_msg} est ajouté à la liste des employés");
                        await Update();
                    }
                    else
                        client.SendNotificationError($"{_msg} est introuvable.");
                }
                else
                    client.SendNotificationError("Aucun nom de rentré.");

                await GestionEmployee(client, menu);
            }
            else if (menuItem.Id == "delete_employe")
            {
                foreach (var playerID in Employees)
                {
                    PlayerHandler ph = await PlayerManager.GetPlayerBySCN(playerID.Key);

                    if (ph != null && ph.Identite.Name == menuItem.Text)
                    {
                        if (InService.Contains(playerID.Key))
                            await QuitterService(ph.Client);

                        Employees.Remove(ph.PID);
                        await Update();
                        client.SendNotificationSuccess(menuItem.Text + " est renvoyé.");
                        await GestionEmployee(client, menu);
                        break;
                    }
                    else if ((await Identite.GetOfflineIdentite(playerID.Key)).Name == menuItem.Text)
                    {
                        Employees.Remove(ph.PID);
                        await Update();
                        client.SendNotificationSuccess(menuItem.Text + " est renvoyé.");
                        await GestionEmployee(client, menu);
                        break;
                    }
                }
            }
        }
    }
}
