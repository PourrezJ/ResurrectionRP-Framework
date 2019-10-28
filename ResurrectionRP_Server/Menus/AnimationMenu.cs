using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using ResurrectionRP_Server.Utils.Enums;
using System.Linq;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Menus
{
    class AnimationsMenu
    {
        #region Private fields
        private int _keySelected;
        private Menu _menu;
        #endregion

        #region Constructor
        public AnimationsMenu()
        { }
        #endregion

        #region Menus
        public static void OpenAnimationsMenu(IPlayer client)
        {
            var animationsMenu = new AnimationsMenu();
            animationsMenu._menu = new Menu("Animation", "Réglages", "Choisissez une touche :", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR);
            animationsMenu._menu.ItemSelectCallback = animationsMenu.AnimationMenuCallBack;

            for (int i = 1; i <= 9; i++)
                animationsMenu._menu.Add(new MenuItem($"Touche {i}", $"Configurer une animation sur la touche {i} du pavé numérique.", "ID_Key", true));

            MenuManager.OpenMenu(client, animationsMenu._menu);
        }

        private void OpenCategoriesMenu(IPlayer client, Menu menu)
        {
            menu.ClearItems();
            menu.Id = "AnimCategories";
            menu.SubTitle = $"Catégorie pour la touche {_keySelected + 1} : ";
            PlayerHandler ph = client.GetPlayerHandler();

            if (ph != null)
            {
                foreach (var animData in AnimationsList.AnimList)
                {
                    if (!menu.Items.Exists(p => p.Text == animData.CategorieName))
                        menu.Add(new MenuItem(animData.CategorieName, "", "ID_Cat", executeCallback: true));
                }

                menu.OpenMenu(client);
            }
        }
        #endregion

        #region Callback
        private void AnimationMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null && menu.Id == "Animation")
            {
                client.GetPlayerHandler()?.OpenPlayerMenu();
                return;
            }
            else if (menuItem == null && menu.Id == "AnimCategories")
            {
                OpenAnimationsMenu(client);
                return;
            }
            else if (menuItem == null && menu.Id == "AnimKeyBiding")
            {
                OpenCategoriesMenu(client, menu);
                return;
            }

            PlayerHandler ph = client.GetPlayerHandler();

            // Callback du choix de la touche
            if (menuItem.Id == "ID_Key")
            {
                _keySelected = itemIndex;
                OpenCategoriesMenu(client, menu);
            }
            else if (menuItem.Id == "ID_Cat")
            {
                menu.SubTitle = menuItem.Text;
                menu.ClearItems();
                menu.Id = "AnimKeyBiding";

                if (ph != null)
                {
                    foreach (var animData in AnimationsList.AnimList.Where(p=>p.CategorieName == menuItem.Text))
                    {
                        var item = new MenuItem(animData.Animation.Name, "", "ID_Anim", executeCallback: true);
                        item.SetData("Anim", animData.Animation);
                        menu.Add(item);
                    }
                    menu.OpenMenu(client);
                }
            }
            // Callback du choix de l'animation
            else if (menuItem.Id == "ID_Anim")
            {
                Animation anim = menuItem.GetData("Anim");

                if (anim != null)
                {
                    anim.AnimName.ToLower();
                    anim.AnimDict.ToLower();
                    ph.AnimSettings[_keySelected] = anim;
                    client.PlayAnimation(anim.AnimDict, anim.AnimName, 8, -1, 5000, (AnimationFlags)1);
                }  
            }
        }
        #endregion
    }
}