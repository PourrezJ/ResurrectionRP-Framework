using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Loader;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Utils;
using System.Drawing;
using AltV.Net.Async;

namespace ResurrectionRP_Server.Business
{
    public class ClothingStore : Business
    {
        #region Fields
        private IColShape _clothingColShape;
        private string _componentName;

        public Vector3 ClothingPos;
        public Banner BannerStyle;

        #region All
        public List<int> Mask;
        public List<int> BackPack;
        #endregion

        #region Men
        public List<int> MenLegs;
        public List<int> MenFeet;
        public List<int> MenGlove;
        public List<int> MenUnderShirt;
        public List<int> MenTops;
        public List<int> MenAccessories;
        #endregion

        #region Girl
        public List<int> GirlLegs;
        public List<int> GirlFeet;
        public List<int> GirlGlove;
        public List<int> GirlUnderShirt;
        public List<int> GirlTops;
        public List<int> GirlAccessories;
        #endregion
        #endregion

        #region Constructor
        public ClothingStore(string businnessName, Location location, uint blipSprite, int inventoryMax, Vector3 clothingPos, PedModel pedhash = 0, string owner = null, int bannerStyle = 0) : base(businnessName, location, blipSprite, inventoryMax, pedhash, owner)
        {
            ClothingPos = clothingPos;

            switch (bannerStyle)
            {
                case 0:
                    BannerStyle = Banner.LowFashion;
                    break;

                case 1:
                    BannerStyle = Banner.LowFashion2;
                    break;

                case 2:
                    BannerStyle = Banner.MidFashion;
                    break;

                case 3:
                    BannerStyle = Banner.HighFashion;
                    break;
            }
        }
        #endregion

        #region Init
        public override void Init()
        {
            base.Init();

            if (Mask == null)
                Mask = new List<int>();

            if (BackPack == null)
                BackPack = new List<int>();

            if (MenLegs == null)
                MenLegs = new List<int>();

            if (MenFeet == null)
                MenFeet = new List<int>();

            if (MenGlove == null)
                MenGlove = new List<int>();

            if (MenUnderShirt == null)
                MenUnderShirt = new List<int>();

            if (MenTops == null)
                MenTops = new List<int>();

            if (MenAccessories == null)
                MenAccessories = new List<int>();

            if (GirlLegs == null)
                GirlLegs = new List<int>();

            if (GirlFeet == null)
                GirlFeet = new List<int>();

            if (GirlGlove == null)
                GirlGlove = new List<int>();

            if (GirlUnderShirt == null)
                GirlUnderShirt = new List<int>();

            if (GirlTops == null)
                GirlTops = new List<int>();

            if (GirlAccessories == null)
                GirlAccessories = new List<int>();

            _clothingColShape = Alt.CreateColShapeCylinder(ClothingPos - new Vector3(0, 0, 1), 4f, 3f);
            Marker.CreateMarker(MarkerType.VerticalCylinder, ClothingPos - new Vector3(0, 0, 1), new Vector3(3, 3, 0.3f), Color.FromArgb(80, 255, 255, 255));
            Entities.Blips.BlipsManager.SetColor(Blip, 25);

            _clothingColShape.SetOnPlayerEnterColShape(OnPlayerEnterColShape);
            _clothingColShape.SetOnPlayerLeaveColShape(OnPlayerLeaveColShape);
            _clothingColShape.SetOnPlayerInteractInColShape(OnPlayerInteractInColShape);
        }
        #endregion

        #region Event handlers
        public void OnPlayerEnterColShape(IColShape colShape, IPlayer client)
        {
            client.DisplayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir", 5000);
        }

        public virtual void OnPlayerLeaveColShape(IColShape colShape, IPlayer client)
        {
            PlayerHandler player = client.GetPlayerHandler();

            if (player == null)
                return;

            if (player.HasOpenMenu())
                MenuManager.CloseMenu(client);
        }

        private void OnPlayerInteractInColShape(IColShape colShape, IPlayer client)
        {
            if (!client.Exists)
                return;

            OpenClothingMenu(client);
        }
        #endregion

        #region Private methods
        private void MenuClose(IPlayer client, Menu menu)
        {
            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            ph.Clothing.UpdatePlayerClothing();
        }

