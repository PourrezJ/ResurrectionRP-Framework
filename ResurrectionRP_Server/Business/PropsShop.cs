
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.EventHandlers;
using ResurrectionRP_Server.Loader;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Models.InventoryData;
using ResurrectionRP_Server.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Business
{
    public class PropsStore : Business
    {
        #region Fields
        public Vector3 ClothingPos;

        [BsonIgnore]
        private IColShape _clothingColShape;

        public Banner BannerStyle;

        public int[] MenHats;
        public int[] GirlHats;

        public int[] MenGlasses;
        public int[] GirlGlasses;

        public int[] MenEars;
        public int[] GirlEars;

        public int[] MenWatches;
        public int[] GirlWatches;

        public int[] MenBracelets;
        public int[] GirlBracelets;
        #endregion

        #region Constructor
        public PropsStore(string businnessName, Models.Location location, uint blipSprite, int inventoryMax, Vector3 clothingPos, PedModel pedhash = 0, string owner = null, int bannerStyle = 0) : base(businnessName, location, blipSprite, inventoryMax, pedhash, owner)
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
        public override async Task Init()
        {
            MaxEmployee = 5;
            await base.Init();
            _clothingColShape = Alt.CreateColShapeCylinder(ClothingPos - new Vector3(0, 0, 1), 4f, 3f);
            Marker.CreateMarker(MarkerType.VerticalCylinder, ClothingPos - new Vector3(0, 0, 1), new Vector3(3, 3, 0.3f), Color.FromArgb(80, 255, 255, 255));
            Entities.Blips.BlipsManager.SetColor(Blip, 25);

            _clothingColShape.SetOnPlayerEnterColShape(OnPlayerEnterColShape);
            _clothingColShape.SetOnPlayerLeaveColShape(OnPlayerLeaveColShape);
            _clothingColShape.SetOnPlayerInteractInColShape(OnPlayerInteractInColShape);
        }
        #endregion

        #region Event handlers
        private Task OnPlayerInteractInColShape(IColShape colShape, IPlayer client)
        {
            if (!client.Exists)
                return Task.CompletedTask;

            OpenPropsStoreMenu(client);
            return Task.CompletedTask;
        }

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
        #endregion

        #region Private methods
        private Task MenuClose(IPlayer client, Menu menu)
        {
            var ph = client.GetPlayerHandler();

            if (ph == null)
                return Task.CompletedTask;

            ph.Clothing.UpdatePlayerClothing();
            return Task.CompletedTask;
        }

        private async Task BuyProp(IPlayer client, byte componentID, int drawable, int variation, double price, string clothName)
        {
            Item item = null;
            ClothManifest? propData = ClothingLoader.FindProps(client, componentID) ?? null;

            if (propData == null)
                return;

            switch (componentID)
            {
                case 0:
                    item = new ClothItem(ItemID.Hats, propData.Value.DrawablesList[drawable].Variations[(byte)variation].Gxt, "", new ClothData((byte)drawable, (byte)variation, 0), 0, true, false, false, true, false, 0, classes: "cap", icon: "cap");
                    break;

                case 1:
                    item = new ClothItem(ItemID.Glasses, propData.Value.DrawablesList[drawable].Variations[(byte)variation].Gxt, "", new ClothData((byte)drawable, (byte)variation, 0), 0, true, false, false, true, false, 0, classes: "glasses", icon: "glasses");
                    break;

                case 2:
                    item = new ClothItem(ItemID.Ears, propData.Value.DrawablesList[drawable].Variations[(byte)variation].Gxt, "", new ClothData((byte)drawable, (byte)variation, 0), 0, true, false, false, true, false, 0, classes: "earring", icon: "earring");
                    break;

                case 6:
                    item = new ClothItem(ItemID.Watch, propData.Value.DrawablesList[drawable].Variations[(byte)variation].Gxt, "", new ClothData((byte)drawable, (byte)variation, 0), 0, true, false, false, true, false, 0, classes: "watch", icon: "watch");
                    break;

                case 7:
                    item = new ClothItem(ItemID.Bracelet, propData.Value.DrawablesList[drawable].Variations[(byte)variation].Gxt, "", new ClothData((byte)drawable, (byte)variation, 0), 0, true, false, false, true, false, 0, classes: "bracelet", icon: "bracelet");
                    break;
            }

            PlayerHandler ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            if (ph.BankAccount.Balance >= price)
            {
                if (ph.AddItem(item, 1))
                {
                    if (await ph.HasBankMoney(price, $"Achat {clothName}"))
                    {
                        client.SendNotificationSuccess($"Vous avez acheté {clothName} pour la somme de ${price}");
                        ph.UpdateFull();
                    }
                }
                else
                    client.SendNotificationError("Vous n'avez pas la place pour cette élément.");
            }
            else
                client.SendNotificationError("Vous n'avez pas assez d'argent sur votre compte en banque.");
        }
        #endregion

        #region Menu
        public void OpenPropsStoreMenu(IPlayer client)
        {
            Menu menu = new Menu("ClothingMenu", "", "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, false, true, true, BannerStyle);
            menu.ItemSelectCallback = MenuCallBack;
            menu.FinalizerAsync = MenuClose;

            if (client.Model == (uint)PedModel.FreemodeMale01)
            {
                if (MenHats != null && MenHats.Length > 0)
                    menu.Add(new MenuItem("Chapeaux", "", "ID_Hats", true));

                if (MenGlasses != null && MenGlasses.Length > 0)
                    menu.Add(new MenuItem("Lunettes", "", "ID_Glasses", true));

                if (MenEars != null && MenEars.Length > 0)
                    menu.Add(new MenuItem("Boucle Oreilles", "", "ID_Ears", true));

                if (MenWatches != null && MenWatches.Length > 0)
                    menu.Add(new MenuItem("Montres", "", "ID_Watches", true));

                if (MenBracelets != null && MenBracelets.Length > 0)
                    menu.Add(new MenuItem("Bracelets", "", "ID_Bracelets", true));
            }
            else if (client.Model == (uint)PedModel.FreemodeFemale01)
            {
                if (GirlHats != null && GirlHats.Length > 0)
                    menu.Add(new MenuItem("Chapeaux", "", "ID_Hats", true));

                if (GirlGlasses != null && GirlGlasses.Length > 0)
                    menu.Add(new MenuItem("Lunettes", "", "ID_Glasses", true));

                if (GirlEars != null && GirlEars.Length > 0)
                    menu.Add(new MenuItem("Boucle Oreilles", "", "ID_Ears", true));

                if (GirlWatches != null && GirlWatches.Length > 0)
                    menu.Add(new MenuItem("Montres", "", "ID_Watches", true));

                if (GirlBracelets != null && GirlBracelets.Length > 0)
                    menu.Add(new MenuItem("Bracelets", "", "ID_Bracelets", true));
            }
            else
                client.SendNotificationError("Vous n'êtes pas autorisé à utiliser la boutique de vêtements avec ce skin.");

            menu.OpenMenu(client);
        }

        private void MenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            switch (menuItem.Id)
            {
                case "ID_Hats":
                    OpenComponentMenu(client, menu, 0);
                    break;

                case "ID_Glasses":
                    OpenComponentMenu(client, menu, 1);
                    break;

                case "ID_Ears":
                    OpenComponentMenuWithoutCat(client, menu, 2);
                    break;

                case "ID_Watches":
                    OpenComponentMenu(client, menu, 6);
                    break;

                case "ID_Bracelets":
                    OpenComponentMenu(client, menu, 7);
                    break;
            }
        }
        #endregion

        #region WithCategorie
        public void OpenComponentMenu(IPlayer client, Menu menu, byte componentID)
        {
            ClothManifest? data = ClothingLoader.FindProps(client, componentID) ?? null;

            if (data == null)
                return;

            menu.ClearItems();
            menu.SubTitle = null;
            menu.BackCloseMenu = false;
            menu.ItemSelectCallbackAsync = CategorieCallBack;
            menu.IndexChangeCallbackAsync = null;

            int[] compoList = null;

            switch (componentID)
            {
                case 0:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenHats : GirlHats;
                    break;

                case 1:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenGlasses : GirlGlasses;
                    break;

                case 2:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenEars : GirlEars;
                    break;

                case 6:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenWatches : GirlWatches;
                    break;

                case 7:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenBracelets : GirlBracelets;
                    break;
            }

            if (compoList == null)
                return;

            foreach (int compo in compoList)
            {
                ClothDrawable drawables = data.Value.DrawablesList[compo];
                int price = drawables.Price / 3;

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

        private async Task CategorieCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                OpenPropsStoreMenu(client);
                return;
            }

            menu.ClearItems();

            int[] compoList = menu.GetData("Categorie");
            menu.SubTitle = menuItem.Text.ToUpper();
            menu.BackCloseMenu = false;
            menu.ItemSelectCallbackAsync = OnCallBackWithCat;
            menu.IndexChangeCallbackAsync = OnCurrentItem;

            byte componentID = menu.GetData("componentID");

            ClothManifest? clothdata = ClothingLoader.FindProps(client, componentID);

            // compoList c'est l'int array correspondant aux items d'un certains type (short, pants ... ) disponible en boutique.
            foreach (int drawable in compoList)
            {
                if (!clothdata.Value.DrawablesList.ContainsKey(drawable))
                    continue;

                ClothDrawable drawables = clothdata.Value.DrawablesList[drawable]; // recup des données sur ces tenues
                int price = drawables.Price / 3;

                if (drawables.Categorie != menuItem.Text)
                    continue;

                foreach (KeyValuePair<int, ClothVariation> pair in drawables.Variations)
                {
                    ClothVariation variation = pair.Value;

                    if (variation.Gxt == "NULL" || variation.Gxt == null)
                        continue;

                    MenuItem ui = new MenuItem(variation.Gxt, executeCallback: true);
                    ui.RightLabel = $"${price}";
                    ui.SetData("drawable", drawable);
                    ui.SetData("variation", pair.Key);
                    ui.SetData("price", price);
                    menu.Add(ui);
                }
            }

            menu.OpenMenu(client);
            await OnCurrentItem(client, menu, 0, menu.Items[0]);
        }

        private async Task OnCallBackWithCat(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
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

            await BuyProp(client, componentID, drawable, variation, price, clothName);
        }

        private Task OnCurrentItem(IPlayer client, Menu menu, int itemIndex, IMenuItem menuItem)
        {
            try
            {
                byte componentID = menu.GetData("componentID");
                int drawable = menuItem.GetData("drawable");
                int variation = menuItem.GetData("variation");
                client.SetProp((PropSlot)componentID, new PropData(drawable, variation));
            }
            catch (Exception ex)
            {
                Alt.Server.LogError("OnCurrentItem" + ex);
            }

            return Task.CompletedTask;
        }
        #endregion

        #region WithoutCategorie
        private void OpenComponentMenuWithoutCat(IPlayer client, Menu menu, byte componentID)
        {
            ClothManifest? data = ClothingLoader.FindProps(client, componentID) ?? null;

            if (data == null)
                return;

            menu.ClearItems();
            menu.SubTitle = null;
            menu.BackCloseMenu = false;
            menu.ItemSelectCallbackAsync = OnCallBackWithoutCat;
            menu.IndexChangeCallbackAsync = OnCurrentItem;

            int[] compoList = null;

            switch (componentID)
            {
                case 0:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenHats : GirlHats;
                    break;

                case 1:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenGlasses : GirlGlasses;
                    break;

                case 2:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenEars : GirlEars;
                    break;

                case 6:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenWatches : GirlWatches;
                    break;

                case 7:
                    compoList = (client.Model == (uint)PedModel.FreemodeMale01) ? MenBracelets : GirlBracelets;
                    break;
            }

            if (compoList == null)
                return;

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

            menu.SetData("componentID", componentID);
            menu.OpenMenu(client);
            OnCurrentItem(client, menu, 0, menu.Items[0]);
        }

        private async Task OnCallBackWithoutCat(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            if (menuItem == null)
            {
                OpenPropsStoreMenu(client);
                return;
            }

            byte componentID = menu.GetData("componentID");
            int drawable = menuItem.GetData("drawable");
            int variation = menuItem.GetData("variation");
            double price = menuItem.GetData("price");
            string clothName = menuItem.Text;

            await BuyProp(client, componentID, drawable, variation, price, clothName);
        }
        #endregion
    }
}
