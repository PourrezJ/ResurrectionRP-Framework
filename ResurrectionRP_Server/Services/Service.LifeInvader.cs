using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Entities.Blips;
using ResurrectionRP_Server.Entities.Peds;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils.Enums;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Services
{
    public class LifeInvader
    {
        #region Public fields
        public static LifeInvader Instance
        {
            get
            {
                if (_instance == null) _instance = new LifeInvader();
                return _instance;
            }
            set => _instance = value;
        }

        public int AnnoncePrice = 1000;
        #endregion

        #region Private fields
        private static LifeInvader _instance;
        private readonly Location Location = new Location(new Vector3(-1081.521f, -244.8004f, 37.76327f), new Vector3(0, 0, 199.0072f));
        #endregion

        #region Methods
        public void Load()
        {
            BlipsManager.CreateBlip("Life Invader", Location.Pos, BlipColor.Red, 77);
            Ped vendor = Ped.CreateNPC(PedModel.Lifeinvad01, Location.Pos, Location.Rot.Z);
            vendor.NpcInteractCallBack = ((IPlayer client, Ped npc) =>
            {
                OpenMenuLifeInvader(client);
            });
        }

        public void OpenMenuLifeInvader(IPlayer player)
        {
            Menu menu = new Menu("Id_LifeInvader", "LifeInvader", "Service d'annonce", 0, 0, Menu.MenuAnchor.MiddleRight, false, true, true);
            menu.ItemSelectCallback = LifeInvaderMenuCallBack;

            MenuItem x1 = new MenuItem("Créer une annonce", "Créer une annonce ~r~99 caractères max!", "ID_AnnonceX1", true, rightLabel: $"${(AnnoncePrice + CalcPriceAnnonce(AnnoncePrice))}");
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
                    if (message.Length > 99)
                    {
                        client.SendNotificationError("La longueur de votre message dépasse la limite autorisée de 99 caractères max!");
                        return;
                    }
                    else
                    {
                        if (client.GetPlayerHandler().HasBankMoney(AnnoncePrice + CalcPriceAnnonce(AnnoncePrice), "Message Life Invader"))
                        {
                            Utils.Utils.Delay(50000, () => { Utils.Utils.SendNotificationPicture(CharPicture.CHAR_LIFEINVADER, "Life Invander", "Message d'annonce:", message); });
                            client.SendNotification("Votre annonce va être diffusée.");
                        }
                        else
                            client.SendNotificationError("Vous n'avez pas assez d'argent sur votre compte bancaire");

                        menu.CloseMenu(client);
                    }
                    break;
                case "ID_Quit":
                    menu.CloseMenu(client);
                    break;
            }
        }

        public double CalcPriceAnnonce(double price)
            => Economy.Economy.CalculPriceTaxe(price, GameMode.Instance.Economy.Taxe_Market);
        #endregion
    }
}