        private void BuyCloth(IPlayer client, byte componentID, int drawable, int variation, double price, string clothName)
        {
            ClothItem item = null;
            ClothManifest? clothdata = ClothingLoader.FindCloths(client, componentID) ?? null;

            if (clothdata == null)
                return;

            switch (componentID)
            {
                case 1:
                    item = new ClothItem(ItemID.Mask, clothdata.Value.DrawablesList[drawable].Variations[variation].Gxt, "", new ClothData(drawable, variation, 0), 0, true, false, false, true, false, 0, classes: "mask", icon: "mask");
                    break;

                case 2:
                    break;

                case 3:
                    break;

                case 4:
                    item = new ClothItem(ItemID.Pant, clothdata.Value.DrawablesList[drawable].Variations[variation].Gxt, "", new ClothData(drawable, variation, 0), 0, true, false, false, true, false, 0, classes: "pants", icon: "pants");
                    break;

                case 5:
                    break;

                case 6:
                    item = new ClothItem(ItemID.Shoes, clothdata.Value.DrawablesList[drawable].Variations[variation].Gxt, "", new ClothData(drawable, variation, 0), 0, true, false, false, true, false, 0, classes: "shoes", icon: "shoes");
                    break;

                case 7:
                    // item = new ClothItem(ItemID., clothdata.Value.DrawablesList[drawable].Variations[(byte)variation].Gxt, "", new ClothData(drawable, variation, 0), 0, true, false, false, true, false, 0, classes: "", icon: "");
                    break;

                case 8:
                    item = new ClothItem(ItemID.TShirt, clothdata.Value.DrawablesList[drawable].Variations[variation].Gxt, "", new ClothData(drawable, variation, 0), 0, true, false, false, true, false, 0, classes: "shirt", icon: "shirt");
                    break;
            }

            BuyCloth(client, item, price, clothName);
        }

        private void BuyCloth(IPlayer client, ClothItem item, double price, string clothName)
        {
            var ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if (ph.BankAccount.Balance >= price)
            {
                if (ph.AddItem(item, 1) && ph.HasBankMoney(price, $"Achat vêtement {clothName}", false))
                {
                    client.SendNotificationSuccess($"Vous avez acheté le vêtement {clothName} pour la somme de ${price}");
                    ph.UpdateFull();
                }
                else
                    client.SendNotificationError("Vous n'avez pas la place pour cette élément.");
            }
            else
                client.SendNotificationError("Vous n'avez pas assez d'argent sur votre compte en banque.");
        }
        #endregion

        #region Admin menu
        public override async Task<Menu> OpenSellMenu(IPlayer client, Menu menu)
        {
            if (client.GetPlayerHandler().StaffRank >= Utils.Enums.AdminRank.Moderator)
            {
                menu.ItemSelectCallbackAsync += AdminMenuCallback;
                menu.Add(new MenuItem("~r~Gérer les catégories en vente", "", "ID_Components", true));
            }

            return await base.OpenSellMenu(client, menu);
        }

