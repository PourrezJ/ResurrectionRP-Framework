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
        public virtual Menu OpenSellMenu(IPlayer client, Menu menu)
        {
            menu.ItemSelectCallback += MenuCallBack;

            if (Buyable && OnSale && Owner == client.GetSocialClub())
                menu.Add(new MenuItem("~r~Annuler la mise en vente", "", id: "ID_CancellSell", executeCallback: true));
            else if (Buyable && (OnSale || Owner == null))
                menu.Add(new MenuItem("~r~Acheter le commerce", $"Acheter le commerce pour la somme de ${BusinessPrice}", "ID_Buy", true, rightLabel: $"${BusinessPrice}"));
            else if (Buyable && !OnSale && Owner ==  client.GetSocialClub())
            {
                MenuItem _item = new MenuItem("~r~Mettre en vente votre commerce", "ATTENTION! Vous ne toucherez la somme qu'une fois le commerce vendu.", id: "ID_Sell", executeCallback: true);
                _item.SetInput(BusinessPrice.ToString(), 10, InputType.UNumber);
                menu.Add(_item);
            }

            if (Owner ==  client.GetSocialClub())
                menu.Add(new MenuItem("Gestion des employés", "", id: "ID_AddStaff", executeCallback: true));

            if (Owner != null && (client.GetPlayerHandler().StaffRank >= Utils.Enums.StaffRank.Moderator || Factions.FactionManager.IsGouv(client)))
            {
                var old = (Models.Identite.GetOfflineIdentite(Owner));
                var identite = "No Owner";

                if (old != null)
                    identite = old.Name;
                else
                    identite = Owner;

                menu.SubTitle = $"Owner: {identite} | Inactivité: {this.Inactivity.ToShortDateString()}";
                menu.Add(new MenuItem("~r~Retirer le propriétaire", "Remet en vente le commerce", "ID_ClearAdmin", true));
            }

            if (client.GetPlayerHandler().StaffRank >= Utils.Enums.StaffRank.Moderator)
                menu.Add(new MenuItem("~r~Supprimer le commerce", "Supprimer le commerce", "ID_DeleteAdmin", true));

            menu.Add(new MenuItem("Fermer", "", "ID_Close", true));
            menu.OpenMenu(client);
            return menu;
        }

        private void MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
                return;

            if (menu.Id == "ID_SellMenu")
            {
                if (menuItem.Id == "ID_Buy")
                {
                    Buy(client);
                    OnNpcSecondaryInteract(client, Ped);
                }
                else if (menuItem.Id == "ID_Sell")
                {
                    if (int.TryParse(menuItem.InputValue, out int value))
                        Sell(client, value);
                    else
                        client.SendNotificationError("Montant non valide");

                    OnNpcSecondaryInteract(client, Ped);
                }
                else if (menuItem.Id == "ID_CancellSell")
                {
                    CancelSell(client);
                    OnNpcSecondaryInteract(client, Ped);
                }
                else if (menuItem.Id == "ID_Close")
                {
                    MenuManager.CloseMenu(client);
                }
                else if (menuItem.Id == "ID_ClearAdmin")
                {
                    Owner = null;
                    OnSale = true;
                    BusinessPrice = 0;
                    BankAccount.Clear();
                    UpdateInBackground();
                    Entities.Blips.BlipsManager.SetColor(Blip, 35);
                    client.SendNotificationSuccess("Propriétaire retiré");
                    OnNpcSecondaryInteract(client, Ped);
                }
                else if (menuItem.Id == "ID_DeleteAdmin")
                {
                    Delete();
                    client.SendNotificationSuccess("Le magasin a été supprimé");
                    MenuManager.CloseMenu(client);
                }
                else if (menuItem.Id == "ID_AddStaff")
                {
                    GestionEmployee(client, menu);
                }
            }
        }

        public virtual void GestionEmployee(IPlayer client, Menu menu)
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

            menu.OpenMenu(client);
        }

        private void GestionEmployeeCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                OnNpcSecondaryInteract(client, Ped);
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
                        UpdateInBackground();
                    }
                    else
                        client.SendNotificationError($"{_msg} est introuvable.");
                }
                else
                    client.SendNotificationError("Aucun nom de rentré.");

                GestionEmployee(client, menu);
            }
            else if (menuItem.Id == "delete_employe")
            {
                foreach (var playerID in Employees)
                {
                    if ((Models.Identite.GetOfflineIdentite(playerID.Key)).Name == menuItem.Text)
                    {
                        Employees.Remove(playerID.Key);
                        UpdateInBackground();
                        client.SendNotificationSuccess(menuItem.Text + " est renvoyé.");
                        GestionEmployee(client, menu);
                        break;
                    }
                }
            }
        }

        public void Buy(IPlayer client)
        {
            Entities.Players.PlayerHandler ph = client.GetPlayerHandler();

            if (ph != null)
            {
                if (ph.HasBankMoney(BusinessPrice, $"Achat de societé {BusinnessName}."))
                {
                    Owner = client.GetSocialClub();
                    OnSale = false;
                    UpdateInBackground();
                    Entities.Blips.BlipsManager.SetColor(Blip, 2);
                    client.SendNotificationSuccess($"Vous avez acheté {BusinnessName} pour la somme de ${BusinessPrice}.");
                }
                else
                    client.SendNotification("Vous n'avez pas l'argent nécessaire pour acheter le commerce.");
            }
        }

        public void Sell(IPlayer client, int money)
        {
            OnSale = true;
            BusinessPrice = money;
            UpdateInBackground();
            Entities.Blips.BlipsManager.SetColor(Blip, 35);
            client.SendNotificationSuccess($"Vous avez mis en vente {BusinnessName} pour la somme de ${BusinessPrice}.");
        }

        public void CancelSell(IPlayer client)
        {
            OnSale = false;
            Entities.Blips.BlipsManager.SetColor(Blip, 2);
            client.SendNotificationSuccess($"Vous avez annulé la mise en vente de {BusinnessName}.");
            MenuManager.CloseMenu(client);
        }
    }
}
