using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Inventory;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Utils.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Houses
{
    public class HouseMenu
    {
        #region Private fields
        private IPlayer _client;
        private House _house;
        private Menu _menu;
        #endregion

        #region Constructor
        public HouseMenu()
        { }
        #endregion

        #region Menu
        public static async Task OpenHouseMenu(IPlayer client, House house)
        {
            var houseMenu = new HouseMenu();
            houseMenu._client = client;
            houseMenu._house = house;

            houseMenu._menu = new Menu("houseMenu", "House Menu", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true);
            houseMenu._menu.BannerColor = new MenuColor(0, 255, 255, 0);
            houseMenu._menu.ItemSelectCallback = houseMenu.HouseCallBackMenu;

            houseMenu._menu.Add(new MenuItem("Ouvrir le coffre", "", "openInventory", executeCallback: true));

            if (house.Locked)
                houseMenu._menu.Add(new MenuItem("Ouvrir la porte à clef", "", "opencloseDoor", executeCallback: true));
            else
                houseMenu._menu.Add(new MenuItem("Fermer la porte à clef", "", "opencloseDoor", executeCallback: true));

            PlayerHandler ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if (ph.StaffRank >= AdminRank.Moderator || houseMenu._house.Owner.ToLower() == (client.GetSocialClub()).ToLower())
            {

                List<object> _playerlist = new List<object>();
                foreach (PlayerHandler player in client.GetNearestPlayers(7f))
                    _playerlist.Add(player.Identite.Name);

                _playerlist.Remove(ph.Identite.Name);
                if (_playerlist.Count > 0)
                {
                    ListItem _playerSelected = new ListItem("Vendre la maison à:", "Choix de la cible", "playerSelected", _playerlist, 0);
                    _playerSelected.ExecuteCallback = true;
                    houseMenu._menu.Add(_playerSelected);
                }
            }

            await MenuManager.OpenMenu(client, houseMenu._menu);
        }
        #endregion

        #region Callback
        private async Task HouseCallBackMenu(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menu.Id == "houseMenu")
            {
                switch (menuItem.Id)
                {
                    case "openInventory":
                        var ph = client.GetPlayerHandler();
                        var inv = new RPGInventoryMenu(ph.PocketInventory, ph.OutfitInventory ,ph.BagInventory, _house.Inventory);
                        inv.OnMove += async (cl, inventaire) =>
                        {
                            ph.Update();
                            await _house.Save();
                        };
                        await menu.CloseMenu(client);
                        await inv.OpenMenu(client);
                        break;

                    case "opencloseDoor":
                        await _house.SetLock(!_house.Locked);

                        if (_house.Locked)
                            client.SendNotification("Vous avez fermé votre maison.");
                        else
                            client.SendNotification("Vous avez ouvert votre maison.");

                        await OpenHouseMenu(client, _house);
                        break;
                    case "playerSelected":

                        var listItem = menu.Items.Find(m => m.Id == "playerSelected") as ListItem;

                        var selected = listItem.SelectedItem;
                        var name = listItem.Items[selected];

                        var player = PlayerManager.GetPlayerByName(name.ToString());

                        if (player == null) return;
                        await _house.SetOwner(player.Client);
                        client.SendNotification($"Vous avez vendu la maison à {player.Identite.Name}");
                        client.SendNotification("Vous êtes désormais propriétaire de ce logement");
                        await MenuManager.CloseMenu(client);
                        break;
                    default:

                        break;
                }
            }
        }
        #endregion
    }
}