        public void OpenComponentsMenu(IPlayer client, Menu menu)
        {
            menu.ClearItems();
            menu.BackCloseMenu = false;
            menu.ResetData("Categories");
            menu.SubTitle = "Composants";

            MenuItem menuItem = new MenuItem("Accessoires homme", "", "ID_Component", true);
            menuItem.SetData("Categories", MenAccessories);
            menu.Add(menuItem);

            menuItem = new MenuItem("Accessoires femme", "", "ID_Component", true);
            menuItem.SetData("Categories", GirlAccessories);
            menu.Add(menuItem);

            menuItem = new MenuItem("Chaussures homme", "", "ID_Component", true);
            menuItem.SetData("Categories", MenFeet);
            menu.Add(menuItem);

            menuItem = new MenuItem("Chaussures femme", "", "ID_Component", true);
            menuItem.SetData("Categories", GirlFeet);
            menu.Add(menuItem);

            menuItem = new MenuItem("Gants homme", "", "ID_Component", true);
            menuItem.SetData("Categories", MenGlove);
            menu.Add(menuItem);

            menuItem = new MenuItem("Gants femme", "", "ID_Component", true);
            menuItem.SetData("Categories", GirlGlove);
            menu.Add(menuItem);

            menuItem = new MenuItem("Hauts homme", "", "ID_Component", true);
            menuItem.SetData("Categories", MenTops);
            menu.Add(menuItem);

            menuItem = new MenuItem("Hauts femme", "", "ID_Component", true);
            menuItem.SetData("Categories", GirlTops);
            menu.Add(menuItem);

            menuItem = new MenuItem("Pantalons homme", "", "ID_Component", true);
            menuItem.SetData("Categories", MenLegs);
            menu.Add(menuItem);

            menuItem = new MenuItem("Pantalons femme", "", "ID_Component", true);
            menuItem.SetData("Categories", GirlLegs);
            menu.Add(menuItem);

            menuItem = new MenuItem("T-shirt homme", "", "ID_Component", true);
            menuItem.SetData("Categories", MenUnderShirt);
            menu.Add(menuItem);

            menuItem = new MenuItem("T-shirt femme", "", "ID_Component", true);
            menuItem.SetData("Categories", GirlUnderShirt);
            menu.Add(menuItem);

            menuItem = new MenuItem("Masques", "", "ID_Component", true);
            menuItem.SetData("Categories", Mask);
            menu.Add(menuItem);

            menuItem = new MenuItem("Sacs à dos", "", "ID_Component", true);
            menuItem.SetData("Categories", BackPack);
            menu.Add(menuItem);

            menu.OpenMenu(client);
        }

        public async Task AdminMenuCallback(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null && menu.HasData("Categories"))
            {
                OpenComponentsMenu(client, menu);
                return;
            }
            else if (menuItem == null)
            {
                await OnNpcSecondaryInteract(client, Ped);
                return;
            }

            if (menuItem.Id == "ID_Components")
            {
                OpenComponentsMenu(client, menu);
            }
            else if (menuItem.Id == "ID_Component")
            {
                menu.ClearItems();
                menu.SubTitle = menuItem.Text;
                menu.SetData("Categories", menuItem.GetData("Categories"));

                MenuItem item = new MenuItem("~r~Lister les catégories", "", "ID_ListCategory", true);
                menu.Add(item);

                item = new MenuItem("~r~Ajouter une catégorie", "", "ID_AddCategory", true);
                item.InputType = InputType.UNumber;
                item.InputMaxLength = 3;
                menu.Add(item);

                item = new MenuItem("~r~Retirer une catégorie", "", "ID_RemCategory", true);
                item.InputType = InputType.UNumber;
                item.InputMaxLength = 3;
                menu.Add(item);

                menu.OpenMenu(client);
            }
            else if (menuItem.Id == "ID_ListCategory")
            {
                List<int> categories = menu.GetData("Categories");

                string message = $"{menu.SubTitle}: ";

                if (categories != null)
                {
                    for (int i = 0; i < categories.Count; i++)
                    {
                        if (i != 0)
                            message += ", ";

                        message += categories[i];
                    }
                }

                client.SendChatMessage(message);
            }
            else if (menuItem.Id == "ID_AddCategory")
            {
                if (!int.TryParse(menuItem.InputValue, out int category))
                    return;

                List<int> categories = menu.GetData("Categories");

                if (categories.Contains(category))
                {
                    client.SendNotificationError($"Catégorie {category} déjà présente");
                    return;
                }

                categories.Add(category);
                categories.Sort();
                UpdateInBackground();
                client.SendNotificationSuccess($"Catégorie {category} ajoutée");
            }
            else if (menuItem.Id == "ID_RemCategory")
            {
                if (!int.TryParse(menuItem.InputValue, out int category))
                    return;

                List<int> categories = menu.GetData("Categories");

                if (!categories.Contains(category))
                {
                    client.SendNotificationError($"Catégorie {category} non présente");
                    return;
                }

                categories.Remove(category);
                UpdateInBackground();
                client.SendNotificationSuccess($"Catégorie {category} retirée");
            }
        }
        #endregion

