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
using ResurrectionRP_Server.Utils;
using System.Linq;

namespace ResurrectionRP_Server.Factions
{
    public partial class Gouv : Faction
    {
        public virtual async Task OpenSecretaryMenu(IPlayer client)
        {
            if (client == null || !client.Exists)
                return;

            if ( HasPlayerIntoFaction(client) &&  this.GetRangPlayer(client) >= 5)
            {
                Menu menu = new Menu("ID_Accueil", FactionName, "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true);
                menu.ItemSelectCallbackAsync = SecretaireMenuCallback;

                MenuItem fisc = new MenuItem("Récupérer un dossier fiscal", "Vérifier la comptabilité d'une entreprise", "ID_ListBusiness", executeCallback: true);
                menu.Add(fisc);

                await menu.OpenMenu(client);
            }
            else
            {
                client.DisplayHelp("La secrétaire vous ignore, elle ne vous connait pas !", 10000);
            }

        }

        private async Task SecretaireMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                await OpenSecretaryMenu(client);
                return;
            }

            if (menuItem.Id == "ID_ListBusiness")
            {
                menu = new Menu("ID_Businesses", "Comptabilité", "Consulter la comptabilité d'une entreprise", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR);
                menu.ItemSelectCallbackAsync = SecretaireMenuCallback;

                foreach (Society.Society market in GameMode.Instance.SocietyManager.SocietyList)
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
                    client.DisplayHelp("Problème, faction non trouvée", 10000);
                    return;
                }

                Society.Society Society = menuItem.GetData("Society");

                if (Society == null)
                    return;

                client.DisplayHelp("Votre relevé sera disponible dans quelques instants sur votre tablette!", 5000);
                await menu.CloseMenu(client);

                String contentData = "HISTORIQUE BANCAIRE DU " + DateTime.Now.AddYears(20) + " POUR LA SOCIETE " + Society.SocietyName;

                List<BankAccountHistory> history = new List<BankAccountHistory>(Society.BankAccount.History);
                history = history.OrderByDescending(h => h.Date).ToList();
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
                        client.SendNotificationError("Ce service est actuellement indisponible, veuillez réeesayer ultérieurement");
                        Alt.Server.LogError("SecretaireMenuCallback: " + ex);
                        return;
                    }

                    WebClient.UploadValuesCompleted += (sender, args) => {
                        try
                        {
                            pastebinLink = Encoding.ASCII.GetString(args.Result);
                            client.DisplaySubtitle($"Dossier disponible: {pastebinLink}", 30000);
                        }
                        catch (Exception ex)
                        {
                            client.SendNotificationError("Ce service est actuellement indisponible, veuillez réeesayer ultérieurement");
                            Alt.Server.LogError("SecretaireMenuCallback: " + ex);
                        }
                    };
                }
            }
        }
    }
}
