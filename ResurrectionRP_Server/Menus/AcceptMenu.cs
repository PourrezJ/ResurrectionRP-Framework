using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using ResurrectionRP_Server.Utils;
using System.Collections.Generic;

namespace ResurrectionRP_Server
{
    public class AcceptMenu
    {
        #region Delegates
        public delegate void AcceptMenuCallbackDelegate(IPlayer client, bool reponse);
        #endregion

        #region Private fields
        public bool _closeAtEnd;
        private Menu menu;
        #endregion

        #region Public fields
        [JsonIgnore]
        public AcceptMenuCallbackDelegate AcceptMenuCallBack { get; set; }
        #endregion

        #region Constructor
        public AcceptMenu()
        { }
        #endregion

        #region Menu
        public static AcceptMenu OpenMenu(IPlayer client, string title = "", string subtitle = "", string acceptDesc = "", string refuseDesc = "", string rightlabel = "", Banner banner = null, bool backCloseMenu = true, bool closeAtEnd = true)
        {
            AcceptMenu accept = new AcceptMenu()
            {
                _closeAtEnd = closeAtEnd
            };

            accept.menu = new Menu("ID_AcceptMenu", title, subtitle, Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, !backCloseMenu, false, backCloseMenu);
            accept.menu.ItemSelectCallback = accept.MenuCallBack;

            if (banner != null)
                accept.menu.BannerSprite = banner;

            accept.menu.Items.AddRange(new List<MenuItem>() {
                new MenuItem("Accepter", acceptDesc, "ID_Accept", true, false, rightlabel),
                new MenuItem("Refuser", refuseDesc, "ID_Refuser", true)
            });

            accept.menu.OpenMenu(client);
            return accept;
        }
        #endregion

        #region Callback
        private void MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
                AcceptMenuCallBack?.Invoke(client, false);
            else if (menu.Id == "ID_AcceptMenu")
            {
                if (menuItem.Id == "ID_Accept")
                    AcceptMenuCallBack?.Invoke(client, true);
                else if (menuItem.Id == "ID_Refuser")
                    AcceptMenuCallBack?.Invoke(client, false);
            }

            if (_closeAtEnd)
                menu.CloseMenu(client);
        }
        #endregion
    }
}