        #region Main Menu
        public void OpenClothingMenu(IPlayer client)
        {
            Menu menu = new Menu("ClothingMenu", "", "Que souhaitez-vous acheter?", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true, BannerStyle);
            menu.ItemSelectCallback = MenuCallBack;
            menu.Finalizer = MenuClose;

            if (Mask != null && Mask.Count > 0)
            {
                OpenComponentMenuWithoutCat(client, menu, 1, true);
                return;
            }

            if (client.Model == (uint)PedModel.FreemodeMale01)
            {
                if (MenTops != null && MenTops.Count > 0)
                    menu.Add(new MenuItem("Haut", "", "ID_Haut", true));

                if (MenUnderShirt != null && MenUnderShirt.Count > 0)
                    menu.Add(new MenuItem("T-Shirt", "", "ID_TShirt", true));

                if (MenLegs != null && MenLegs.Count > 0)
                    menu.Add(new MenuItem("Pantalon", "", "ID_Pantalon", true));

                if (MenFeet != null && MenFeet.Count > 0)
                    menu.Add(new MenuItem("Chaussure", "", "ID_Chaussure", true));

                if (MenAccessories != null && MenAccessories.Count > 0)
                    menu.Add(new MenuItem("Accessoire", "", "ID_Accessoire", true));
            }
            else if (client.Model == (uint)PedModel.FreemodeFemale01)
            {
                if (GirlTops != null && GirlTops.Count > 0)
                    menu.Add(new MenuItem("Haut", "", "ID_Haut", true));

                if (GirlUnderShirt != null && GirlUnderShirt.Count > 0)
                    menu.Add(new MenuItem("T-Shirt", "", "ID_TShirt", true));

                if (GirlLegs != null && GirlLegs.Count > 0)
                    menu.Add(new MenuItem("Pantalon", "", "ID_Pantalon", true));

                if (GirlFeet != null && GirlFeet.Count > 0)
                    menu.Add(new MenuItem("Chaussure", "", "ID_Chaussure", true));

                if (GirlAccessories != null && GirlAccessories.Count > 0)
                    menu.Add(new MenuItem("Accessoire", "", "ID_Accessoire", true));
            }
            else
                client.SendNotificationError("Vous n'êtes pas autorisé à utiliser la boutique de vêtements avec ce skin.");

             menu.OpenMenu(client);
        }

        private void MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            _componentName = menuItem.Text.ToUpper();

