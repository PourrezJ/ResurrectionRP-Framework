using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server
{
    public class AcceptMenu
    {
        #region Delegates
        public delegate Task AcceptMenuCallbackDelegate(IPlayer client, bool reponse);
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

            accept.menu = new Menu("ID_AcceptMenu", title, subtitle, noExit: !backCloseMenu, backCloseMenu: backCloseMenu);
            accept.menu.ItemSelectCallbackAsync = accept.MenuCallBack;

            if (banner != null)
                accept.menu.BannerSprite = banner;

            accept.menu.Items.AddRange(new List<MenuItem>() {
                new MenuItem("Accepter", acceptDesc, "ID_Accept", true, false, rightlabel),
                new MenuItem("Refuser", refuseDesc, "ID_Refuser", true)
            });

            Task.Run(async () => { await accept.menu.OpenMenu(client); }).Wait();
            return accept;
        }
        #endregion

        #region Callback
        private async Task MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
                await AcceptMenuCallBack?.Invoke(client, false);
            else if (menu.Id == "ID_AcceptMenu")
            {
                if (menuItem.Id == "ID_Accept")
                    await AcceptMenuCallBack?.Invoke(client, true);
                else if (menuItem.Id == "ID_Refuser")
                    await AcceptMenuCallBack?.Invoke(client, false);
            }

            if (_closeAtEnd)
                await menu.CloseMenu(client);
        }
        #endregion
    }
}
