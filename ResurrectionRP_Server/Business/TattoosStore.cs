using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Entities.Players.Data;
using ResurrectionRP_Server.Loader.TattooLoader;
using ResurrectionRP_Server.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Business
{

    public class TattoosStore : Business
    {
        #region Main
        [BsonIgnore]
        public PlayerHandler ClientSelected;

        public TattoosStore(string businnessName, Models.Location location, uint blipSprite, int inventoryMax, PedModel pedhash = 0, string owner = null) : base(businnessName, location, blipSprite, inventoryMax, pedhash, owner)
        {
        }

        public override void Init()
        {
            MaxEmployee = 5;
            base.Init();
        }

        public override void OpenMenu(IPlayer client, Entities.Peds.Ped npc = null)
        {
            if (!( IsOwner(client) ||  this.IsEmployee(client)))
            {
                client.SendNotificationError("Vous n'êtes pas autorisé à tatouer!");
                return;
            }

            Menu mainmenu = new Menu("ID_TattooMain", "", "Choisissez une option :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true)
            {
                BannerSprite = Banner.Tattoos2,
                ItemSelectCallback = TattooMenuCallBack,
                Finalizer = MenuFinalizer
            };

            List<object> _playerlist = new List<object>();

            foreach (IPlayer player in client.GetNearestPlayers(5f, false))
                _playerlist.Add(player.GetPlayerHandler().Identite.Name);

            if (_playerlist.Count > 0)
            {
                ClientSelected = null;

                if ( IsOwner(client))
                    mainmenu.Add(new MenuItem("Gérer les finances", "", "ID_TakeMoney", true, rightLabel: $"${BankAccount.Balance}"));

                MenuItem depot = new MenuItem("Déposer de l'argent", "", "ID_Depot", true);
                depot.SetInput("", 10, InputType.UFloat, true);
                depot.OnMenuItemCallback = DepotMoneyMenu;
                mainmenu.Add(depot);

                mainmenu.Add(new ListItem("Client:", "Choix du client", "ID_PlayerSelect", _playerlist, 0, true));
                mainmenu.Add(new MenuItem("Tête", "", "ID_Head", true));
                mainmenu.Add(new MenuItem("Torse", "", "ID_Torso", true));
                mainmenu.Add(new MenuItem("Bras Gauche", "", "ID_LeftArm", true));
                mainmenu.Add(new MenuItem("Bras Droit", "", "ID_RightArm", true));
                mainmenu.Add(new MenuItem("Jambes Gauche", "", "ID_LeftLeg", true));
                mainmenu.Add(new MenuItem("Jambes Droite", "", "ID_RightLeg", true));
                MenuManager.OpenMenu(client, mainmenu);
            }
            else
                client.SendNotificationError("Aucun client autour.");
        }

        private void TattooMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menu.Id == "ID_TattooMain")
            {
                if (menuItem == null)
                {
                    OpenMenu(client);
                    return;
                }

                if (menuItem.Id == "ID_TakeMoney")
                {
                    Bank.BankMenu.OpenBankMenu(client, BankAccount, Bank.AtmType.Business, menu, TattooMenuCallBack);
                    return;
                }

                var listItem = menu.Items.Find(m => m.Id == "ID_PlayerSelect") as ListItem;
                var selected = listItem.SelectedItem;
                var name = listItem.Items[selected];

                ClientSelected = PlayerManager.GetPlayerByName(name.ToString());

                if (ClientSelected == null)
                {
                    client.SendNotificationError("Joueur inconnu");
                    menu.CloseMenu(client);
                }
                else
                {
                    if (ClientSelected.Client == client)
                    {
                        ChoiseBones(client, menuItem.Id);
                        return;
                    }

                    AcceptMenu accept = AcceptMenu.OpenMenu(ClientSelected.Client, "", "Voulez-vous vous faire tatouer?", banner: Banner.Tattoos2);
                    accept.AcceptMenuCallBack = (IPlayer tatouer, bool responce) =>
                    {
                        if (responce)
                            ChoiseBones(client, menuItem.Id);
                        else
                        {
                            menu.CloseMenu(client);
                            client.SendNotificationError("Le client ne veut pas être tatoué.");
                        }

                        return Task.CompletedTask;
                    };
                }
            }
            else if (menu.Id == "ID_TattooSelect")
            {
                if (menuItem == null)
                {
                    ResetTattoos();
                    OpenMenu(client);
                    return;
                }

                Tattoo Tattoo = (Tattoo)menuItem.GetData("Tattoo");

                if (ClientSelected == null)
                {
                    client.SendNotificationError("inconnu.");
                    return;
                }

                if (!ClientSelected.Client.Exists)
                    return;

                uint selectedTattoo = Alt.Hash((ClientSelected.Character.Gender == 0) ? Tattoo.HashNameMale : Tattoo.HashNameFemale);

                if (ClientSelected.Character.HasDecoration(selectedTattoo))
                {
                    Decoration decoration = ClientSelected.Character.Decorations.FirstOrDefault(d => d.Overlay == selectedTattoo);
                    ClientSelected.Character.Decorations.Remove(decoration);
                    ResetTattoos();
                    ClientSelected.UpdateInBackground();
                    UpdateInBackground();
                    client.SendNotificationSuccess("Vous avez retiré le tatouage");
                }
                else
                {
                    if (BankAccount.GetBankMoney(Tattoo.Price, $"Tatouage par {BusinnessName}"))
                    {
                        uint collection = Alt.Hash(Tattoo.Collection);
                        uint overlay = (ClientSelected.Character.Gender == 0) ? Alt.Hash(Tattoo.HashNameMale) : Alt.Hash(Tattoo.HashNameFemale);
                        ClientSelected.Character.Decorations.Add(new Decoration(collection, overlay));
                        ClientSelected.UpdateInBackground();
                        UpdateInBackground();
                        client.SendNotificationSuccess("Vous avez appliqué le tatouage");
                    }
                    else
                        client.SendNotificationError("Vous n'avez pas assez dans la caisse enregistreuse.");
                }

                OpenMenu(client);
            }
        }
        #endregion

        #region Choix & Preview
        private void TattooChoiseMenu(List<Tattoo> TattooList, IPlayer tatoueur)
        {
            Menu selectMenu = new Menu("ID_TattooSelect", "", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, false, Banner.Tattoos2);
            selectMenu.IndexChangeCallback = TattooChoiseIndex; 
            selectMenu.ItemSelectCallback = TattooMenuCallBack;
            selectMenu.Finalizer = MenuFinalizer;

            foreach (Tattoo Tattoo in TattooList)
            {
                uint overlay = Alt.Hash((ClientSelected.Character.Gender == 0) ? Tattoo.HashNameMale : Tattoo.HashNameFemale);

                if (overlay == 0)
                    continue;

                MenuItem item = new MenuItem(Tattoo.LocalizedName, "", "", true);

                if (ClientSelected.Character.HasDecoration(overlay))
                    item.RightBadge = BadgeStyle.Makeup;
                else
                    item.RightLabel = $"${Tattoo.Price}";

                item.SetData("Tattoo", Tattoo);
                selectMenu.Add(item);
            }

            MenuManager.OpenMenu(tatoueur, selectMenu);
            TattooChoiseIndex(tatoueur, selectMenu, 0, selectMenu.Items[0]);
        }

        private void TattooChoiseIndex(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            if (ClientSelected != null)
            {
                Tattoo Tattoo = menuItem.GetData("Tattoo");
                uint collection = Alt.Hash(Tattoo.Collection);
                uint overlay = Alt.Hash((ClientSelected.Character.Gender == 0) ? Tattoo.HashNameMale : Tattoo.HashNameFemale);
                ResetTattoos();

                if (!ClientSelected.Character.HasDecoration(overlay))
                    ClientSelected.Client.SetDecoration(collection, overlay);
            }
        }

        private void ResetTattoos()
        {
            if (ClientSelected == null || !ClientSelected.Client.Exists)
                return;

            ClientSelected.Client.ClearDecorations();

            foreach (Decoration decoration in ClientSelected.Character.Decorations)
                ClientSelected.Client.SetDecoration(decoration.Collection, decoration.Overlay);
        }
        #endregion

        #region Menu Finalizer
        private void MenuFinalizer(IPlayer client, Menu menu)
        {
            ResetTattoos();
        }
        #endregion

        #region Depot
        // Depot d'argent dans la caisse.
        private void DepotMoneyMenu(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (double.TryParse(menuItem.InputValue, out double amount))
            {
                if (amount <= 0)
                    return;

                var ph = client.GetPlayerHandler();

                if (ph.HasMoney(amount))
                {
                    BankAccount.AddMoney(amount, $"Ajout d'argent par {ph.Identite.Name}");
                    client.SendNotificationSuccess($"Vous avez déposé ${amount} dans la caisse.");
                }
                else
                    client.SendNotificationError("Vous n'avez pas assez d'argent sur vous.");
            }

            OpenMenu(client);
        }

        private void ChoiseBones(IPlayer client, string bone)
        {
            switch (bone)
            {
                case "ID_Head":
                    TattooChoiseMenu(TattooLoader.HeadTattooList, client);
                    break;
                case "ID_Torso":
                    TattooChoiseMenu(TattooLoader.TorsoTattooList, client);
                    break;
                case "ID_LeftArm":
                    TattooChoiseMenu(TattooLoader.LeftArmTattooList, client);
                    break;
                case "ID_RightArm":
                    TattooChoiseMenu(TattooLoader.RightArmTattooList, client);
                    break;
                case "ID_LeftLeg":
                    TattooChoiseMenu(TattooLoader.LeftLegTattooList, client);
                    break;
                case "ID_RightLeg":
                    TattooChoiseMenu(TattooLoader.RightLegTattooList, client);
                    break;
            }
        }
        #endregion
    }
}
