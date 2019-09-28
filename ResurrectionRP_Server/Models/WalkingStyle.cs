using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Players.Data;
using ResurrectionRP_Server.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Models
{
    public class WalkingStyleMenu
    {
        #region Static fields
        // names & style names taken from https://github.com/Guad/EnhancedInteractionMenu/blob/master/EnhancedInteractionMenu/pi_menu.cs
        public static List<WalkingStyle> WalkingStyles = new List<WalkingStyle>
        {
            new WalkingStyle("Normal", null),
            new WalkingStyle("Brave", "move_m@brave"),
            new WalkingStyle("Confident", "move_m@confident"),
            //new WalkingStyle("Drunk", "move_m@drunk@verydrunk"),
            new WalkingStyle("Fat", "move_m@fat@a"),
            new WalkingStyle("Gangster", "move_m@shadyped@a"),
            new WalkingStyle("Hurry", "move_m@hurry@a"),
            new WalkingStyle("Injured", "move_m@injured"),
            new WalkingStyle("Intimidated", "move_m@intimidation@1h"),
            new WalkingStyle("Quick", "move_m@quick"),
            new WalkingStyle("Sad", "move_m@sad@a"),
            new WalkingStyle("Tough Guy", "move_m@tool_belt@a")
        };
        #endregion

        #region Private fields
        private Menu menu;
        #endregion

        #region Constructor
        public WalkingStyleMenu()
        { }
        #endregion

        #region Menu
        public static void OpenWalkingStyleMenu(IPlayer client)
        {
            var walkmenu = new WalkingStyleMenu()
            {
                menu = new Menu("ID_WalkingMenu", "Style de marche", "Choisissez une option :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR)
            };

            walkmenu.menu.ItemSelectCallbackAsync = walkmenu.WalkingStyleMenuCallBack;

            foreach (WalkingStyle walk in WalkingStyles)
            {
                walkmenu.menu.Add(new MenuItem(walk.Name, "", "ID_Walk", true));
            }

            walkmenu.menu.OpenMenu(client);
        }
        #endregion

        #region Callback
        private async Task WalkingStyleMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                await client.GetPlayerHandler()?.OpenPlayerMenu();
                return;
            }

            string anim = WalkingStyles.Find(x => x.Name == menuItem.Text).AnimName ?? "";
            /*
            if (string.IsNullOrEmpty(anim))
                await PlayerManager.GetPlayerByClient(client)?.ResetWalkingStyle();
            else
                await PlayerManager.GetPlayerByClient(client)?.SetWalkingStyle(anim);
            */
        }
        #endregion
    }
}