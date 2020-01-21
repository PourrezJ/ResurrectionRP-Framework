using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Utils.Enums;
using System.Numerics;

namespace ResurrectionRP_Server
{
    public class Weazel : Society.Society
    {
        public int AnnoncePrice = 500;

        public Weazel(string societyName, Vector3 servicePos, uint blipSprite, int blipColor, string owner = null, Inventory.Inventory inventory = null, Parking parking = null) : base(societyName, servicePos, blipSprite, blipColor, owner, inventory, parking)
        {
        }

        public override void Init()
        {
            Ped vendor = Ped.CreateNPC(PedModel.Lifeinvad01, new Vector3(-591.5868f, -933.32306f, 23.871094f), 0);
            vendor.NpcInteractCallBack = (IPlayer client, Ped npc) => { OpenMenuWeazelNews(client); };
            base.Init();
        }

        public void OpenMenuWeazelNews(IPlayer player)
        {
            Menu menu = new Menu("Id_Weazel", "Weazel News", "Service d'annonce", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true);
            menu.ItemSelectCallback = LifeInvaderMenuCallBack;

            MenuItem x1 = new MenuItem("Créer une annonce", "Créer une annonce ~r~99 caractères max!", "ID_AnnonceX1", true, rightLabel: $"${AnnoncePrice + CalcPriceAnnonce(AnnoncePrice)}");
            x1.SetInput("", 50, InputType.Text);
            menu.Add(x1);
            menu.Add(new MenuItem("Fermer", "", "ID_Quit", true));
            menu.OpenMenu(player);
        }

        private void LifeInvaderMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            switch (menuItem.Id)
            {
                case "ID_AnnonceX1":
                    string message = menuItem.InputValue;

                    if (message == null)
                        return;
                    else if (message.Length > 99)
                    {
                        client.SendNotificationError("La longueur de votre message dépasse la limite autorisée de 99 caractères!");
                        return;
                    }

                    if (client.GetPlayerHandler().HasBankMoney(AnnoncePrice + CalcPriceAnnonce(AnnoncePrice), "Message Weazel News"))
                    {
                        Utils.Util.Delay(50000, () => Utils.Util.SendNotificationPicture(CharPicture.CHAR_DEFAULT, "Weazel News", "Message d'annonce:", message));
                        client.SendNotification("Votre annonce va être diffusée.");
                        BankAccount.AddMoney(AnnoncePrice);
                    }
                    else
                        client.SendNotificationError("Vous n'avez pas assez d'argent sur votre compte bancaire");

                    menu.CloseMenu(client);
                    break;

                case "ID_Quit":
                    menu.CloseMenu(client);
                    break;
            }
        }

        public double CalcPriceAnnonce(double price)
            => Economy.Economy.CalculPriceTaxe(price, GameMode.Instance.Economy.Taxe_Market);
    }
}
