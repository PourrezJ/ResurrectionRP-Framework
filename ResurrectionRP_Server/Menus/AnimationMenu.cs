using AltV.Net.Elements.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
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
        public static async Task OpenAnimationsMenu(IPlayer client)
        {
            var animationsMenu = new AnimationsMenu();
            animationsMenu._menu = new Menu("Animation", "Réglages", "Choisissez une touche :", 0, 0, Menu.MenuAnchor.MiddleRight);
            animationsMenu._menu.Callback = animationsMenu.AnimationMenuCallBack;

            for (int i = 1; i <= 9; i++)
                animationsMenu._menu.Add(new MenuItem($"Touche {i}", $"Configurer une animation sur la touche {i} du pavé numérique.", "ID_Key", true));

            await MenuManager.OpenMenu(client, animationsMenu._menu);
        }

        private async Task OpenCategoriesMenu(IPlayer client, Menu menu)
        {
            menu.ClearItems();
            menu.Id = "AnimCategories";
            menu.SubTitle = $"Catégorie pour la touche {_keySelected + 1} : ";
            PlayerHandler ph = PlayerManager.GetPlayerByClient(client);

            if (ph != null)
            {
                foreach (var animData in AnimationsList.AnimList)
                {
                    if (!menu.Items.Exists(p => p.Text == animData.CategorieName))
                        menu.Add(new MenuItem(animData.CategorieName, "", "ID_Cat", executeCallback: true));
                }

                await menu.OpenMenu(client);
            }
        }
        #endregion

        #region Callback
        private async Task AnimationMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null && menu.Id == "Animation")
            {
                PlayerManager.GetPlayerByClient(client)?.OpenPlayerMenu();
                return;
            }
            else if (menuItem == null && menu.Id == "AnimCategories")
            {
                await OpenAnimationsMenu(client);
                return;
            }
            else if (menuItem == null && menu.Id == "AnimKeyBiding")
            {
                await OpenCategoriesMenu(client, menu);
                return;
            }

            PlayerHandler ph = PlayerManager.GetPlayerByClient(client);

            // Callback du choix de la touche
            if (menuItem.Id == "ID_Key")
            {
                _keySelected = itemIndex;
                await OpenCategoriesMenu(client, menu);
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
                    await menu.OpenMenu(client);
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
                }  
            }
        }
        #endregion
    }
}