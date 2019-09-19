using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;


namespace ResurrectionRP_Server.Business
{
    public partial class Business
    {
        public virtual async Task<Menu> OpenSellMenu(IPlayer client, Menu menu)
        {
            menu.ItemSelectCallback += MenuCallBack;

            if (Buyable && OnSale && Owner == client.GetSocialClub())
                menu.Add(new MenuItem("~r~Annuler la mise en vente", "", id: "ID_CancellSell", executeCallback: true));
            else if (Buyable && OnSale)
                menu.Add(new MenuItem("~r~Acheter le commerce", $"Acheter le commerce pour la somme de ${BusinessPrice}", "ID_Buy", true, rightLabel: $"${BusinessPrice}"));
            else if (Buyable && !OnSale && Owner ==  client.GetSocialClub())
            {
                MenuItem _item = new MenuItem("~r~Mettre en vente votre commerce", "ATTENTION! Vous ne toucherez la somme qu'une fois le commerce vendu.", id: "ID_Sell", executeCallback: true);
                _item.SetInput(BusinessPrice.ToString(), 10, InputType.UNumber);
                menu.Add(_item);
            }

            if (Owner ==  client.GetSocialClub())
                menu.Add(new MenuItem("Gestion des employés", "", id: "ID_AddStaff", executeCallback: true));

            if (Owner != null && (client.GetPlayerHandler().StaffRank >= Utils.Enums.AdminRank.Moderator || Factions.FactionManager.IsGouv(client)))
            {
                var old = (await Models.Identite.GetOfflineIdentite(Owner));
                var identite = "No Owner";

                if (old != null)
                    identite = old.Name;
                else
                    identite = Owner;

                menu.SubTitle = $"Owner: {identite} | Inactivité: {this.Inactivity.ToShortDateString()}";
                menu.Add(new MenuItem("~r~Retirer le propriétaire", "Remet en vente le commerce", "ID_ClearAdmin", true));
            }

            if (client.GetPlayerHandler().StaffRank >= Utils.Enums.AdminRank.Moderator)
                menu.Add(new MenuItem("~r~Supprimer le commerce", "Supprimer le commerce", "ID_DeleteAdmin", true));

            menu.Add(new MenuItem("Fermer", "", "ID_Close", true));
            await menu.OpenMenu(client);
            return menu;
        }

        private async Task MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
                return;

            if (menu.Id == "ID_SellMenu")
            {
                if (menuItem.Id == "ID_Buy")
                {
                    await Buy(client);
                    await OnNpcSecondaryInteract(client, Ped);
                }
                else if (menuItem.Id == "ID_Sell")
                {
                    if (int.TryParse(menuItem.InputValue, out int value))
                        await Sell(client, value);
                    else
                        client.SendNotificationError("Montant non valide");

                    await OnNpcSecondaryInteract(client, Ped);
                }
                else if (menuItem.Id == "ID_CancellSell")
                {
                    await CancelSell(client);
                    await OnNpcSecondaryInteract(client, Ped);
                }
                else if (menuItem.Id == "ID_Close")
                {
                    await MenuManager.CloseMenu(client);
                }
                else if (menuItem.Id == "ID_ClearAdmin")
                {
                    Owner = null;
                    OnSale = true;
                    BusinessPrice = 0;
                    BankAccount.Clear();
                    await Update();
                    Entities.Blips.BlipsManager.SetColor(Blip, 35);
                    client.SendNotificationSuccess("Propriétaire retiré");
                    await OnNpcSecondaryInteract(client, Ped);
                }
                else if (menuItem.Id == "ID_DeleteAdmin")
                {
                    await Delete();
                    client.SendNotificationSuccess("Le magasin a été supprimé");
                    await MenuManager.CloseMenu(client);
                }
                else if (menuItem.Id == "ID_AddStaff")
                {
                    await GestionEmployee(client, menu);
                }
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
                await OnNpcSecondaryInteract(client, Ped);
                return;
            }

            if (menuItem.Id == "add_employe")
            {
                string _msg = menuItem.InputValue;

                if (!string.IsNullOrEmpty(_msg))
                {
                    var ph = Entities.Players.PlayerManager.GetPlayerByName(_msg);

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
                    if ((await Models.Identite.GetOfflineIdentite(playerID.Key)).Name == menuItem.Text)
                    {
                        Employees.Remove(playerID.Key);
                        await Update();
                        client.SendNotificationSuccess(menuItem.Text + " est renvoyé.");
                        await GestionEmployee(client, menu);
                        break;
                    }
                }
            }
        }

        public async Task Buy(IPlayer client)
        {
            Entities.Players.PlayerHandler ph = client.GetPlayerHandler();

            if (ph != null)
            {
                if (await ph.HasBankMoney(BusinessPrice, $"Achat de societé {BusinnessName}."))
                {
                    Owner = client.GetSocialClub();
                    OnSale = false;
                    await Update();
                    Entities.Blips.BlipsManager.SetColor(Blip, 2);
                    client.SendNotificationSuccess($"Vous avez acheté {BusinnessName} pour la somme de ${BusinessPrice}.");
                }
                else
                    client.SendNotification("Vous n'avez pas l'argent nécessaire pour acheter le commerce.");
            }
        }

        public async Task Sell(IPlayer client, int money)
        {
            OnSale = true;
            BusinessPrice = money;
            await Update();
            Entities.Blips.BlipsManager.SetColor(Blip, 35);
            client.SendNotificationSuccess($"Vous avez mis en vente {BusinnessName} pour la somme de ${BusinessPrice}.");
        }

        public async Task CancelSell(IPlayer client)
        {
            OnSale = false;
            Entities.Blips.BlipsManager.SetColor(Blip, 2);
            client.SendNotificationSuccess($"Vous avez annulé la mise en vente de {BusinnessName}.");
            await MenuManager.CloseMenu(client);
        }
    }
}