            switch (menuItem.Id)
            {
                case "ID_Haut":
                    OpenHautMenu(client, menu);
                    break;

                case "ID_Pantalon":
                    OpenComponentMenu(client, menu, 4);
                    break;

                case "ID_TShirt":
                    OpenComponentMenuWithoutCat(client, menu, 8, false);
                    break;

                case "ID_Chaussure":
                    OpenComponentMenu(client, menu, 6);
                    break;

                case "ID_Accessoire":
                    OpenComponentMenu(client, menu, 7);
                    break;
            }
        }
        #endregion

        #region Tops
        public void OpenHautMenu(IPlayer client, Menu menu)
        {
            menu.ClearItems();
            menu.BackCloseMenu = false;
            menu.SubTitle = _componentName;
            var data = ClothingLoader.FindTops(client) ?? null;

            if (data == null)
                return;

            menu.ItemSelectCallback = OnTopsCategorieCallBack;
            menu.IndexChangeCallback = null;
            var compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenTops : GirlTops;

            foreach (var compo in compoList)
            {
                if (compo == 220 || compo == 218 || compo == 157) // Les vêtements de la Milice sont blacklisteds + add vetement biker le cuir
                    continue;

                var drawables = data.Value.DrawablesList[compo];

                for (int b = 0; b < drawables.Variations.Count; b++)
                {
                    if (!menu.Items.Exists(p => p.Text == drawables.Categorie))
                    {
                        if (drawables.Categorie == "NULL")
                            continue;

                        var ui = new MenuItem(drawables.Categorie, executeCallback: true);
                        menu.Add(ui);
                    }
                }
            }

            menu.OpenMenu(client);
        }

        private void OnTopsCategorieCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                OpenClothingMenu(client);
                return;
            }

            menu.ClearItems();
            var compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenTops : GirlTops;
            menu.SubTitle = menuItem.Text.ToUpper();
            menu.ItemSelectCallback = OnTopsCallBack;
            menu.IndexChangeCallback = PreviewTopsItem;

            ClothManifest? clothdata = ClothingLoader.FindTops(client);

            if (clothdata == null)
                return;

            // compoList c'est l'int array correspondant aux items d'un certains type (short, pants ... ) disponible en boutique.
            foreach (var drawable in compoList)
            {
                var drawables = clothdata.Value.DrawablesList[drawable]; // recup des données sur ces tenues

                if (drawables.Categorie != menuItem.Text)
                    continue;

                for (int b = 0; b < drawables.Variations.Count; b++)
                {
                    var variation = drawables.Variations[b];

                    if (variation.Gxt == "NULL")
                        continue;

                    var ui = new MenuItem(variation.Gxt, executeCallback: true);
                    ui.RightLabel = $"${ drawables.Price}";
                    ui.SetData("drawable", drawable);
                    ui.SetData("variation", b);
                    ui.SetData("price", drawables.Price);
                    ui.SetData("name", variation.Gxt);
                    ui.SetData("torso", drawables.Torso[0]);
                    menu.Add(ui);
                }
            }

            menu.OpenMenu(client);
            PreviewTopsItem(client, menu, 0, menu.Items[0]);
        }

        private void PreviewTopsItem(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            try
            {
                int drawable = (int)menuItem.GetData("drawable");
                int variation = (int)menuItem.GetData("variation");
                int torso = (int)menuItem.GetData("torso");

                client.SetCloth(ClothSlot.Tops, drawable, variation, 0);
                client.SetCloth(ClothSlot.Torso, torso, 0, 0);
            }
            catch(Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
            }
        }

        private void OnTopsCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                OpenHautMenu(client, menu);
                return;
            }

            int drawable = menuItem.GetData("drawable");
            int variation = menuItem.GetData("variation");
            double price = menuItem.GetData("price");

            ClothItem item = new ClothItem(ItemID.Jacket, menuItem.Text, "", new ClothData(drawable, variation, 0), 0.2, true, false, false, true, false, classes: "jacket", icon: "jacket");
            BuyCloth(client, item, price, menuItem.Text);
        }
        #endregion

        #region WithCategorie
        public void OpenComponentMenu(IPlayer client, Menu menu, byte componentID)
        {
            ClothManifest? data = ClothingLoader.FindCloths(client, componentID) ?? null;

            if (data == null)
                return;

            menu.ClearItems();
            menu.SubTitle = _componentName;
            menu.BackCloseMenu = false;
            menu.ItemSelectCallback = CategorieCallBack;
            menu.IndexChangeCallback = null;

            List<int> compoList = null;

            switch (componentID)
            {
                case 1:
                    compoList = Mask;
                    break;

                case 3:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenGlove : GirlGlove;
                    break;

                case 4:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenLegs : GirlLegs;
                    break;

                case 5:
                    compoList = BackPack;
                    break;

                case 6:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenFeet : GirlFeet;
                    break;

                case 7:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenAccessories : GirlAccessories;
                    break;

                case 11:
                    compoList = MenTops;
                    break;
            }

            if (compoList == null)
                return;

            foreach (int compo in compoList)
            {
                ClothDrawable drawables = data.Value.DrawablesList[compo];

                for (int b = 0; b < drawables.Variations.Count; b++)
                {
                    if (!menu.Items.Exists(p => p.Text == drawables.Categorie))
                    {
                        if (drawables.Categorie == null || drawables.Categorie == "NULL")
                            continue;

                        MenuItem ui = new MenuItem(drawables.Categorie, executeCallback: true);
                        menu.Add(ui);
                    }
                }
            }

            menu.SetData("componentID", componentID);
            menu.SetData("Categorie", compoList);
            menu.OpenMenu(client);
        }

        private void CategorieCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                OpenClothingMenu(client);
                return;
            }

            menu.ClearItems();
            List<int> compoList = menu.GetData("Categorie");

            menu.SubTitle = menuItem.Text.ToUpper();
            menu.BackCloseMenu = false;
            menu.ItemSelectCallback = OnCallBackWithCat;
            menu.IndexChangeCallback = OnCurrentItem;

            byte componentID = menu.GetData("componentID");

            ClothManifest? clothdata = ClothingLoader.FindCloths(client, componentID);

            // compoList c'est l'int array correspondant aux items d'un certains type (short, pants ... ) disponible en boutique.
            foreach (int drawable in compoList)
            {
                if (!clothdata.Value.DrawablesList.ContainsKey(drawable))
                    continue; 

                ClothDrawable drawables = clothdata.Value.DrawablesList[drawable]; // recup des données sur ces tenues

                if (drawables.Categorie != menuItem.Text)
                    continue;

                foreach (KeyValuePair<int, ClothVariation> pair in drawables.Variations)
                {
                    ClothVariation variation = pair.Value;

                    if (variation.Gxt == "NULL" || variation.Gxt == null)
                        continue;

                    MenuItem ui = new MenuItem(variation.Gxt, executeCallback: true);
                    ui.RightLabel = $"${ drawables.Price}";
                    ui.SetData("drawable", drawable);
                    ui.SetData("variation", pair.Key);
                    ui.SetData("price", drawables.Price);
                    menu.Add(ui);
                }
            }

            if (menu.Items.Count == 0)
            {
                OpenComponentMenu(client, menu, componentID);
                return;
            }

            menu.OpenMenu(client);
            OnCurrentItem(client, menu, 0, menu.Items[0]);
        }

        private void OnCallBackWithCat(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                byte compID = menu.GetData("componentID");
                OpenComponentMenu(client, menu, compID);
                return;
            }

            byte componentID = menu.GetData("componentID");
            int drawable = menuItem.GetData("drawable");
            int variation = menuItem.GetData("variation");
            double price = menuItem.GetData("price");
            string clothName = menuItem.Text;
            BuyCloth(client, componentID, drawable, variation, price, clothName);
        }

        private void OnCurrentItem(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            try
            {
                byte componentID = menu.GetData("componentID");
                int drawable = menuItem.GetData("drawable");
                int variation = menuItem.GetData("variation");
                client.SetCloth((ClothSlot)componentID, drawable, variation, 0);
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("OnCurrentItem" + ex);
            }
        }
        #endregion

        #region WithoutCategorie
        private void OpenComponentMenuWithoutCat(IPlayer client, Menu menu, byte componentID, bool backCloseMenu)
        {
            ClothManifest? data = ClothingLoader.FindCloths(client, componentID) ?? null;

            if (data == null)
                return;

            menu.ClearItems();
            menu.SubTitle = _componentName;
            menu.BackCloseMenu = backCloseMenu;
            menu.ItemSelectCallback = OnCallBackWithoutCat;
            menu.IndexChangeCallback = OnCurrentItem;

            List<int> compoList = null;

            switch (componentID)
            {
                case 1:
                    compoList = Mask;
                    break;

                case 8:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenUnderShirt : GirlUnderShirt;
                    break;
            }

            foreach (int compo in compoList)
            {
                if (!data.Value.DrawablesList.ContainsKey(compo))
                    continue;

                ClothDrawable drawables = data.Value.DrawablesList[compo];
                int price = drawables.Price;

                foreach (KeyValuePair<int, ClothVariation> pair in drawables.Variations)
                {
                    string name = drawables.Variations[pair.Key].Gxt;

                    if (name == "NULL")
                        continue;

                    MenuItem ui = new MenuItem(drawables.Variations[pair.Key].Gxt, executeCallback: true);
                    ui.RightLabel = $"${price}";
                    ui.SetData("drawable", compo);
                    ui.SetData("variation", pair.Key);
                    ui.SetData("price", price);
                    menu.Add(ui);
                }
            }

            if (menu.Items.Count == 0)
            {
                OpenComponentMenu(client, menu, componentID);
                return;
            }

            menu.SetData("componentID", componentID);
            menu.OpenMenu(client);
            OnCurrentItem(client, menu, 0, menu.Items[0]);
        }

        private void OnCallBackWithoutCat(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                OpenClothingMenu(client);
                return;
            }

            byte componentID = menu.GetData("componentID");
            int drawable = menuItem.GetData("drawable");
            int variation = menuItem.GetData("variation");
            double price = menuItem.GetData("price");
            string clothName = menuItem.Text;

            BuyCloth(client, componentID, drawable, variation, price, clothName);
        }
        #endregion
    }
}
