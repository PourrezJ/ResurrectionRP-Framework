using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using ResurrectionRP_Server.Bank;
using ResurrectionRP_Server.Entities.Players;

namespace ResurrectionRP_Server.Factions
{

    public partial class Gouv : Faction
    {
/*        public virtual async Task OpenSecretaireMenu(IPlayer client) TODO
        {
            if (client == null || !client.Exists)
                return;

            if ( HasPlayerIntoFaction(client) &&  this.GetRangPlayer(client) >= 5)
            {
                Menu menu = new Menu("ID_Accueil", FactionName, "", 0, 0, Menu.MenuAnchor.MiddleRight, backCloseMenu: true);
                menu.ItemSelectCallback = SecretaireMenuCallback;

                MenuItem fisc = new MenuItem("Récupérer un dossier fiscal", "Vérifier la comptabilité d'une entreprise", "ID_ListBusiness", executeCallback: true);
                menu.Add(fisc);

                await menu.OpenMenu(client);
            }
            else
            {
                await client.DisplayHelp("La secrétaire vous ignore, elle ne vous connait pas !", 10000);
            }

        }*/
/*
        private async Task SecretaireMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                await OpenSecretaireMenu(client);
                return;
            }

            if (menuItem.Id == "ID_ListBusiness")
            {
                menu = new Menu("ID_Businesses", "Comptabilité", "Consulter la comptabilité d'une entreprise", 0, 0, Menu.MenuAnchor.MiddleRight);
                menu.ItemSelectCallback = SecretaireMenuCallback;

                foreach (Society market in GameMode.Instance.SocietyManager.SocietyList)
                {
                    var item = new MenuItem(market.SocietyName, "", "ID_BusinessSelect", executeCallback: true);
                    item.SetData("Society", market);
                    menu.Add(item);
                }

                await menu.OpenMenu(client);
            }
            else if (menuItem.Id == "ID_BusinessSelect")
            {
                if (menuItem.GetData("Society") == null)
                {
                    await client.DisplayHelp("Problème, faction non trouvée", 10000);
                    return;
                }

                Society Society = menuItem.GetData("Society");

                if (Society == null)
                    return;

                client.Call("Display_Help", "Votre relevé sera disponible dans quelques instants sur votre tablette!", 5000);
                await menu.CloseMenu(client);

                List<BankAccountHistory> history = Society.BankAccount.History;
                String contentData = "HISTORIQUE BANCAIRE DU " + DateTime.Now.AddYears(20) + " POUR LA SOCIETE " + Society.SocietyName;

                foreach (BankAccountHistory fisc in history)
                    contentData += $"\n[{fisc.Date}] [${fisc.Amount}] {fisc.Text}";

                contentData += "\nFIN HISTORIQUE BANCAIRE";
                string pastebinLink = "";

                using (var WebClient = new WebClient())
                {
                    NameValueCollection content = new NameValueCollection();
                    content.Add("api_option", "paste");
                    content.Add("api_paste_name", "Récapitulatif");
                    content.Add("api_dev_key", "41c56158417c5c4db6b934c5843b9748");
                    content.Add("api_paste_code", contentData);

                    try
                    {
                        WebClient.UploadValuesAsync(new Uri("https://pastebin.com/api/api_post.php"), content);
                    }
                    catch (Exception ex)
                    {
                        await client.SendNotificationError("Ce service est actuellement indisponible, veuillez réeesayer ultérieurement");
                        MP.Logger.Error("SecretaireMenuCallback", ex);
                        return;
                    }

                    WebClient.UploadValuesCompleted += async (sender, args) => {
                        try
                        {
                            pastebinLink = Encoding.ASCII.GetString(args.Result);
                            await client.SendSubTitle($"Dossier disponible: {pastebinLink}", 30000);
                        }
                        catch (Exception ex)
                        {
                            await client.SendNotificationError("Ce service est actuellement indisponible, veuillez réeesayer ultérieurement");
                            MP.Logger.Error("SecretaireMenuCallback", ex);
                        }
                    };
                }
            }
        }*/
    }
}
